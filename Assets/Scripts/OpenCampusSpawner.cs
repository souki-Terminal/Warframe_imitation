using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCampusSpawner : MonoBehaviour
{
    [Header("ランダム配置範囲")]
    public Vector2 randomXRange = new Vector2(-45f, 45f);
    public Vector2 randomZRange = new Vector2(-45f, 45f);

    [Header("出現時のエフェクト")]
    public GameObject spawnEffectPrefab;

    [Header("ウェーブ設定（ヒエラルキーのEnemy_Lvなどを直接ドラッグ＆ドロップ）")]
    public Transform practiceWaveGroup;
    public Transform wave0Group;
    public Transform wave15Group;
    public Transform wave30Group;
    public Transform wave45Group;
    public Transform wave60Group;

    void Awake()
    {
        // Start()が走る前に非表示にすることで、敵側の自動ランダム化を防ぐ
        HideAllChildren();
    }

    void Start()
    {
        // 練習ウェーブ開始時に本番ウェーブと同じBGMを流す
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameBGM();

        // 練習ウェーブ開始
        StartCoroutine(StartPracticeWave());
    }

    private void HideAllChildren()
    {
        Transform[] allGroups = { practiceWaveGroup, wave0Group, wave15Group, wave30Group, wave45Group, wave60Group };
        foreach (Transform group in allGroups)
        {
            if (group != null)
            {
                // もし group 自身が Enemy (EnemyStatusを持つ) だった場合は、自身を非表示にする
                if (group.GetComponent<EnemyStatus>() != null)
                {
                    group.gameObject.SetActive(false);
                }
                else
                {
                    // グループ用オブジェクト（空のGameObjectなど）の場合は、子オブジェクトを非表示にする
                    foreach (Transform enemy in group)
                    {
                        if (enemy.GetComponent<EnemyStatus>() != null)
                        {
                            enemy.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    IEnumerator StartPracticeWave()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ShowNotification("練習ウェーブ開始！敵を5体倒せ！");
        }
        yield return new WaitForSeconds(3.0f);
        // 練習ウェーブ
        if (practiceWaveGroup != null)
        {
            StartCoroutine(SpawnGroup(practiceWaveGroup));
        }
        else
        {
            Debug.LogWarning("PracticeWave グループが設定されていません。");
        }
    }
    
    // GameManagerから呼ばれる
    public void OnPracticeWaveCleared()
    {
        StartCoroutine(StartMainWave());
    }

    IEnumerator StartMainWave()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ShowNotification("本番ウェーブ開始！敵が次々と現れるぞ！");
        }
        
        // 練習ウェーブから本番BGMを継続して流すため、BGMのフェードアウトと切り替え処理を削除

        yield return new WaitForSeconds(3.0f);

        // 0, 15, 30, 45, 60 秒でそれぞれのグループを出現させる
        StartCoroutine(SpawnGroupAfterDelay(wave0Group, 0f));
        StartCoroutine(SpawnGroupAfterDelay(wave15Group, 15.0f));
        StartCoroutine(SpawnGroupAfterDelay(wave30Group, 30.0f));
        StartCoroutine(SpawnGroupAfterDelay(wave45Group, 45.0f));
        StartCoroutine(SpawnGroupAfterDelay(wave60Group, 60.0f));
    }

    IEnumerator SpawnGroupAfterDelay(Transform group, float delay)
    {
        if (group == null) yield break;

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        StartCoroutine(SpawnGroup(group));
    }

    IEnumerator SpawnGroup(Transform group)
    {
        if (group == null) yield break;

        float delayBetweenSpawns = 0.1f;

        // もし group 自身が Enemy (EnemyStatusを持つ) だった場合は、自身をスポーンさせる
        if (group.GetComponent<EnemyStatus>() != null)
        {
            StartCoroutine(SpawnSingleEnemy(group.gameObject));
        }
        else
        {
            // グループオブジェクトの場合は、子オブジェクトを順にスポーンさせる
            foreach (Transform enemy in group)
            {
                if (enemy.GetComponent<EnemyStatus>() != null)
                {
                    StartCoroutine(SpawnSingleEnemy(enemy.gameObject));
                    yield return new WaitForSeconds(delayBetweenSpawns);
                }
            }
        }
    }

    IEnumerator SpawnSingleEnemy(GameObject enemyPrefab)
    {
        // 座標をランダム化
        float randomX = Random.Range(randomXRange.x, randomXRange.y);
        float randomZ = Random.Range(randomZRange.x, randomZRange.y);
        // ★修正：元のオブジェクトのY座標ではなく、高めの位置から落下させるかNavMeshにスナップする
        Vector3 targetPos = new Vector3(randomX, enemyPrefab.transform.position.y + 2f, randomZ);
        Vector3 originalPos = enemyPrefab.transform.position;

        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out UnityEngine.AI.NavMeshHit hit, 15.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            targetPos = hit.position;
        }
        else
        {
            // NavMeshが見つからない場合は元の位置にする
            targetPos = originalPos;
        }

        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, targetPos, Quaternion.identity);
            Destroy(effect, 3.0f);
        }
        
        yield return new WaitForSeconds(0.5f);

        if (enemyPrefab == null) yield break;

        // ★追加：同じ敵オブジェクトを複数ウェーブで使い回しても問題ないように、新しく複製（クローン）を生成する
        GameObject spawnedEnemy = Instantiate(enemyPrefab, targetPos, Quaternion.identity);

        EnemyStatus status = spawnedEnemy.GetComponent<EnemyStatus>();
        if (status != null)
        {
            status.randomizeHPOnStart = false;
            status.randomizePositionOnStart = false; 
        }

        spawnedEnemy.SetActive(true);
        Animator anim = spawnedEnemy.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Spawn");
    }
}
