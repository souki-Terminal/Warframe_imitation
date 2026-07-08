using UnityEngine;
using UnityEngine.UI; // スライダー(HPバー)を使うために必要

[RequireComponent(typeof(DeathAction))]
public class EnemyStatus : MonoBehaviour
{
    [Header("敵のステータス")]
    public int maxHP = 50;
    private int currentHP;
    public int CurrentHP => currentHP;

    [Header("ノックバック")]
    public float knockbackForce = 3.0f;
    public float knockbackUpForce = 0.8f;

    [Header("UI設定")]
    [Tooltip("敵の頭上に出すHPスライダー")]
    public Slider hpSlider;
    [Tooltip("HPバーの色")]
    public Color hpBarColor = Color.red;

    [Header("自律ランダム配置設定")]
    [Tooltip("ゲーム開始時にこの敵の位置を自動でランダムにするか")]
    public bool randomizePositionOnStart = true;
    [Tooltip("ランダム配置するX座標の範囲（最小値と最大値）")]
    public Vector2 randomXRange = new Vector2(-45f, 45f);
    [Tooltip("Z座標もランダムにするか（オフの場合は元のZ座標を維持します）")]
    public bool randomizeZ = true;
    [Tooltip("ランダム配置するZ座標の範囲（最小値と最大値、randomizeZがオンの時のみ有効）")]
    public Vector2 randomZRange = new Vector2(-45f, 45f);

    [Header("自律体力ランダム設定")]
    [Tooltip("ゲーム開始時に体力を自動でランダムにするか")]
    public bool randomizeHPOnStart = true;
    [Tooltip("ランダム決定する最小体力")]
    public int minEnemyHP = 30;
    [Tooltip("ランダム決定する最大体力")]
    public int maxEnemyHP = 80;

    private DeathAction deathAction;
    private Rigidbody rb;
    private CharacterCore core;

    void Start()
    {
        // ★追加：敵ユニット単体で体力をランダム化する
        if (randomizeHPOnStart)
        {
            maxHP = Random.Range(minEnemyHP, maxEnemyHP + 1);
        }
        currentHP = maxHP;

        // ★追加：敵ユニット単体で座標をランダム化する
        if (randomizePositionOnStart)
        {
            float randomX = Random.Range(randomXRange.x, randomXRange.y);
            float randomZ = randomizeZ ? Random.Range(randomZRange.x, randomZRange.y) : transform.position.z;
            
            Vector3 targetPos = new Vector3(randomX, transform.position.y, randomZ);

            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                // NavMeshAgentが有効なままだと座標変更が適用されない場合があるため、一時的に無効化してワープ
                bool agentWasEnabled = agent.enabled;
                agent.enabled = false;

                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, 15.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    targetPos = hit.position;
                }
                
                transform.position = targetPos;
                agent.enabled = agentWasEnabled;
            }
            else
            {
                transform.position = targetPos;
            }
        }

        deathAction = GetComponent<DeathAction>();
        if (deathAction == null)
        {
            deathAction = gameObject.AddComponent<DeathAction>();
        }
        rb = GetComponent<Rigidbody>();
        core = GetComponent<CharacterCore>();

        // HPバーの初期設定
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;

            // ★追加：インスペクターで設定した色をHPバーのゲージ（Fill）に適用する
            if (hpSlider.fillRect != null)
            {
                Image fillImage = hpSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = hpBarColor;
                }
            }
        }

        // スポーンした瞬間にGameManagerに「敵が増えた」と報告
        if (GameManager.instance != null) 
        {
            GameManager.instance.AddEnemyCount();
            hasBeenCounted = true;
        }
    }

    private bool hasBeenCounted = false;

    private void OnDestroy()
    {
        // Spawnerによって間引かれるなど、倒される前に削除された場合の補正
        if (hasBeenCounted && currentHP > 0 && GameManager.instance != null)
        {
            GameManager.instance.RemoveEnemyCount();
        }
    }

   public void TakeDamage(int damage, bool isCritical = false)
    {
        TakeDamage(damage, Vector3.zero, 3.0f, 0.2f, isCritical);
    }

    public void TakeDamage(int damage, Vector3 knockbackDirection, float knockbackDist = 3.0f, float knockbackDur = 0.2f, bool isCritical = false)
    {
        if (currentHP <= 0) return;

        if (knockbackDirection.sqrMagnitude > 0.001f)
        {
            ApplyKnockback(knockbackDirection, knockbackDist, knockbackDur);
        }
        else
        {
            Debug.LogError("[EnemyStatus] ノックバック方向ベクトルが小さすぎるためノックバックしませんでした。");
        }

        currentHP -= damage;
        UpdateUI();
        
        // ダメージテキストの表示
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(transform.position + Vector3.up * 1.5f, damage, isCritical, false);
        }

        // アニメーターを取得（モデルが子オブジェクトにある場合も考慮）
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();

        if (currentHP <= 0)
        {
            currentHP = 0; 
            UpdateUI();

            if (GameManager.instance != null) GameManager.instance.RemoveEnemyCount();

            // DeathAction を使用して、物理挙動の停止、NavMeshAgent無効化、コライダー無効化などを実行する
            if (deathAction != null)
            {
                deathAction.ExecuteDeath();
            }
            else
            {
                // フォールバック（DeathAction がない場合）
                CharacterCore charCore = GetComponent<CharacterCore>();
                if (charCore != null) charCore.enabled = false;

                UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.enabled = false;

                if (anim != null) anim.SetTrigger("Die"); 
                Destroy(gameObject, 1.0f);
            }
        }
        else
        {
            // まだ生きている場合はダメージリアクションを再生する
            if (anim != null) anim.SetTrigger("Damage"); 
        }
    }

    private void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        direction.y = 0;
        if (direction.sqrMagnitude <= 0.001f)
        {
            Debug.LogError("[EnemyStatus] ノックバック方向(水平)が0になったためノックバックしませんでした。");
            return;
        }

        if (core != null)
        {
            if (distance <= 0f || duration <= 0f)
            {
                Debug.LogError($"[EnemyStatus] ノックバック距離({distance})または時間({duration})が不正なためノックバックしませんでした。");
                return;
            }
            core.TriggerKnockback(direction, distance, duration);
        }
        else
        {
            Debug.LogError("[EnemyStatus] CharacterCoreが見つからないためノックバックできません！");
        }
    }

    // ★追加：スポナーなど外部から最大HPを設定し、現在のHPとスライダーも即座に同期する
    public void SetMaxHP(int hp)
    {
        maxHP = hp;
        currentHP = hp;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        UpdateUI();
    }

    // ★追加：エラーの原因だった UpdateUI メソッドを作成
    private void UpdateUI()
    {
        // hpSliderが設定されていれば、現在のHPをスライダーのゲージに反映させる
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }
    }

    // 敵が消滅（Destroy）した瞬間に呼ばれる
    private float cutTimer = 0f;
    private int cutLevel = 0;
    private Coroutine cutCoroutine;

    public void ApplyCut(int level)
    {
        if (level <= 0 || currentHP <= 0) return;
        cutLevel = level;
        cutTimer += 5f; // ヒット数に応じて効果秒数が増える
        if (cutCoroutine == null)
        {
            cutCoroutine = StartCoroutine(CutRoutine());
        }
    }

    private System.Collections.IEnumerator CutRoutine()
    {
        while (cutTimer > 0 && currentHP > 0)
        {
            yield return new WaitForSeconds(1f);
            cutTimer -= 1f;
            
            // 4%にレベルアップごとの20%上昇を加算。MaxHPの割合とする。
            float multiplier = 1f + 0.2f * (cutLevel - 1);
            int damage = Mathf.Max(1, Mathf.RoundToInt(maxHP * 0.04f * multiplier));
            
            TakeDamage(damage, false); // 継続ダメージ
        }
        cutCoroutine = null;
    }

    private float slowTimer = 0f;
    private int slowLevel = 0;
    private Coroutine slowCoroutine;
    private float originalSpeed = -1f;

    public void ApplySlow(int level)
    {
        if (level <= 0 || currentHP <= 0 || core == null) return;
        slowLevel = level;
        slowTimer = level >= 5 ? 20f : 10f; // MAXで20秒間停止
        
        if (originalSpeed < 0) originalSpeed = core.speed;
        
        if (slowCoroutine == null)
        {
            slowCoroutine = StartCoroutine(SlowRoutine());
        }
    }

    private System.Collections.IEnumerator SlowRoutine()
    {
        while (slowTimer > 0 && currentHP > 0)
        {
            float slowRate = slowLevel >= 5 ? 1f : (0.2f * slowLevel); // MAXは100%遅延（停止）
            core.speed = originalSpeed * (1f - slowRate);
            
            yield return null;
            slowTimer -= Time.deltaTime;
        }
        
        if (core != null && originalSpeed >= 0)
        {
            core.speed = originalSpeed;
            originalSpeed = -1f;
        }
        slowCoroutine = null;
    }
}