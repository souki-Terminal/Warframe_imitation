using UnityEngine;

public class WeaponAnimationEvent : MonoBehaviour
{
    [Header("制御したい武器のコライダー")]
    public Collider attackCollider;

    private Animator anim;
    private bool isPlayerWeapon;
    private Coroutine disableCoroutine;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInParent<Animator>();

        // 初期状態では武器のコライダーを無効化しておく
        if (attackCollider != null)
        {
            // Damager (プレイヤー武器) がアタッチされているか判定
            isPlayerWeapon = attackCollider.GetComponent<Damager>() != null;
            attackCollider.enabled = false;
        }
    }

    void Update()
    {
        // コライダーが有効な時のみチェックを行う（処理負荷軽減）
        if (attackCollider != null && attackCollider.enabled && anim != null)
        {
            // ディレイ消去コルーチンが動作している間は、Updateでの即時OFFをスキップ
            if (disableCoroutine != null) return;

            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            
            // CharacterCore.cs で使用されている攻撃アニメーションのステート/タグの判定ロジックと同期
            bool isAttackingState = stateInfo.IsTag("slash") || stateInfo.IsTag("Action") || stateInfo.IsTag("Attack") || stateInfo.IsTag("attack") ||
                                   stateInfo.IsName("Attack") || stateInfo.IsName("attack") || stateInfo.IsName("EnemyAttack") ||
                                   stateInfo.IsName("Enemy_Attack_1") || stateInfo.IsName("SplashAttack") || stateInfo.IsName("LeftAttack") || stateInfo.IsName("RightAttack") ||
                                   stateInfo.IsName("slash1") || stateInfo.IsName("slash2") || stateInfo.IsName("slash3") || stateInfo.IsName("slash4") || stateInfo.IsName("slash5") ||
                                   stateInfo.IsName("root_slash01") || stateInfo.IsName("root_slash 02");

            if (!isAttackingState)
            {
                if (attackCollider.enabled)
                {
                    Debug.Log($"[WeaponAnimationEvent] 攻撃ステートではないため、UpdateでコライダーをOFFにしました！ 現在のステート名がないか確認してください。");
                }
                attackCollider.enabled = false;
            }
        }
    }

    // アニメーションイベントから呼ばれる関数
    public void OnColliderAttack()
    {
        Debug.Log($"[WeaponAnimationEvent] OnColliderAttack 呼ばれました！ (GameObject: {gameObject.name})");
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
            disableCoroutine = null;
        }
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            Debug.Log($"[WeaponAnimationEvent] コライダーをONにしました。 現在のステート: {(anim.GetCurrentAnimatorClipInfo(0).Length > 0 ? anim.GetCurrentAnimatorClipInfo(0)[0].clip.name : "不明")}");
        }
    }

    public void OffColliderAttack()
    {
        Debug.Log($"[WeaponAnimationEvent] OffColliderAttack 呼ばれました！");
        if (attackCollider != null)
        {
            if (!isPlayerWeapon)
            {
                // 敵の攻撃の場合、判定を当てやすくするため0.2秒遅らせてコライダーを無効化する
                disableCoroutine = StartCoroutine(DisableColliderDelay(0.2f));
            }
            else
            {
                attackCollider.enabled = false;
            }
        }
    }

    private System.Collections.IEnumerator DisableColliderDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (attackCollider != null) attackCollider.enabled = false;
        disableCoroutine = null;
    }
}