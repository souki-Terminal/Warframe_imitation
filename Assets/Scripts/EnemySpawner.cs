using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("ゲーム開始から出現するまでの時間（秒）")]
    public float spawnStartTime = 5.0f;

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
            }
        }
        // 全員スポーンする処理が終わったフラグを立てる
        allSpawned = true;
    }

    IEnumerator SpawnIndividualEnemy(GameObject enemy)
    {
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, enemy.transform.position, Quaternion.identity);
            Destroy(effect, 3.0f); 
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
        // まだ全員出現しきっていない、または既に報酬を渡したなら何もしない
        if (!allSpawned || buffApplied) return;

        // 生きている（まだ消滅していない）敵の数を数える
        int aliveCount = 0;
        foreach (GameObject enemy in enemyGroup)
        {
            if (enemy != null) aliveCount++;
        }

        // 生きている敵が0になったら報酬を与える
        if (aliveCount == 0)
        {
            buffApplied = true;
            Debug.Log("ウェーブクリア！プレイヤーを強化します！");

            if (playerStatus != null) playerStatus.HealAndBuffMaxHP(20);
            if (playerWeapon != null)
            {
                playerWeapon.damage *= 3;
                Debug.Log($"武器のダメージが強化された！ (現在: {playerWeapon.damage})");
            }
        }
    }
}