using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("攻撃設定")]
    [Tooltip("相手に与えるダメージ量")]
    public int attackDamage = 20;

    // 物理的な衝突（OnCollisionEnter）でダメージを与える場合
    private void OnCollisionEnter(Collision collision)
    {
        ApplyDamage(collision.gameObject);
    }

    // トリガー（IsTriggerをオンにしたコライダー）でダメージを与える場合
    private void OnTriggerEnter(Collider other)
    {
        ApplyDamage(other.gameObject);
    }

    /// <summary>
    /// ぶつかった相手が敵かプレイヤーかを判断してダメージを与える共通処理
    /// </summary>
    private void ApplyDamage(GameObject target)
    {
        // 相手が敵（EnemyStatusを持っている）か確認
        EnemyStatus enemy = target.GetComponent<EnemyStatus>();
        if (enemy != null)
        {
            enemy.TakeDamage(attackDamage);
            return; // 敵へのダメージ処理が終わったら終了
        }

        // 相手がプレイヤー（PlayerStatusを持っている）か確認
        PlayerStatus player = target.GetComponent<PlayerStatus>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);
        }
    }
}