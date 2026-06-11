using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActivator : MonoBehaviour
{
    public float spawnInterval = 3.0f;

    [Header("全滅時の報酬設定")]
    public PlayerStatus playerStatus;
    public Damager playerWeapon;

    private List<GameObject> allEnemies = new List<GameObject>();
    private bool allSpawned = false;
    private bool buffApplied = false;

    void Start()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject enemy = transform.GetChild(i).gameObject;
            enemy.SetActive(false);
            allEnemies.Add(enemy); // リストに登録
        }
        StartCoroutine(ActivateEnemiesRoutine());
    }

    private IEnumerator ActivateEnemiesRoutine()
    {
        // 順番に敵を表示していく
        for (int i = 0; i < allEnemies.Count; i++)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            // 出現する前にすでに攻撃を受けて消滅(null)していないか確認
            if (allEnemies[i] != null)
            {
                allEnemies[i].SetActive(true);
            }
        }
        allSpawned = true; // 全員出現し終わったフラグ
    }

    void Update()
    {
        if (buffApplied) return;

        // リストの中身を数えるだけにして、List自体は書き換えない（エラー回避）
        int aliveCount = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i] != null)
            {
                aliveCount++;
            }
        }

        // 「全員出現し終わった」＆「生きている敵が0人」場合
        if (allSpawned && aliveCount == 0)
        {
            ApplyBuffs();
        }
    }

    private void ApplyBuffs()
    {
        buffApplied = true;
        Debug.Log("ウェーブクリア！プレイヤーを強化します！");

        if (playerStatus != null)
        {
            playerStatus.HealAndBuffMaxHP(20);
        }

        if (playerWeapon != null)
        {
            playerWeapon.damage *= 3;
            Debug.Log($"武器のダメージが5倍になった！ (現在: {playerWeapon.damage})");
        }
    }
}