using UnityEngine;
using UnityEngine.UI; // スライダー(HPバー)を使うために必要

[RequireComponent(typeof(DeathAction))]
public class EnemyStatus : MonoBehaviour
{
    [Header("敵のステータス")]
    public int maxHP = 50;
    private int currentHP;

    [Header("UI設定")]
    [Tooltip("敵の頭上に出すHPスライダー")]
    public Slider hpSlider;

    private DeathAction deathAction;

    void Start()
    {
        currentHP = maxHP;
        deathAction = GetComponent<DeathAction>();

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
        if (currentHP <= 0) return;

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