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
    private float deathYPosition;

    public void ExecuteDeath()
    {
        if (isDead) return;
        isDead = true;

        // ★追加：床抜け防止のため、レイキャストで足元の地面の正確な高さを取得する（空中死亡時でも地面で止まるようにする）
        deathYPosition = transform.position.y;
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 0.5f, Vector3.down, 20.0f);
        float bestY = -9999f;
        foreach (var h in hits)
        {
            if (h.transform != transform && !h.transform.IsChildOf(transform))
            {
                // ★追加：他の敵やプレイヤーなどの動的キャラクターを床面と誤認しないように除外する
                if (h.transform.GetComponentInParent<CharacterCore>() == null &&
                    h.transform.GetComponentInParent<PlayerStatus>() == null &&
                    h.transform.GetComponentInParent<EnemyStatus>() == null)
                {
                    if (h.point.y > bestY)
                    {
                        bestY = h.point.y;
                    }
                }
            }
        }
        if (bestY > -9000f)
        {
            deathYPosition = bestY;
        }

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

        // ★修正：プレイヤー同様に物理演算と地面への接触（コライダー）を残すことで、床抜けを防ぎ自然に倒れ込ませる。
        // ただし、倒れた死体がプレイヤーの移動を邪魔しないよう、接地に必要なメインコライダー以外のコライダーは無効化し、メインコライダーは極小化する。
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.height = 0.2f;
            cc.center = new Vector3(0, 0.1f, 0);
        }

        // 全コライダーを取得し、接地に使うメインコライダー（ルート優先、なければ最初の子）を特定する
        Collider[] cols = GetComponentsInChildren<Collider>();
        Collider mainCol = GetComponent<Collider>();
        if (mainCol == null && cols.Length > 0)
        {
            mainCol = cols[0]; // ルートになければ最初の子コライダーをメインとする
        }

        foreach (var c in cols)
        {
            if (c == mainCol)
            {
                // 接地用コライダーは有効のまま残し、床抜けを防ぐために極小サイズに縮小する
                c.enabled = true;
                c.isTrigger = false;

                if (c is CapsuleCollider cap)
                {
                    cap.height = 0.2f;
                    cap.center = new Vector3(0, 0.1f, 0);
                }
                else if (c is BoxCollider box)
                {
                    box.size = new Vector3(box.size.x, 0.2f, box.size.z);
                    box.center = new Vector3(box.center.x, 0.1f, box.center.z);
                }
                else if (c is SphereCollider sphere)
                {
                    sphere.radius = 0.1f;
                    sphere.center = new Vector3(sphere.center.x, 0.1f, sphere.center.z);
                }
            }
            else
            {
                // それ以外のサブコライダー（手足など）はプレイヤーのすり抜けのために無効化する
                if (destroyOnDeath)
                {
                    c.enabled = false;
                }
            }
        }

        // ★修正：重力落下による床抜け（すり抜け）を完全に防ぐため、すべてのRigidbodyをキネマティック（物理演算停止）にし、重力をオフにする
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (var r in rbs)
        {
            r.linearVelocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
            r.isKinematic = true;
            r.useGravity = false;
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

    void LateUpdate()
    {
        if (isDead)
        {
            // ★追加：何があっても床下（死亡時に検知した地面の高さ）にオブジェクトの座標がいかないようにクランプする
            if (transform.position.y < deathYPosition)
            {
                transform.position = new Vector3(transform.position.x, deathYPosition, transform.position.z);
                
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    // 物理速度の下向き成分もクリアする
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Max(0f, rb.linearVelocity.y), rb.linearVelocity.z);
                }
            }
        }
    }
}