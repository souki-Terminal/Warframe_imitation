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
    private Animator anim;
    private PlayerControllerReal playerController;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody>();
        core = GetComponent<CharacterCore>();
        
        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();
        
        playerController = GetComponent<PlayerControllerReal>();
        
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    [Header("防御関連")]
    // 鉄壁の守りレベルに応じて軽減（1レベル10%）
    public float damageReduction => PlayerPowerUps.instance != null ? PlayerPowerUps.instance.ironDefenseLevel * 0.1f : 0f;

    public void TakeDamage(int damage, Vector3 knockbackDirection, float knockbackDist = 3.0f, float knockbackDur = 0.2f)
    {
        if (currentHP <= 0) return;

        // ダメージ軽減処理 (被ダメージ軽減のパワーアップ対応)
        // （拡張性を残すための変数。現状はそのままのダメージが入るか、外部から操作される）
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * (1.0f - damageReduction)));

        if (knockbackDirection.sqrMagnitude > 0.001f)
        {
            ApplyKnockback(knockbackDirection, knockbackDist, knockbackDur);
        }
        else
        {
            Debug.LogError("[PlayerStatus] ノックバック方向ベクトルが小さすぎるためノックバックしませんでした。");
        }

        currentHP -= finalDamage;
        UpdateUI();

        // ダメージテキストの表示（プレイヤーは赤色）
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageText(transform.position + Vector3.up * 1.5f, finalDamage, false, true);
        }

        if (currentHP <= 0)
        {
            currentHP = 0; // マイナス表示を防ぐ
            UpdateUI();

            // ★追加：死亡した瞬間にプレイヤーの移動操作とCharacterCoreを停止する
            if (playerController != null) playerController.enabled = false;
            if (core != null) core.enabled = false;

            // リジッドボディがある場合、速度もリセットしてその場に留まらせる
            if (rb != null) rb.linearVelocity = Vector3.zero;

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

    private void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        direction.y = 0;
        if (direction.sqrMagnitude <= 0.001f)
        {
            Debug.LogError("[PlayerStatus] ノックバック方向(水平)が0になったためノックバックしませんでした。");
            return;
        }

        if (core != null)
        {
            if (distance <= 0f || duration <= 0f)
            {
                Debug.LogError($"[PlayerStatus] ノックバック距離({distance})または時間({duration})が不正なためノックバックしませんでした。");
                return;
            }
            core.TriggerKnockback(direction, distance, duration);
        }
        else
        {
            Debug.LogError("[PlayerStatus] CharacterCoreが見つからないためノックバックできません！");
        }
    }

    public void RecalculateMaxHP()
    {
        if (PlayerPowerUps.instance == null) return;
        
        // 体力レベルアップごとに×5
        int multiplier = 1;
        for (int i = 0; i < PlayerPowerUps.instance.healthLevel; i++) multiplier *= 5;

        int newMaxHP = 400 * multiplier; // 初期体力400想定
        int diff = newMaxHP - maxHP;
        
        if (diff > 0)
        {
            maxHP = newMaxHP;
            currentHP += diff; // 上限が増えた分だけ現在HPも回復
            UpdateUI();
        }
    }

    public void RecalculateSpeedAndJump()
    {
        if (core == null || PlayerPowerUps.instance == null) return;

        // 移動速度：レベル1で×2、レベル2で×3、レベル3で×4
        int speedMultiplier = PlayerPowerUps.instance.moveSpeedLevel + 1;
        core.speed = 5.0f * speedMultiplier; // 初期速度5想定
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
    public void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHP;
        
        if (hpText != null)
        {
            hpText.text = currentHP + " / " + maxHP; // 画面に「100 / 100」のように表示
        }
    }
}