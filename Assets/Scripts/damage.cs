using UnityEngine;

public class Damager : MonoBehaviour
{
    public int damage = 10;
    private Collider myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = false; // 最初はオフ
    }

    void Update()
    {
        // ★応急処置：左クリック(0)を押している一瞬だけ、剣の当たり判定を強制的にONにする
        // （※もし別のキーで攻撃している場合は Input.GetKeyDown(KeyCode.Z) などに変更してください）
        if (Input.GetMouseButtonDown(0))
        {
            if (myCollider != null) myCollider.enabled = true;
            Invoke("AttackEnd", 0.5f); // 0.5秒後に自動でOFFにする
        }
    }

    private void AttackEnd()
    {
        if (myCollider != null) myCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyStatus enemy = other.GetComponentInParent<EnemyStatus>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"敵に {damage} のダメージ！");
            return;
        }

        PlayerStatus player = other.GetComponentInParent<PlayerStatus>();
        if (player != null)
        {
            player.TakeDamage(damage); 
            Debug.Log($"プレイヤーが {damage} のダメージを受けた！");
        }
    }
}