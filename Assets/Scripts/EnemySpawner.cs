using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("ゲーム開始から出現するまでの時間（秒）")]
    public float spawnStartTime = 5.0f;

    // ★追加：敵が1体ずつ出現する間隔（EnemyActivate.csにあったものを移植）
    [Header("敵が1体ずつ出現する間隔（秒）")]
    public float spawnInterval = 3.0f;

    [Header("出現時のエフェクトのプレハブ")]
    public GameObject spawnEffectPrefab; 

    [Header("エフェクトが出てから敵が表示されるまでのラグ（秒）")]
    public float delayBeforeSpawn = 0.5f; 

    [Header("全滅時の報酬設定")]
    public PlayerStatus playerStatus;
    public Damager playerWeapon;

    private List<GameObject> enemyGroup = new List<GameObject>(); 
    private bool allSpawned = false;
    private bool buffApplied = false;

    void Start()
    {
        foreach (Transform child in transform)
        {
            enemyGroup.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        
        // GameManager が存在しない場合は自動的に開始する
        if (GameManager.instance == null)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnAllEnemies());
    }

    IEnumerator SpawnAllEnemies()
    {
        yield return new WaitForSeconds(spawnStartTime);

        foreach (GameObject enemy in enemyGroup)
        {
            if (enemy != null)
            {
                StartCoroutine(SpawnIndividualEnemy(enemy));
                
                // ★ここが重要：1体の出現処理を開始したら、指定した秒数だけ待つ！
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        // 全員スポーンする処理が終わったフラグを立てる
        allSpawned = true;
    }

    IEnumerator SpawnIndividualEnemy(GameObject enemy)
    {
        if (spawnEffectPrefab != null)
        {
            // インスペクターの設定ミス防止：エフェクトプレハブに敵キャラクター自身やスポナーが設定されている場合は生成しない
            if (spawnEffectPrefab.GetComponent<Enemy>() != null || 
                spawnEffectPrefab.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>() != null ||
                spawnEffectPrefab.GetComponent<EnemySpawner>() != null)
            {
                Debug.LogWarning($"[EnemySpawner Warning] {gameObject.name} の Spawn Effect Prefab に敵キャラクターまたはスポナー自身 ({spawnEffectPrefab.name}) が設定されているため、生成をスキップしました。インスペクターの設定を確認してください。");
            }
            else
            {
                // ここで生成されるクローンはエフェクトだけです
                GameObject effect = Instantiate(spawnEffectPrefab, enemy.transform.position, Quaternion.identity);
                Destroy(effect, 3.0f); 
            }
        }

        yield return new WaitForSeconds(delayBeforeSpawn);

        if (enemy != null)
        {
            enemy.SetActive(true);
            Animator anim = enemy.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetTrigger("Spawn");
        }
    }

    void Update()
    {
        if (!allSpawned || buffApplied) return;

        int aliveCount = 0;
        foreach (GameObject enemy in enemyGroup)
        {
            if (enemy != null) aliveCount++;
        }

        if (aliveCount == 0)
        {
            buffApplied = true;
            string msg = "ウェーブクリア！プレイヤーを強化します！";
            Debug.Log(msg);

            if (playerStatus != null) playerStatus.HealAndBuffMaxHP(20);
            if (playerWeapon != null)
            {
                playerWeapon.damage *= 3;
                string buffMsg = $"武器のダメージが強化された！ (現在: {playerWeapon.damage})";
                Debug.Log(buffMsg);
            }

            // GameManager にこのウェーブが完了したことを通知する
            if (GameManager.instance != null)
            {
                // LiberationSans SDF などのデフォルトフォントでの文字化け（□表示）を防ぐため、
                // ゲーム画面内の通知は英語で送信します。
                string uiMsg = "Wave Cleared! Player Buffed!";
                if (playerWeapon != null)
                {
                    uiMsg += $"\nWeapon Damage Upgraded! (Now: {playerWeapon.damage})";
                }
                GameManager.instance.ShowNotification(uiMsg);
                GameManager.instance.OnSpawnerCleared(this);
            }
        }
    }
}