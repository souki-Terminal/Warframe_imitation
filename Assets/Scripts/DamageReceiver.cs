using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("攻撃設定")]
    [Tooltip("相手に与えるダメージ量")]
    public int attackDamage = 20;

    [Header("ノックバック設定")]
    [Tooltip("プレイヤーをノックバックさせる距離")]
    public float knockbackDistance = 3.0f;
    [Tooltip("プレイヤーのノックバックにかかる時間（秒）")]
    public float knockbackDuration = 0.2f;

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
            direction.y = 0;
            if (direction.sqrMagnitude <= 0.001f)
            {
                Debug.LogError("[DamageReceiver] プレイヤーへのノックバック方向が計算できませんでした！仮の方向(Z軸)を使用します。");
                direction = Vector3.forward;
            }
            direction.Normalize();

            player.TakeDamage(attackDamage, direction, knockbackDistance, knockbackDuration);
            Debug.Log($"プレイヤーに {attackDamage} のダメージ！ ノックバック方向: {direction}, 距離: {knockbackDistance}");
        }
    }
}