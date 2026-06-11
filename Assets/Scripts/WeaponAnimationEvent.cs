using UnityEngine;

public class WeaponAnimationEvent : MonoBehaviour
{
    [Header("制御したい武器のコライダー")]
    public Collider attackCollider;

    void Start()
    {
        // 初期状態では武器のコライダーを無効化しておく
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    // アニメーションイベントから呼ばれる関数
    public void OnColliderAttack()
    {
        if (attackCollider != null) attackCollider.enabled = true;
    }

    public void OffColliderAttack()
    {
        if (attackCollider != null) attackCollider.enabled = false;
    }
}