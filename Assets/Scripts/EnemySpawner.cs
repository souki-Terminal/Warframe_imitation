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

        // ★修正：敵の体力をレベルに応じて指定の攻撃回数で倒せるように設定
        // Lv1=2回, Lv2=4回, Lv3=8回, Lv4=16回... と倍増させる (2のレベル乗)
        float hitCount = Mathf.Pow(2f, level);
        
        // プレイヤーの武器の攻撃力はウェーブクリア毎に3倍になるため、ベースダメージ10に3の(レベル-1)乗をかけたものが想定ダメージ
        double rawHP = 10.0 * System.Math.Pow(3.0, level - 1) * hitCount;
        int enemyHP = (int)System.Math.Min(rawHP, 2000000000); // 20億上限

        // 被弾可能回数をマイルドに（Lv.1で15回、Lv.100でも最低4回耐えられるようにする）
        int playerHP = 100 + 20 * (level - 1);
        float surviveHits = Mathf.Lerp(15f, 4f, (level - 1) / 99f);
        int enemyAttackDamage = Mathf.RoundToInt(playerHP / surviveHits);
        if (enemyAttackDamage < 1) enemyAttackDamage = 1;

        Debug.Log($"[{gameObject.name}] Level {level} Configured -> HP: {enemyHP}, DMG: {enemyAttackDamage}");

        // ウェーブごとの敵の数（Lv1: 4, Lv2: 3, Lv3: 2, Lv4以降: 1）
        int targetCount = Mathf.Max(1, 5 - level);

        GameObject baseEnemy = null;
        if (transform.childCount > 0)
        {
            baseEnemy = transform.GetChild(0).gameObject;
        }

        if (baseEnemy != null)
        {
            List<GameObject> existingChildren = new List<GameObject>();
            foreach (Transform child in transform)
            {
                existingChildren.Add(child.gameObject);
            }

            // 必要な数だけ敵を用意する
            for (int i = 0; i < targetCount; i++)
            {
                GameObject enemyObj;
                if (i < existingChildren.Count)
                {
                    enemyObj = existingChildren[i];
                }
                else
                {
                    enemyObj = Instantiate(baseEnemy, transform);
                }
                enemyGroup.Add(enemyObj);
            }

            // 余分な子オブジェクトは削除する
            for (int i = targetCount; i < existingChildren.Count; i++)
            {
                Destroy(existingChildren[i]);
            }
        }

        // 決定した敵グループに対して設定を行う
        foreach (GameObject childObj in enemyGroup)
        {
            Transform child = childObj.transform;

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
                    receiver.AttackDamage = enemyAttackDamage;
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
                string uiMsg = "ウェーブクリア！";
                if (playerWeapon != null)
                {
                    uiMsg += $"\n武器のダメージが強化されました！ (現在: {playerWeapon.damage})";
                }
                GameManager.instance.ShowNotification(uiMsg);
                GameManager.instance.OnSpawnerCleared(this);
            }
        }
    }
}