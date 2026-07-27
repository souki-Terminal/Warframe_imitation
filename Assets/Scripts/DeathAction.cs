using System.Collections;
using UnityEngine;

public class DeathAction : MonoBehaviour
{
    [Header("アニメーション設定")]
    [Tooltip("Animatorのパラメーター名")]
    public string dieParameterName = "Die";
    [Tooltip("Triggerとして呼び出す場合はチェック、Bool値(true)として呼ぶ場合は外す")]
    public bool useTrigger = true;

    [Header("消滅設定")]
    [Tooltip("死亡後にオブジェクトを削除します（敵はオン、プレイヤーはオフを推奨）")]
    public bool destroyOnDeath = true;
    [Tooltip("死亡アニメーションが再生されてから消滅するまでの時間（秒）")]
    public float destroyDelay = 2.0f;

    private bool isDead = false;

    public void ExecuteDeath()
    {
        if (isDead) return;
        isDead = true;

        // ★修正：モデルが子オブジェクトにある場合を考慮し、子オブジェクトからもAnimatorを探す
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();

        if (anim != null)
        {
            // 死亡アニメーションを再生
            if (useTrigger) anim.SetTrigger(dieParameterName);
            else anim.SetBool(dieParameterName, true);
        }

        // ★修正：型安全な GetComponent<T> に変更し、移動・AI関連のスクリプトを確実に停止する
        CharacterCore charCore = GetComponent<CharacterCore>();
        if (charCore != null) charCore.enabled = false;

        // NavMeshAgent を無効化する（敵用：これがないと直立姿勢を強制されて横になれない）
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // ★修正：AI・攻撃・ステータス制御スクリプトを安全に停止
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null) enemy.enabled = false;

        EnemyStatus enemyStatus = GetComponent<EnemyStatus>();
        if (enemyStatus != null) enemyStatus.enabled = false;

        // --- 床抜け防止 ---
        Rigidbody myRb = GetComponentInChildren<Rigidbody>();
        if (myRb != null)
        {
            myRb.linearVelocity = Vector3.zero;
            myRb.angularVelocity = Vector3.zero;
            myRb.isKinematic = true; 
            myRb.useGravity = false; 
        }

        // 自身のすべてのコライダーを取得
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols)
        {
            // 死亡した敵に攻撃が当たらないようにするため、すべてのコライダーを無効化する
            // （Rigidbodyをキネマティックにして重力も切っているため、床抜けはしません）
            c.enabled = false;
        }

        if (destroyOnDeath)
        {
            StartCoroutine(DestroyRoutine());
        }
    }
    private IEnumerator DestroyRoutine()
    {
        // ★修正：死亡モーションが確実に最後まで見れるように、長めのディレイを設定（最低でも2.5秒）
        float delay = Mathf.Max(destroyDelay, 2.5f);
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }


}