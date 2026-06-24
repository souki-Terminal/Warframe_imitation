using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★数字テキストを扱うために必要

public class PlayerStatus : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;

    [Header("ノックバック")]
    public float knockbackForce = 5.0f;
    public float knockbackUpForce = 1.0f;

    [Header("UI設定")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText; // ★追加：HPの数値を表示する枠

    private Rigidbody rb;
    private CharacterCore core;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody>();
        core = GetComponent<CharacterCore>();
        UpdateUI();
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

        // アニメーターを取得
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();

        if (currentHP <= 0)
        {
            currentHP = 0; // マイナス表示を防ぐ
            UpdateUI();

            // ★追加：死亡した瞬間にプレイヤーの移動操作とCharacterCoreを停止する
            PlayerControllerReal controller = GetComponent<PlayerControllerReal>();
            if (controller != null) controller.enabled = false;

            CharacterCore charCore = GetComponent<CharacterCore>();
            if (charCore != null) charCore.enabled = false;

            // リジッドボディがある場合、速度もリセットしてその場に留まらせる
            Rigidbody playerRb = GetComponent<Rigidbody>();
            if (playerRb != null) playerRb.linearVelocity = Vector3.zero;

            if (GameManager.instance != null)
            {
                GameManager.instance.OnPlayerDied();
            }

            // 死亡アニメーション（Dieなどに設定している場合は名前に合わせてください）
            if (anim != null) anim.SetTrigger("Die"); 
        }
        else
        {
            // ★追加：まだ生きている場合はダメージリアクションを再生する
            if (anim != null) anim.SetTrigger("Damage");
        }
    }

    private void ApplyKnockback(Vector3 direction)
    {
        direction.y = 0;
        if (direction.sqrMagnitude <= 0.001f) return;

        // ★修正：力を加える物理移動ではなく、CharacterCoreに座標を直接3動かすように指示する
        if (core != null)
        {
            core.TriggerKnockback(direction, 3.0f, 0.2f);
        }
    }

    public void HealAndBuffMaxHP(int amount)
    {
        maxHP += amount;
        currentHP += amount;
        UpdateUI();
    }

    // ★追加：体力を完全に最大値まで回復する
    public void HealToFull()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    // UI（バーと数値）を同時に更新する便利な処理
    private void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHP;
        
        if (hpText != null)
        {
            hpText.text = currentHP + " / " + maxHP; // 画面に「100 / 100」のように表示
        }
    }
}