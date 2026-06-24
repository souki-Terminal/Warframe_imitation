using UnityEngine;
using UnityEngine.UI; // スライダー(HPバー)を使うために必要

[RequireComponent(typeof(DeathAction))]
public class EnemyStatus : MonoBehaviour
{
    [Header("敵のステータス")]
    public int maxHP = 50;
    private int currentHP;

    [Header("ノックバック")]
    public float knockbackForce = 3.0f;
    public float knockbackUpForce = 0.8f;

    [Header("UI設定")]
    [Tooltip("敵の頭上に出すHPスライダー")]
    public Slider hpSlider;

    private DeathAction deathAction;
    private Rigidbody rb;
    private CharacterCore core;

    void Start()
    {
        currentHP = maxHP;
        deathAction = GetComponent<DeathAction>();
        rb = GetComponent<Rigidbody>();
        core = GetComponent<CharacterCore>();

        // HPバーの初期設定
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // スポーンした瞬間にGameManagerに「敵が増えた」と報告
        if (GameManager.instance != null) GameManager.instance.AddEnemyCount();
    }

   public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    public void TakeDamage(int damage, Vector3 knockbackDirection)
    {
        if (currentHP <= 0) return;

        if (knockbackDirection.sqrMagnitude > 0.001f)
        {
            ApplyKnockback(knockbackDirection);
        }

        currentHP -= damage;
        UpdateUI();

        // アニメーターを取得（モデルが子オブジェクトにある場合も考慮）
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();

        if (currentHP <= 0)
        {
            currentHP = 0; 
            UpdateUI();

            if (GameManager.instance != null) GameManager.instance.RemoveEnemyCount();

            CharacterCore charCore = GetComponent<CharacterCore>();
            if (charCore != null) charCore.enabled = false;

            // 死亡時は "Die" トリガーを呼ぶように修正（元はDamageになっていました）
            if (anim != null) anim.SetTrigger("Die"); 

            Destroy(gameObject, 1.0f);
        }
        else
        {
            // ★修正：まだ生きている場合はダメージリアクションを再生する
            if (anim != null) anim.SetTrigger("Damage"); 
        }
    }

    private void ApplyKnockback(Vector3 direction)
    {
        if (rb == null) return;

        direction.y = 0;
        if (direction.sqrMagnitude <= 0.001f) return;

        rb.linearVelocity = new Vector3(direction.normalized.x * knockbackForce, knockbackUpForce, direction.normalized.z * knockbackForce);

        if (core != null)
        {
            core.TriggerKnockback(0.2f); // 0.2秒間ノックバックの物理移動を優先し、通常の移動や摩擦による減速をバイパスする
        }
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
}