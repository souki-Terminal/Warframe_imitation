using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("攻撃力")]
    public int damage = 10;

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
                knockbackDir = (enemy.transform.position - transform.position).normalized;
                knockbackDir.y = 0;
            }

            enemy.TakeDamage(damage, knockbackDir);
            Debug.Log($"敵に {damage} のダメージ！ 方向: {knockbackDir}");
        }
    }
}