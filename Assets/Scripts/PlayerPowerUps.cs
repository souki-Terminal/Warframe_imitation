using UnityEngine;

public class PlayerPowerUps : MonoBehaviour
{
    public static PlayerPowerUps instance;

    [Header("属性 (Max 5)")]
    public int cutLevel = 0;
    public int explosionLevel = 0;
    public int slowLevel = 0;
    public int lifestealLevel = 0;

    [Header("与ダメージアップ (Max 3)")]
    public int meleeDamageLevel = 0;
    public int rifleDamageLevel = 0;

    [Header("身体能力アップ")]
    public int healthLevel = 0;       // Max 5
    public int moveSpeedLevel = 0;    // Max 3

    [Header("防御 (Max 5)")]
    public int ironDefenseLevel = 0;  // 鉄壁の守り

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeUI();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private TMPro.TextMeshProUGUI statusUI;

    private void InitializeUI()
    {
        // GameManagerのUIの親(キャンバス)に新しくテキストを追加する
        if (GameManager.instance != null && GameManager.instance.notificationText != null)
        {
            Transform canvasTransform = GameManager.instance.notificationText.transform.parent;
            
            GameObject uiObj = new GameObject("PowerUpStatusUI");
            uiObj.transform.SetParent(canvasTransform, false);
            
            statusUI = uiObj.AddComponent<TMPro.TextMeshProUGUI>();
            statusUI.fontSize = 24; // 少し見やすく大きく
            statusUI.color = Color.white;
            
            RectTransform rect = statusUI.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 500);

            // 敵の数テキストの上部に配置する
            if (GameManager.instance.enemyCountText != null)
            {
                RectTransform enemyCountRect = GameManager.instance.enemyCountText.GetComponent<RectTransform>();
                rect.anchorMin = enemyCountRect.anchorMin;
                rect.anchorMax = enemyCountRect.anchorMax;
                // ピボットを下部(y=0)にして、上に伸びるようにする
                rect.pivot = new Vector2(enemyCountRect.pivot.x, 0);
                // 敵の数UIの少し上に配置
                rect.anchoredPosition = enemyCountRect.anchoredPosition + new Vector2(0, 30);
                statusUI.alignment = GameManager.instance.enemyCountText.alignment;
            }
            else
            {
                // フォールバック（左上）
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(20, -120);
                statusUI.alignment = TMPro.TextAlignmentOptions.TopLeft;
            }

            // フォントは通知UIから借りる
            statusUI.font = GameManager.instance.notificationText.font;

            UpdatePowerUpUI();
        }
    }

    private string FormatLevel(int level, int maxLevel = 3)
    {
        return level >= maxLevel ? "<color=#FF5555>MAX</color>" : $"Lv{level}";
    }

    public void UpdatePowerUpUI()
    {
        if (statusUI == null) return;

        string text = ""; // ヘッダーは削除
        bool hasAny = false;

        if (cutLevel > 0) { text += $"切断 {FormatLevel(cutLevel)}\n"; hasAny = true; }
        if (explosionLevel > 0) { text += $"爆発 {FormatLevel(explosionLevel)}\n"; hasAny = true; }
        if (slowLevel > 0) { text += $"遅延 {FormatLevel(slowLevel)}\n"; hasAny = true; }
        if (lifestealLevel > 0) { text += $"吸収 {FormatLevel(lifestealLevel)}\n"; hasAny = true; }
        if (meleeDamageLevel > 0) { text += $"近接ダメージ上昇 {FormatLevel(meleeDamageLevel)}\n"; hasAny = true; }
        if (rifleDamageLevel > 0) { text += $"ライフルダメージ上昇 {FormatLevel(rifleDamageLevel)}\n"; hasAny = true; }
        if (healthLevel > 0) { text += $"体力上昇 {FormatLevel(healthLevel)}\n"; hasAny = true; }
        if (moveSpeedLevel > 0) { text += $"移動速度上昇 {FormatLevel(moveSpeedLevel)}\n"; hasAny = true; }
        if (ironDefenseLevel > 0) { text += $"鉄壁の守り {FormatLevel(ironDefenseLevel)}\n"; hasAny = true; }

        if (!hasAny)
        {
            text += "";
        }

        statusUI.text = text;
    }
}
