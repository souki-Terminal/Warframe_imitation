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
        public string description;
        public System.Action applyEffect;
    }

    private List<PowerUpItem> allPowerUps;

    void Start()
    {
        // 選択肢の初期化
        allPowerUps = new List<PowerUpItem>
        {
            new PowerUpItem { 
                title = "体力上限突破", 
                description = "最大HPが50増加します", 
                applyEffect = () => { 
                    PlayerStatus p = FindFirstObjectByType<PlayerStatus>(); 
                    if(p != null) p.HealAndBuffMaxHP(50); 
                } 
            },
            new PowerUpItem { 
                title = "神速の脚", 
                description = "移動速度が少し上がります", 
                applyEffect = () => { 
                    CharacterCore c = FindFirstObjectByType<CharacterCore>(); 
                    if(c != null) c.speed += 1.5f; 
                } 
            },
            new PowerUpItem { 
                title = "軽業師", 
                description = "ジャンプ力が増加します", 
                applyEffect = () => { 
                    CharacterCore c = FindFirstObjectByType<CharacterCore>(); 
                    if(c != null) c.jumpForce += 2.0f; 
                } 
            },
            new PowerUpItem { 
                title = "完全回復", 
                description = "HPが最大まで回復します", 
                applyEffect = () => { 
                    PlayerStatus p = FindFirstObjectByType<PlayerStatus>(); 
                    if(p != null) p.HealToFull(); 
                } 
            },
            new PowerUpItem { 
                title = "鋭い一撃", 
                description = "武器のダメージが追加で10%上昇", 
                applyEffect = () => { 
                    Damager w = FindFirstObjectByType<Damager>(); 
                    if(w != null) w.damage = (int)(w.damage * 1.1f); 
                } 
            }
        };
    }

    public void ShowChoices(int optionsCount)
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

        // リストをシャッフルして指定個数だけ取り出す
        List<PowerUpItem> shuffled = new List<PowerUpItem>(allPowerUps);
        for (int i = 0; i < shuffled.Count; i++)
        {
            PowerUpItem temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        int count = Mathf.Min(optionsCount, shuffled.Count);
        for (int i = 0; i < count; i++)
        {
            PowerUpItem item = shuffled[i];
            GameObject btnObj = Instantiate(powerUpButtonPrefab, buttonsContainer);
            
            // ボタンのテキスト設定 (タイトルと説明)
            TextMeshProUGUI[] texts = btnObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = item.title;
            if (texts.Length > 1) texts[1].text = item.description;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnPowerUpSelected(item));
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

    private void OnPowerUpSelected(PowerUpItem item)
    {
        // 効果適用
        item.applyEffect?.Invoke();
        GameManager.instance.ShowNotification($"{item.title} を獲得！");

        // 次のウェーブへ
        GameManager.instance.ProceedToNextWave();
    }
}
