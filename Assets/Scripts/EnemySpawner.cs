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

    [Header("ランダムスポーン設定")]
    [Tooltip("ゲーム開始時に敵の位置をランダムにするか")]
    public bool randomizePositionOnStart = true;
    [Tooltip("ランダム配置するX座標の範囲（最小値と最大値）")]
    public Vector2 randomXRange = new Vector2(-45f, 45f);
    [Tooltip("Z座標もランダムにするか（オフの場合は元のZ座標を維持します）")]
    public bool randomizeZ = true;
    [Tooltip("ランダム配置するZ座標の範囲（最小値と最大値、randomizeZがオンの時のみ有効）")]
    public Vector2 randomZRange = new Vector2(-45f, 45f);

    [Header("自動生成設定（プレハブから生成する場合）")]
    [Tooltip("ゲーム開始時に生成する敵のプレハブ（空の場合は既存の子オブジェクトを使用します）")]
    public GameObject enemyPrefab;
    [Tooltip("ゲーム開始時に生成する敵の数")]
    public int spawnCount = 10;
    [Tooltip("生成される敵の最小体力")]
    public int minEnemyHP = 30;
    [Tooltip("生成される敵の最大体力")]
    public int maxEnemyHP = 80;

    private List<GameObject> enemyGroup = new List<GameObject>(); 
    private bool allSpawned = false;
    private bool buffApplied = false;

    private int GetLevelFromName(string name)
    {
        int index = name.IndexOf("Enemy_Lv");
        if (index >= 0)
        {
            string numStr = name.Substring(index + 8);
            string digits = "";
            foreach (char c in numStr)
            {
                if (char.IsDigit(c)) digits += c;
                else break;
            }
            if (int.TryParse(digits, out int val))
            {
                return val;
            }
        }
        return 1; // デフォルトはレベル1
    }

    void Start()
    {
        // スポナーの名前からレベル数を取得（例: Enemy_Lv50 -> 50）
        int level = GetLevelFromName(gameObject.name);

        // ★修正：敵の出現数を「Lv 1で4体」「Lv 10で3体」「Lv 50で2体」「Lv 100で1体」に段階的に減少させる（折れ線補間）
        float countFloat;
        if (level <= 10)
        {
            countFloat = Mathf.Lerp(4f, 3f, (level - 1) / 9f);
        }
        else if (level <= 50)
        {
            countFloat = Mathf.Lerp(3f, 2f, (level - 10) / 40f);
        }
        else
        {
            countFloat = Mathf.Lerp(2f, 1f, (level - 50) / 50f);
        }
        spawnCount = Mathf.RoundToInt(countFloat);
        if (spawnCount < 1) spawnCount = 1;

        // ★修正：難易度（ゲームレベル）を落とし、敵の体力・攻撃力をマイルドに変更
        // 必要攻撃回数をマイルドに（Lv.1で1.5撃、Lv.100で3撃）
        float hitCount = Mathf.Lerp(1.5f, 3f, (level - 1) / 99f);
        double rawHP = 10.0 * System.Math.Pow(3.0, level - 1) * hitCount;
        int enemyHP = (int)System.Math.Min(rawHP, 2000000000); // 20億上限

        // 被弾可能回数をマイルドに（Lv.1で15回、Lv.100でも最低4回耐えられるようにする）
        int playerHP = 100 + 20 * (level - 1);
        float surviveHits = Mathf.Lerp(15f, 4f, (level - 1) / 99f);
        int enemyAttackDamage = Mathf.RoundToInt(playerHP / surviveHits);
        if (enemyAttackDamage < 1) enemyAttackDamage = 1;

        Debug.Log($"[{gameObject.name}] Level {level} Configured -> Count: {spawnCount}, HP: {enemyHP}, DMG: {enemyAttackDamage}");

        // プレハブが設定されている場合は、ゲーム開始時に自動生成する
        if (enemyPrefab != null)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                float randomX = Random.Range(randomXRange.x, randomXRange.y);
                float randomZ = randomizeZ ? Random.Range(randomZRange.x, randomZRange.y) : transform.position.z;
                
                // Y座標はスポナーの高さを基準にする
                Vector3 targetPos = new Vector3(randomX, transform.position.y, randomZ);

                GameObject newEnemy = Instantiate(enemyPrefab, targetPos, Quaternion.identity, transform);

                // 体力（HP）の設定
                EnemyStatus status = newEnemy.GetComponent<EnemyStatus>();
                if (status != null)
                {
                    status.randomizeHPOnStart = false; // 自動スケール値を優先するため自律ランダムは無効化
                    status.SetMaxHP(enemyHP); // ★修正：最大HPと現在HP、スライダーを確実に同期
                }

                // 攻撃力（DamageReceiver）の設定
                DamageReceiver receiver = newEnemy.GetComponentInChildren<DamageReceiver>();
                if (receiver == null) receiver = newEnemy.GetComponent<DamageReceiver>();
                if (receiver != null)
                {
                    receiver.attackDamage = enemyAttackDamage;
                }

                // NavMeshAgentがある場合、NavMesh上の最も近い位置にスナップさせて不具合を防ぐ
                UnityEngine.AI.NavMeshAgent agent = newEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, 15.0f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        newEnemy.transform.position = hit.position;
                    }
                }

                enemyGroup.Add(newEnemy);
                newEnemy.SetActive(false); // スポナーのウェーブ順次出現ロジックに任せるため一旦非アクティブにする
            }
        }
        else
        {
            // プレハブが指定されていない場合は、従来通り配置済みの子オブジェクトを収集して配置する
            foreach (Transform child in transform)
            {
                enemyGroup.Add(child.gameObject);

                // 体力の設定
                EnemyStatus status = child.GetComponent<EnemyStatus>();
                if (status != null)
                {
                    status.randomizeHPOnStart = false;
                    status.SetMaxHP(enemyHP); // ★修正：同期
                }

                // 攻撃力の設定
                DamageReceiver receiver = child.GetComponentInChildren<DamageReceiver>();
                if (receiver == null) receiver = child.GetComponent<DamageReceiver>();
                if (receiver != null)
                {
                    receiver.attackDamage = enemyAttackDamage;
                }

                if (randomizePositionOnStart)
                {
                    float randomX = Random.Range(randomXRange.x, randomXRange.y);
                    float randomZ = randomizeZ ? Random.Range(randomZRange.x, randomZRange.y) : child.position.z;
                    
                    Vector3 targetPos = new Vector3(randomX, child.position.y, randomZ);

                    UnityEngine.AI.NavMeshAgent agent = child.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null)
                    {
                        UnityEngine.AI.NavMeshHit hit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, 15.0f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            targetPos = hit.position;
                        }
                    }
                    
                    child.position = targetPos;
                }

                child.gameObject.SetActive(false);
            }
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
            if (enemy != null)
            {
                EnemyStatus status = enemy.GetComponent<EnemyStatus>();
                if (status != null && status.CurrentHP > 0)
                {
                    aliveCount++;
                }
            }
        }

        if (aliveCount == 0)
        {
            buffApplied = true;
            string msg = "ウェーブクリア！プレイヤーを強化します！";
            Debug.Log(msg);

            if (playerStatus != null)
            {
                playerStatus.HealAndBuffMaxHP(20);
                playerStatus.HealToFull(); // ★追加：レベルクリア時に体力を全回復
            }

            if (playerWeapon != null)
            {
                // ★修正：C#のint型オーバーフロー（21億超過によるマイナスバグ）を防ぐための安全なクランプ
                long nextDamage = (long)playerWeapon.damage * 3;
                if (nextDamage > int.MaxValue - 100000)
                {
                    playerWeapon.damage = int.MaxValue - 100000;
                }
                else
                {
                    playerWeapon.damage = (int)nextDamage;
                }
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