using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("攻撃設定")]
    [Tooltip("相手に与えるダメージ量")]
    public int attackDamage = 20;

    private void OnCollisionEnter(Collision collision)
    {
        ApplyDamage(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        ApplyDamage(other.gameObject);
    }

    private void ApplyDamage(GameObject target)
    {
        // ★修正：プレイヤーのコライダーが子オブジェクトにある場合を考慮し、GetComponentInParent を使用する
        PlayerStatus player = target.GetComponentInParent<PlayerStatus>();
        if (player != null)
        {
            Vector3 direction = player.transform.position - transform.position;
            player.TakeDamage(attackDamage, direction);
        }
    }
}