using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    [Header("UI参照")]
    public Transform buttonsContainer; // ボタンを配置する親オブジェクト(HorizontalLayoutGroupなど)
    public GameObject powerUpButtonPrefab; // 生成するボタンのプレハブ

    // アイテムのデータ構造
    public class PowerUpItem
    {
        public string title;
        public System.Func<int, string> getDescription;
        public System.Func<bool> canUpgrade;
        public System.Action<int> applyEffect;
    }

    private List<PowerUpItem> allPowerUps;

    void Start()
    {
        // 選択肢の初期化
        allPowerUps = new List<PowerUpItem>
        {
            new PowerUpItem { 
                title = "切断", 
                getDescription = (boost) => $"5秒間継続ダメージを与える（レベルアップで威力上昇）。\n現在Lv: {PlayerPowerUps.instance.cutLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.cutLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.cutLevel = Mathf.Min(3, PlayerPowerUps.instance.cutLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                }
            },
            new PowerUpItem { 
                title = "爆発", 
                getDescription = (boost) => $"ヒット時に周囲の敵へダメージ（レベルアップで威力上昇）。\n現在Lv: {PlayerPowerUps.instance.explosionLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.explosionLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.explosionLevel = Mathf.Min(3, PlayerPowerUps.instance.explosionLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                }
            },
            new PowerUpItem { 
                title = "遅延", 
                getDescription = (boost) => $"10秒間相手の動きを遅くする（LvMAXで停止）。\n現在Lv: {PlayerPowerUps.instance.slowLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.slowLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.slowLevel = Mathf.Min(3, PlayerPowerUps.instance.slowLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                }
            },
            new PowerUpItem { 
                title = "吸収", 
                getDescription = (boost) => $"武器ダメージの一部を体力として回復。\n現在Lv: {PlayerPowerUps.instance.lifestealLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.lifestealLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.lifestealLevel = Mathf.Min(3, PlayerPowerUps.instance.lifestealLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                }
            },
            new PowerUpItem { 
                title = "近接ダメージ上昇", 
                getDescription = (boost) => $"基礎近接ダメージが上昇。\n現在Lv: {PlayerPowerUps.instance.meleeDamageLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.meleeDamageLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.meleeDamageLevel = Mathf.Min(3, PlayerPowerUps.instance.meleeDamageLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                    Damager w = FindFirstObjectByType<Damager>();
                    if (w != null) w.RecalculateDamage();
                }
            },
            new PowerUpItem { 
                title = "体力上昇", 
                getDescription = (boost) => $"最大体力が上昇。\n現在Lv: {PlayerPowerUps.instance.healthLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.healthLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.healthLevel = Mathf.Min(3, PlayerPowerUps.instance.healthLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                    PlayerStatus p = FindFirstObjectByType<PlayerStatus>();
                    if (p != null) p.RecalculateMaxHP();
                }
            },
            new PowerUpItem { 
                title = "移動速度上昇", 
                getDescription = (boost) => $"移動速度が上昇。\n現在Lv: {PlayerPowerUps.instance.moveSpeedLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.moveSpeedLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.moveSpeedLevel = Mathf.Min(3, PlayerPowerUps.instance.moveSpeedLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                    PlayerStatus p = FindFirstObjectByType<PlayerStatus>();
                    if (p != null) p.RecalculateSpeedAndJump();
                }
            },
            new PowerUpItem {
                title = "鉄壁の守り",
                getDescription = (boost) => $"受けるダメージを軽減します。\n現在Lv: {PlayerPowerUps.instance.ironDefenseLevel} / 3",
                canUpgrade = () => PlayerPowerUps.instance.ironDefenseLevel < 3,
                applyEffect = (boost) => {
                    PlayerPowerUps.instance.ironDefenseLevel = Mathf.Min(3, PlayerPowerUps.instance.ironDefenseLevel + boost);
                    PlayerPowerUps.instance.UpdatePowerUpUI();
                }
            }
        };
    }

    public void ShowChoices(int optionsCount, bool isTypingGame = false, bool isPerfectTyping = false)
    {
        if (GameManager.instance == null) return;
        
        GameManager.instance.powerUpPanel.SetActive(true);

        // ボタンのクリア
        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }

        if (optionsCount <= 0)
        {
            // 選択肢0の場合、強制的に次へ
            StartCoroutine(ShowZeroOptionsMessage());
            return;
        }

        // アップグレード可能なものだけを抽出
        List<PowerUpItem> availableItems = new List<PowerUpItem>();
        foreach(var item in allPowerUps)
        {
            if (item.canUpgrade())
            {
                availableItems.Add(item);
            }
        }

        if (availableItems.Count == 0)
        {
            // 全て取得済みの場合
            StartCoroutine(ShowZeroOptionsMessage());
            return;
        }

        // リストをシャッフルして指定個数だけ取り出す
        List<PowerUpItem> shuffled = new List<PowerUpItem>(availableItems);
        for (int i = 0; i < shuffled.Count; i++)
        {
            PowerUpItem temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        int count = Mathf.Min(optionsCount, shuffled.Count);

        // タイピング完全成功の場合は必ず1つはLv3を出現させる
        int guaranteedLv3Index = -1;
        if (isPerfectTyping && count > 0)
        {
            guaranteedLv3Index = Random.Range(0, count);
        }

        for (int i = 0; i < count; i++)
        {
            PowerUpItem item = shuffled[i];
            
            // レベルブーストの抽選
            int levelBoost = 1;
            if (i == guaranteedLv3Index)
            {
                levelBoost = 3;
            }
            else
            {
                float rand = Random.value;
                float chanceLv2 = isTypingGame ? 0.15f : 0.05f;
                float chanceLv3 = isTypingGame ? 0.15f : 0.05f;

                if (rand < chanceLv3)
                {
                    levelBoost = 3;
                }
                else if (rand < chanceLv3 + chanceLv2)
                {
                    levelBoost = 2;
                }
            }

            GameObject btnObj = Instantiate(powerUpButtonPrefab, buttonsContainer);
            
            // ボタンのテキスト設定 (タイトルと説明)
            TextMeshProUGUI[] texts = btnObj.GetComponentsInChildren<TextMeshProUGUI>();
            string titleWithBoost = levelBoost > 1 ? $"{item.title} <color=#FFD700>(+Lv{levelBoost})</color>" : item.title;
            if (texts.Length > 0) texts[0].text = titleWithBoost;
            if (texts.Length > 1) texts[1].text = item.getDescription(levelBoost);

            Button btn = btnObj.GetComponent<Button>();
            int capturedBoost = levelBoost;
            btn.onClick.AddListener(() => OnPowerUpSelected(item, capturedBoost));
        }

        // 最初のボタンを選択状態にする
        GameManager.instance.SelectFirstActiveButtonInPanel(GameManager.instance.powerUpPanel);
    }

    private System.Collections.IEnumerator ShowZeroOptionsMessage()
    {
        GameManager.instance.ShowNotification("正解数が足りず、パワーアップを獲得できませんでした...");
        yield return new WaitForSecondsRealtime(3.0f);
        GameManager.instance.ProceedToNextWave();
    }

    private void OnPowerUpSelected(PowerUpItem item, int levelBoost)
    {
        // 効果適用
        item.applyEffect?.Invoke(levelBoost);
        GameManager.instance.ShowNotification($"{item.title} (+Lv{levelBoost}) を獲得！");

        // 次のウェーブへ
        GameManager.instance.ProceedToNextWave();
    }
}
