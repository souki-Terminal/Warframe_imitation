using UnityEngine;

public class Damager : MonoBehaviour
{
    public int damage = 10;
    private Collider myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = false; 
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (myCollider != null) myCollider.enabled = true;
            Invoke("AttackEnd", 0.5f); 
        }
    }

    private void AttackEnd()
    {
        if (myCollider != null) myCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ▼敵へのダメージ判定のみ残す（自分や味方へのダメージ処理を削除）
        EnemyStatus enemy = other.GetComponentInParent<EnemyStatus>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"敵に {damage} のダメージ！");
        }
    }
}