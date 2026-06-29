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
            // ★追加：デスアニメーションのルートモーションがキャラクターを床下に沈める（床抜けする）のを防ぐため、一時的にルートモーションをオフにする
            anim.applyRootMotion = false;

            // ★追加：死亡アニメーションへのスムーズな遷移と、死亡後に攻撃等のアニメーションが上書き再生されるのを防ぐため、パラメータをリセットする
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    anim.SetBool(param.name, false);
                }
                else if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    anim.ResetTrigger(param.name);
                }
                else if (param.type == AnimatorControllerParameterType.Float)
                {
                    anim.SetFloat(param.name, 0f);
                }
                else if (param.type == AnimatorControllerParameterType.Int)
                {
                    anim.SetInteger(param.name, 0);
                }
            }

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

        // --- 床抜け防止とプレイヤーとの衝突無視を設定 ---
        // 物理演算（重力）をオンにしてしまうと、コライダーのサイズやアニメーションとの兼ね合いで床抜け（すり抜け）が発生しやすいため、
        // アニメーションによる倒れ込み（キネマティックな状態）を維持します。
        Rigidbody myRb = GetComponent<Rigidbody>();
        if (myRb != null)
        {
            myRb.linearVelocity = Vector3.zero;
            myRb.angularVelocity = Vector3.zero;
            myRb.isKinematic = true; // ★修正：床抜けを防ぐため、キネマティックのままにする
            myRb.useGravity = false; // ★修正：重力をオフにする
        }

        // 自身のすべてのコライダーを取得
        Collider[] cols = GetComponentsInChildren<Collider>();

        // プレイヤーとの衝突を無視
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("unitychan");
        if (player != null)
        {
            Collider[] playerCols = player.GetComponentsInChildren<Collider>();
            foreach (var myCol in cols)
            {
                foreach (var playerCol in playerCols)
                {
                    Physics.IgnoreCollision(myCol, playerCol, true);
                }
            }
        }

        // 他の敵との衝突も無視
        EnemyStatus[] allEnemies = FindObjectsByType<EnemyStatus>(FindObjectsSortMode.None);
        foreach (var otherEnemy in allEnemies)
        {
            if (otherEnemy.gameObject == gameObject) continue;
            Collider[] otherCols = otherEnemy.GetComponentsInChildren<Collider>();
            foreach (var myCol in cols)
            {
                foreach (var otherCol in otherCols)
                {
                    Physics.IgnoreCollision(myCol, otherCol, true);
                }
            }
        }

        // 接地用コライダー（メイン）を特定する
        Collider mainCol = GetComponent<Collider>();
        if (mainCol == null && cols.Length > 0)
        {
            mainCol = cols[0];
        }

        foreach (var c in cols)
        {
            if (c == mainCol)
            {
                // メインコライダーは有効のまま、サイズも縮小しない（床抜けを完璧に防ぐため）
                c.enabled = true;
                c.isTrigger = false;
            }
            else
            {
                // それ以外のサブコライダー（手足、武器など）は無効化
                c.enabled = false;
            }
        }

        if (destroyOnDeath)
        {
            StartCoroutine(DestroyRoutine());
        }
    }
    private IEnumerator DestroyRoutine()
    {
        // ★修正：インスペクター設定でディレイが0秒など極端に短い値になっていても、死亡モーションが正しく再生されるよう最低1.5秒の猶予を保証する
        float delay = Mathf.Max(destroyDelay, 1.5f);
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }


}