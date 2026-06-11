using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★数字テキストを扱うために必要

public class PlayerStatus : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;

    [Header("UI設定")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText; // ★追加：HPの数値を表示する枠

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        UpdateUI();

        if (currentHP <= 0)
        {
            currentHP = 0; // マイナス表示を防ぐ
            UpdateUI();

            // ★ここでGameManagerに「死んだよ！」と報告する（これを忘れると画面が出ません）
            if (GameManager.instance != null)
            {
                GameManager.instance.OnPlayerDied();
            }

            // 死亡アニメーション（HitやDamageなど、設定した名前に合わせてください）
            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Damage"); 
        }
    }

    public void HealAndBuffMaxHP(int amount)
    {
        maxHP += amount;
        currentHP += amount;
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