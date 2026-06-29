using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("攻撃力")]
    public int damage = 10;

    [Header("ノックバック設定")]
    [Tooltip("敵をノックバックさせる距離")]
    public float knockbackDistance = 3.0f;
    [Tooltip("敵のノックバックにかかる時間（秒）")]
    public float knockbackDuration = 0.2f;

    // StartやUpdateでの直接制御はすべて削除！
    // コライダーのON/OFFは WeaponAnimationEvent.cs に任せます。

    private void OnTriggerEnter(Collider other)
    {
        // 敵へのダメージ判定のみ実行
        EnemyStatus enemy = other.GetComponentInParent<EnemyStatus>();
        if (enemy != null)
        {
            // プレイヤーの正面方向をノックバック方向とする（正面が取得できない場合は敵との相対位置から計算）
            Vector3 knockbackDir = Vector3.zero;
            Transform playerRoot = transform.root;
            if (playerRoot != null)
            {
                knockbackDir = playerRoot.forward;
            }
            else
            {
                knockbackDir = transform.forward;
            }

            knockbackDir.y = 0;
            if (knockbackDir.sqrMagnitude <= 0.001f)
            {
                knockbackDir = (enemy.transform.position - transform.position);
                knockbackDir.y = 0;
            }
            
            if (knockbackDir.sqrMagnitude <= 0.001f)
            {
                Debug.LogError("[Damager] 敵へのノックバック方向が計算できませんでした！仮の方向(Z軸)を使用します。");
                knockbackDir = Vector3.forward;
            }

            knockbackDir.Normalize();

            enemy.TakeDamage(damage, knockbackDir, knockbackDistance, knockbackDuration);
            Debug.Log($"敵に {damage} のダメージ！ ノックバック方向: {knockbackDir}, 距離: {knockbackDistance}");
        }
    }
}