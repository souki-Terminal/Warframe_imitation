using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("攻撃設定")]
    [Tooltip("相手に与えるダメージ量")]
    [SerializeField] private int attackDamage = 20;

    public int AttackDamage { get => attackDamage; set => attackDamage = value; }

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
                // 完全に位置が重なっている場合の安全なフォールバック
                // 攻撃側が向いている方向へ吹き飛ばす
                direction = transform.forward;
                direction.y = 0;
                if (direction.sqrMagnitude <= 0.001f) direction = Vector3.forward;
            }
            direction.Normalize();

            player.TakeDamage(attackDamage, direction, knockbackDistance, knockbackDuration);
            Debug.Log($"プレイヤーに {attackDamage} のダメージ！ ノックバック方向: {direction}, 距離: {knockbackDistance}");
        }
    }
}