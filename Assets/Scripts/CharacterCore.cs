using UnityEngine;
using UnityEngine.AI;

public class CharacterCore : MonoBehaviour
{
    private Rigidbody rb;
    private NavMeshAgent agent;
    public Animator anim { get; private set; }

    [Header("ステータス設定")]
    public float speed = 5.0f;
    public float jumpForce = 5.0f; 

    private Vector3 inputDirection;
    private bool isRunning;
    private bool isHoldingAttack;   
    private bool isAttackTriggered; 
    private bool isJumpTriggered;
    private Transform lockOnTarget;

    private bool hasIsHoldingAttack;
    private bool hasAttackTrigger;
    private bool hasJumpTrigger;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>(); 
        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();

        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "isHoldingAttack") hasIsHoldingAttack = true;
                if (param.name == "Attack") hasAttackTrigger = true;
                if (param.name == "Jump") hasJumpTrigger = true;
            }
        }
    }

    void Update()
    {
        bool isDamaged = false;
        if (anim != null)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            // ⭕ 「Tag」と「考えられる名前のパターン」をすべて網羅
            isDamaged = stateInfo.IsTag("Damage") || stateInfo.IsName("damaged (tired) stance") || stateInfo.IsName("damaged [tired] stance") || stateInfo.IsName("damage") || stateInfo.IsName("root|Take Damage");
        }

        if (isJumpTriggered && !isDamaged && anim != null)
        {
            if (hasJumpTrigger)
            {
                anim.ResetTrigger("Jump");
                anim.SetTrigger("Jump");
            }
            isJumpTriggered = false;
        }

        if (anim != null)
        {
            bool actualAttackInput = isHoldingAttack && !isDamaged;
            if (hasIsHoldingAttack) anim.SetBool("isHoldingAttack", actualAttackInput);

            if (isAttackTriggered && !isDamaged)
            {
                if (hasAttackTrigger)
                {
                    anim.ResetTrigger("Attack");
                    anim.SetTrigger("Attack");
                }
                isAttackTriggered = false; 
            }
            else
            {
                isAttackTriggered = false; 
            }
        }
    }

    void FixedUpdate()
    {
        bool isAttackingState = false;
        bool isDamagedState = false;
        bool isSpawningState = false; // ★追加1：スポーン中かどうかの判定用

        if (anim != null)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            
            isAttackingState = stateInfo.IsTag("slash") || stateInfo.IsName("slash1") || stateInfo.IsName("slash2") || stateInfo.IsName("slash3") || stateInfo.IsName("slash4") || stateInfo.IsName("slash5") || stateInfo.IsTag("Action") || stateInfo.IsName("Attack") || stateInfo.IsName("root|slash01") || stateInfo.IsName("root|slash 02");
            
            isDamagedState = stateInfo.IsTag("Damage") || stateInfo.IsName("damaged (tired) stance") || stateInfo.IsName("damaged [tired] stance") || stateInfo.IsName("damage") || stateInfo.IsName("root|Take Damage");

            // ★追加2：Animatorで「Spawn」というTagや名前のついたアニメーションが再生中かどうか
            isSpawningState = stateInfo.IsTag("Spawn") || stateInfo.IsName("Spawn");
        }

        // --- NavMeshAgent（敵）の移動制御 ---
        if (agent != null && agent.isActiveAndEnabled)
        {
            if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);
            // ★変更1：ダメージ中だけでなく「スポーン中」もストップさせる
            agent.isStopped = isAttackingState || isDamagedState || isSpawningState;
            return; 
        }

        // --- Rigidbody（プレイヤー等）の移動制御 ---
        float currentSpeed = isRunning ? speed * 2.0f : speed;
        float moveAmount = inputDirection.magnitude;
        
        if (anim != null) anim.SetFloat("Speed", moveAmount * (isRunning ? 2.0f : 1.0f));

        // 🛑 ★変更2：ダメージ中または「スポーン中」は速度を0にして一切動けなくする
        if (isDamagedState || isSpawningState)
        {
            if (rb != null) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        else
        {
            // プレイヤーの向き制御（ロックオン中は敵の方向、それ以外は移動入力の方向）
            if (lockOnTarget != null)
            {
                Vector3 targetDirection = lockOnTarget.position - transform.position;
                targetDirection.y = 0; // 高低差による体の傾きを防ぐ
                if (targetDirection.sqrMagnitude > 0.001f && rb != null)
                {
                    Quaternion targetRot = Quaternion.LookRotation(targetDirection);
                    rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, Time.deltaTime * 10f);
                }
            }
            else
            {
                if (moveAmount > 0.1f && rb != null)
                {
                    Quaternion targetRot = Quaternion.LookRotation(inputDirection);
                    rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, Time.deltaTime * 10f);
                }
            }

            // プレイヤーの移動速度制御
            if (isAttackingState)
            {
                // 攻撃中の移動処理（デフォルトでは移動入力があれば回転のみを行うが、速度は変更しない）
            }
            else
            {
                if (moveAmount > 0.1f && rb != null)
                {
                    rb.linearVelocity = new Vector3(inputDirection.normalized.x * currentSpeed, rb.linearVelocity.y, inputDirection.normalized.z * currentSpeed);
                }
                else
                {
                    if (rb != null) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                }
            }
        }
    }

    public void ExecuteJump()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); 
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    public void SetMovement(Vector3 direction, bool run) { inputDirection = direction; isRunning = run; }
    public void TriggerJump() { isJumpTriggered = true; }
    public void SetAttack(bool attack) { isHoldingAttack = attack; } 
    public void TriggerAttack() { isAttackTriggered = true; } 
    public void SetLockOnTarget(Transform target) { lockOnTarget = target; }
}