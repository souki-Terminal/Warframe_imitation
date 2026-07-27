using UnityEngine;

public class Damager : MonoBehaviour
{
    [Header("攻撃力")]
    public int baseDamage = 40; // 刀の初期ダメージ40
    public int damage = 40;

    [Header("クリティカル設定")]
    public float criticalChance = 0f; // 0.0 ~ 1.0 の確率
    public float criticalMultiplier = 2.0f; // クリティカル時の倍率

    [Header("ノックバック設定")]
    [Tooltip("敵をノックバックさせる距離")]
    public float knockbackDistance = 3.0f;
    [Tooltip("敵のノックバックにかかる時間（秒）")]
    public float knockbackDuration = 0.2f;

    private System.Collections.Generic.HashSet<EnemyStatus> hitEnemiesInCurrentSwing = new System.Collections.Generic.HashSet<EnemyStatus>();

    public void ClearHitList()
    {
        hitEnemiesInCurrentSwing.Clear();
    }

    public void RecalculateDamage()
    {
        if (PlayerPowerUps.instance == null) return;
        
        // 修正前: レベルアップごとに +100% (基本ダメージ分が加算される)
        // 修正後: レベルアップごとに 3倍（元の仕様ベース）
        // Lv0=40, Lv1=120, Lv2=360, Lv3=1080
        int multiplier = 1;
        for (int i = 0; i < PlayerPowerUps.instance.meleeDamageLevel; i++) multiplier *= 3;
        
        damage = baseDamage * multiplier;
    }

    // StartやUpdateでの直接制御はすべて削除！
    // コライダーのON/OFFは WeaponAnimationEvent.cs に任せます。

    private void OnTriggerEnter(Collider other)
    {
        // 敵へのダメージ判定のみ実行
        EnemyStatus enemy = other.GetComponentInParent<EnemyStatus>();
        if (enemy != null && !hitEnemiesInCurrentSwing.Contains(enemy))
        {
            hitEnemiesInCurrentSwing.Add(enemy);
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

            // クリティカル判定
            bool isCritical = Random.value < criticalChance;
            int finalDamage = isCritical ? Mathf.RoundToInt(damage * criticalMultiplier) : damage;

            enemy.TakeDamage(finalDamage, knockbackDir, knockbackDistance, knockbackDuration, isCritical);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerHit();
            Debug.Log($"敵に {finalDamage} のダメージ！(クリティカル: {isCritical}) ノックバック方向: {knockbackDir}, 距離: {knockbackDistance}");

            if (PlayerPowerUps.instance != null)
            {
                // 1. 吸収 (Lifesteal) - 切断や爆発ダメージは含めない
                if (PlayerPowerUps.instance.lifestealLevel > 0)
                {
                    float lifestealRate = 0.2f * PlayerPowerUps.instance.lifestealLevel;
                    int healAmount = Mathf.Max(1, Mathf.RoundToInt(finalDamage * lifestealRate));
                    PlayerStatus player = FindFirstObjectByType<PlayerStatus>();
                    if (player != null && player.currentHP > 0 && player.currentHP < player.maxHP)
                    {
                        player.currentHP = Mathf.Min(player.maxHP, player.currentHP + healAmount);
                        player.UpdateUI();
                    }
                }

                // 2. 切断 (Cut)
                if (PlayerPowerUps.instance.cutLevel > 0)
                {
                    enemy.ApplyCut(PlayerPowerUps.instance.cutLevel);
                }

                // 3. 遅延 (Slow)
                if (PlayerPowerUps.instance.slowLevel > 0)
                {
                    enemy.ApplySlow(PlayerPowerUps.instance.slowLevel);
                }

                // 4. 爆発 (Explosion)
                if (PlayerPowerUps.instance.explosionLevel > 0)
                {
                    float explosionRate = 0.2f * PlayerPowerUps.instance.explosionLevel;
                    int explosionDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * explosionRate));
                    float explosionRadius = 5f; // 固定の爆発半径

                    Collider[] colliders = Physics.OverlapSphere(enemy.transform.position, explosionRadius);
                    System.Collections.Generic.HashSet<EnemyStatus> hitEnemies = new System.Collections.Generic.HashSet<EnemyStatus>();
                    foreach (Collider col in colliders)
                    {
                        EnemyStatus targetEnemy = col.GetComponentInParent<EnemyStatus>();
                        if (targetEnemy != null && targetEnemy != enemy && targetEnemy.CurrentHP > 0 && !hitEnemies.Contains(targetEnemy))
                        {
                            hitEnemies.Add(targetEnemy);
                            targetEnemy.TakeDamage(explosionDamage, false);

                            // 爆発の副次的ダメージにも属性(切断・遅延)を付与する（吸収は付与しない）
                            if (PlayerPowerUps.instance.cutLevel > 0) targetEnemy.ApplyCut(PlayerPowerUps.instance.cutLevel);
                            if (PlayerPowerUps.instance.slowLevel > 0) targetEnemy.ApplySlow(PlayerPowerUps.instance.slowLevel);
                        }
                    }
                }
            }
        }
    }
}