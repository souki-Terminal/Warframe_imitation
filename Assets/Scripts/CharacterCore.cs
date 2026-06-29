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
    private float knockbackTimer = 0f;
    private Vector3 knockbackDirection;
    private float knockbackDistanceRemaining = 0f;

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

        // ★追加：摩擦によるキャラクター同士の引っかかり（はまり込み）を防ぐため、コライダーの摩擦を0（Frictionless）に設定する
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            PhysicsMaterial pm = new PhysicsMaterial("Frictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0f,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            col.sharedMaterial = pm;
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

        if (isJumpTriggered && anim != null)
        {
            if (!isDamaged && hasJumpTrigger)
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
        // ノックバック処理を FixedUpdate で行うことで物理挙動と同期させる
        if (knockbackTimer > 0f)
        {
            float dt = Time.fixedDeltaTime;
            float moveDist = 0f;

            if (knockbackTimer <= dt)
            {
                // このフレームでノックバックが終了するため、残りの距離をすべて移動
                moveDist = knockbackDistanceRemaining;
                knockbackTimer = 0f;
            }
            else
            {
                // 通常の移動割合計算（割り算バグを回避するため、減算前の knockbackTimer で割る）
                moveDist = (knockbackDistanceRemaining / knockbackTimer) * dt;
                knockbackTimer -= dt;
            }

            moveDist = Mathf.Min(moveDist, knockbackDistanceRemaining);
            knockbackDistanceRemaining -= moveDist;

            Vector3 moveStep = knockbackDirection * moveDist;

            // 敵（NavMeshAgentを持つキャラクター）の場合は、Rigidbodyがあっても必ずNavMeshの制御（Transform移動）を優先する
            if (rb != null && !rb.isKinematic && agent == null)
            {
                rb.MovePosition(rb.position + moveStep);
                // Debug.Log($"[Knockback] (Rigidbody) Timer: {knockbackTimer:F3}, Move: {moveStep}, Pos: {rb.position}");
            }
            else
            {
                Vector3 prevPos = transform.position;
                Vector3 nextPos = transform.position + moveStep;
                UnityEngine.AI.NavMeshHit hit;
                
                if (UnityEngine.AI.NavMesh.Raycast(transform.position, nextPos, out hit, UnityEngine.AI.NavMesh.AllAreas))
                {
                    nextPos = hit.position;
                }
                else
                {
                    if (UnityEngine.AI.NavMesh.SamplePosition(nextPos, out hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        nextPos.y = hit.position.y;
                    }
                }
                transform.position = nextPos;
                // Debug.Log($"[Knockback] (Transform) Timer: {knockbackTimer:F3}, Move: {moveStep}, Pos: {prevPos} -> {transform.position}");
            }

            if (knockbackTimer <= 0f)
            {
                knockbackDistanceRemaining = 0f;
                if (agent != null && !agent.enabled)
                {
                    agent.enabled = true;
                }
            }
        }

        bool isAttackingState = false;
        bool isDamagedState = false;
        bool isSpawningState = false; // ★追加1：スポーン中かどうかの判定用

        if (anim != null)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            
            isAttackingState = stateInfo.IsTag("slash") || stateInfo.IsTag("Action") || stateInfo.IsTag("Attack") || stateInfo.IsTag("attack") ||
                               stateInfo.IsName("Attack") || stateInfo.IsName("attack") || stateInfo.IsName("EnemyAttack") ||
                               stateInfo.IsName("Enemy_Attack_1") || stateInfo.IsName("SplashAttack") || stateInfo.IsName("LeftAttack") || stateInfo.IsName("RightAttack") ||
                               stateInfo.IsName("slash1") || stateInfo.IsName("slash2") || stateInfo.IsName("slash3") || stateInfo.IsName("slash4") || stateInfo.IsName("slash5") ||
                               stateInfo.IsName("root_slash01") || stateInfo.IsName("root_slash 02");
            
            isDamagedState = stateInfo.IsTag("Damage") || stateInfo.IsName("damaged (tired) stance") || stateInfo.IsName("damaged [tired] stance") || stateInfo.IsName("damage") || stateInfo.IsName("root|Take Damage");

            // ★追加2：Animatorで「Spawn」というTagや名前のついたアニメーションが再生中かどうか
            isSpawningState = stateInfo.IsTag("Spawn") || stateInfo.IsName("Spawn");
        }

        // --- NavMeshAgent（敵）の移動制御 ---
        if (agent != null && agent.isActiveAndEnabled)
        {
            if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);
            // ★変更1：ダメージ中だけでなく「スポーン中」もストップさせる
            // isOnNavMesh が true の時のみ isStopped を変更する (Resume/Stop エラーの防止)
            if (agent.isOnNavMesh)
            {
                agent.isStopped = isAttackingState || isDamagedState || isSpawningState;
            }

            // ★追加：物理的な押し出しによる「ドリフト（氷の上を滑るように遠ざかる挙動）」を防ぐため、エージェント移動中はRigidbodyの水平速度を0にする
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            return; 
        }

        // --- Rigidbody（プレイヤー等）の移動制御 ---
        float currentSpeed = isRunning ? speed * 2.0f : speed;
        float moveAmount = inputDirection.magnitude;
        
        if (anim != null) anim.SetFloat("Speed", moveAmount * (isRunning ? 2.0f : 1.0f));

        // 🛑 ★変更2：ダメージ中または「スポーン中」は速度を0にして一切動けなくする
        // ただし、ノックバック中（knockbackTimer > 0）は物理挙動を優先し、速度をリセットしない
        if ((isDamagedState || isSpawningState) && knockbackTimer <= 0f)
        {
            if (rb != null) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        else if (knockbackTimer > 0f)
        {
            // ノックバック中は直接座標移動しているため、物理慣性による余計な滑りを防ぐ（Y方向の重力のみ維持）
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            return;
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

    public void TriggerKnockback(Vector3 direction, float distance = 3.0f, float duration = 0.2f)
    {
        direction.y = 0;
        if (direction.sqrMagnitude <= 0.001f) return;

        knockbackDirection = direction.normalized;
        knockbackDistanceRemaining = distance;
        knockbackTimer = duration;

        // 物理速度をリセットしてクリアにする
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // 敵（NavMeshAgent）の場合、ノックバック中の座標移動を邪魔させないため一時的に無効化する
        if (agent != null && agent.enabled)
        {
            agent.enabled = false;
        }
    }

    public void TriggerKnockback(float duration)
    {
        TriggerKnockback(transform.forward * -1f, 3.0f, duration);
    }

    public void SetMovement(Vector3 direction, bool run) { inputDirection = direction; isRunning = run; }
    public void TriggerJump() { isJumpTriggered = true; }
    public void SetAttack(bool attack) { isHoldingAttack = attack; } 
    public void TriggerAttack() { isAttackTriggered = true; } 
    public void SetLockOnTarget(Transform target) { lockOnTarget = target; }
}