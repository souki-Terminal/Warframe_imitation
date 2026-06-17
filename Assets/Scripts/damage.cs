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
            enemy.TakeDamage(damage);
            Debug.Log($"敵に {damage} のダメージ！");
        }
    }
}