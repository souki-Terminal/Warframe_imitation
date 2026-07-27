using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI設定")]
    public GameObject gameOverUI;    
    // ★追加：大きく「GameOver」と表示されているテキストを指定するための枠
    public TextMeshProUGUI resultTitleText; 
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI enemyCountText; 
    
    [Header("通知UI設定")]
    public TextMeshProUGUI notificationText; 

    [Header("タイピング・パワーアップ関連UI")]
    public GameObject typingChoicePanel; // タイピング挑戦確認UI
    public GameObject typingGamePanel;   // タイピング画面UI
    public GameObject powerUpPanel;      // パワーアップ選択UI

    private EnemySpawner pendingSpawner;

    private float survivalTime = 0f;
    private bool isPlayerDead = false;

    // オープンキャンパス向け：パワーアップ権利
    public int powerUpRights = 0;
    private int consecutiveKillsForPowerUp = 0;

    void Awake()
    {
        instance = this; 
        
        // パワーアップ状態の管理クラスを自動追加（シングルトンとして動作）
        if (GetComponent<PlayerPowerUps>() == null)
        {
            gameObject.AddComponent<PlayerPowerUps>();
        }
    }

    // --- スポナー（ウェーブ）の管理 ---
    private List<EnemySpawner> spawners = new List<EnemySpawner>();
    private int currentSpawnerIndex = 0;
    private Queue<string> notificationQueue = new Queue<string>();
    private bool isDisplayingNotification = false;

    void Start()
    {
        // BGM再生（OpenCampusSpawnerがない場合のみ。ある場合はそちらで制御する）
        gameOverUI.SetActive(false);
        UpdateEnemyCountText();
        Time.timeScale = 1f; 

        OpenCampusSpawner spawner = FindFirstObjectByType<OpenCampusSpawner>();
        if (spawner == null)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGameBGM();

            // シーン内のすべての EnemySpawner を取得する (OpenCampusSpawnerがない場合のみ)
            EnemySpawner[] foundSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
            spawners = new List<EnemySpawner>(foundSpawners);

            // 敵のレベル名（例: Enemy_Lv1, Enemy_Lv10 等）に含まれる数値でソートする。
            // 数値が含まれない、または同一の場合は spawnStartTime でソートする。
            spawners.Sort((a, b) => {
                int aLv = GetLevelFromName(a.name);
                int bLv = GetLevelFromName(b.name);
                if (aLv != bLv)
                {
                    return aLv.CompareTo(bLv);
                }
                return a.spawnStartTime.CompareTo(b.spawnStartTime);
            });

            // 最初のスポナーを開始する
            if (spawners.Count > 0)
            {
                currentSpawnerIndex = 0;
                spawners[currentSpawnerIndex].StartSpawning();
            }
        }
        else
        {
            // OpenCampusSpawnerがある場合は、EnemySpawnerシステムを完全に無効化する
            spawners = new List<EnemySpawner>();
        }
    }

    private int GetLevelFromName(string name)
    {
        int index = name.IndexOf("Enemy_Lv");
        if (index >= 0)
        {
            string numStr = name.Substring(index + 8);
            string digits = "";
            foreach (char c in numStr)
            {
                if (char.IsDigit(c)) digits += c;
                else break;
            }
            if (int.TryParse(digits, out int val))
            {
                return val;
            }
        }
        return 0;
    }

    public void OnSpawnerCleared(EnemySpawner spawner)
    {
        pendingSpawner = spawner;

        // オープンキャンパス版：クリア時は選択UIを出さずにすぐ次へ行く
        ProceedToNextWave();
    }

    public void OnTypingChoiceYes()
    {
        if (typingChoicePanel != null) typingChoicePanel.SetActive(false);
        if (typingGamePanel != null) typingGamePanel.SetActive(true);
        var tm = GetComponent<TypingManager>();
        if (tm != null) tm.StartChallenge();
    }

    public void OnTypingChoiceNo()
    {
        if (typingChoicePanel != null) typingChoicePanel.SetActive(false);
        var pm = GetComponent<PowerUpManager>();
        if (pm != null) pm.ShowChoices(3); // タイピングをしない場合は通常の3択
    }

    public void ProceedToNextWave()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (powerUpPanel != null) powerUpPanel.SetActive(false);
        if (typingChoicePanel != null) typingChoicePanel.SetActive(false);
        if (typingGamePanel != null) typingGamePanel.SetActive(false);

        if (spawners.Count > 0 && currentSpawnerIndex < spawners.Count && spawners[currentSpawnerIndex] == pendingSpawner)
        {
            currentSpawnerIndex++;
            if (currentSpawnerIndex < spawners.Count)
            {
                ApplyWaveClearBuff();

                string msg = $"次のウェーブを開始します: {spawners[currentSpawnerIndex].name}";
                Debug.Log(msg);
                ShowNotification($"ウェーブ開始: {spawners[currentSpawnerIndex].name}");
                spawners[currentSpawnerIndex].StartSpawning();
            }
            else
            {
                // すべてのウェーブがクリアされた
                string msg = "すべてのウェーブがクリアされました！";
                Debug.Log(msg);
                ShowNotification("全ウェーブクリア！");
                StartCoroutine(WaitAndClear(1.0f));
            }
        }
    }

    private void ApplyWaveClearBuff()
    {
        if (pendingSpawner == null) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayWaveClear();

        string msg = "ウェーブクリア！プレイヤーを強化します！";
        Debug.Log(msg);

        if (pendingSpawner.playerStatus != null)
        {
            pendingSpawner.playerStatus.HealAndBuffMaxHP(20);
            pendingSpawner.playerStatus.HealToFull(); 
        }

        string uiMsg = "ウェーブクリア！";

        if (pendingSpawner.playerWeapon != null)
        {
            long nextDamage = (long)pendingSpawner.playerWeapon.damage * 3;
            if (nextDamage > int.MaxValue - 100000)
            {
                pendingSpawner.playerWeapon.damage = int.MaxValue - 100000;
            }
            else
            {
                pendingSpawner.playerWeapon.damage = (int)nextDamage;
            }
            string buffMsg = $"武器のダメージが強化された！ (現在: {pendingSpawner.playerWeapon.damage})";
            Debug.Log(buffMsg);
            uiMsg += $"\n武器のダメージが強化されました！ (現在: {pendingSpawner.playerWeapon.damage})";
        }

        ShowNotification(uiMsg);
    }

    private void CreateDynamicNotificationText()
    {
        if (notificationText != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject go = new GameObject("DynamicNotificationText");
        go.transform.SetParent(canvas.transform, false);

        notificationText = go.AddComponent<TextMeshProUGUI>();
        notificationText.text = ""; // 「New Text」という初期文字列を消去する
        
        // ★修正：確実に「NotoSerifJP」等の日本語フォントを持っているテキストを探して拝借する
        // ※パネルが非表示（Inactive）の状態でも検索できるように FindObjectsInactive.Include を追加！
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var text in allTexts)
        {
            if (text.font != null && text.font.name.Contains("NotoSerif"))
            {
                notificationText.font = text.font;
                break;
            }
        }
        
        notificationText.fontSize = 28;
        notificationText.fontStyle = FontStyles.Bold;
        notificationText.alignment = TextAlignmentOptions.Center;
        
        // グラデーションとアウトラインを設定して見やすくする
        notificationText.enableVertexGradient = true;
        notificationText.colorGradient = new VertexGradient(
            Color.white, Color.white,
            new Color(1f, 0.85f, 0.2f), new Color(1f, 0.85f, 0.2f)
        );
        notificationText.outlineWidth = 0.2f;
        notificationText.outlineColor = Color.black;

        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -100f);
            rect.sizeDelta = new Vector2(800f, 150f);
        }
    }

    public void ShowNotification(string message)
    {
        notificationQueue.Enqueue(message);
        if (!isDisplayingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }

    private IEnumerator ProcessNotificationQueue()
    {
        isDisplayingNotification = true;

        if (notificationText == null)
        {
            CreateDynamicNotificationText();
        }

        while (notificationQueue.Count > 0)
        {
            string message = notificationQueue.Dequeue();
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            yield return new WaitForSecondsRealtime(3.0f);
        }

        if (notificationText != null)
        {
            notificationText.text = "";
        }
        isDisplayingNotification = false;
    }

    private GameObject lastSelectedUI;

    void Update()
    {
        if (!isPlayerDead && Time.timeScale > 0)
        {
            survivalTime += Time.deltaTime;
        }

        // Qキーによるパワーアップ発動
        if (Input.GetKeyDown(KeyCode.Q) && powerUpRights > 0 && Time.timeScale > 0)
        {
            powerUpRights--;
            UpdateEnemyCountText();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnTypingChoiceYes();
        }

        // 各UIパネル表示中の選択状態の維持・キー入力処理
        if (gameOverUI != null && gameOverUI.activeInHierarchy)
        {
            HandleUIInput(gameOverUI);
        }
        else if (typingChoicePanel != null && typingChoicePanel.activeInHierarchy)
        {
            HandleUIInput(typingChoicePanel);
        }
        else if (powerUpPanel != null && powerUpPanel.activeInHierarchy)
        {
            HandleUIInput(powerUpPanel);
        }
    }

    private void HandleUIInput(GameObject panel)
    {
        EnsureEventSystemExists();

        if (UnityEngine.EventSystems.EventSystem.current == null) return;

        // 1. マウスクリック等で選択が解除された場合、十字キー・矢印キー入力時に再選択する
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                if (lastSelectedUI != null && lastSelectedUI.activeInHierarchy)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(lastSelectedUI);
                }
                else
                {
                    SelectFirstActiveButtonInPanel(panel);
                }
            }
        }
        else
        {
            // 現在選択されているオブジェクトがパネル内のものである場合のみ記録
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.IsChildOf(panel.transform))
            {
                lastSelectedUI = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            }
        }

        // 2. Enterキー押下時のクリック処理（Submitアクションのフォールバック）
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            GameObject selectedObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (selectedObj != null && selectedObj.transform.IsChildOf(panel.transform))
            {
                UnityEngine.UI.Button button = selectedObj.GetComponent<UnityEngine.UI.Button>();
                if (button != null && button.interactable)
                {
                    button.onClick.Invoke();
                }
            }
        }
    }

    public void SelectFirstActiveButtonInPanel(GameObject panel)
    {
        if (panel == null) return;
        StartCoroutine(SelectFirstActiveButtonRoutine(panel));
    }

    private IEnumerator SelectFirstActiveButtonRoutine(GameObject panel)
    {
        // EventSystem の存在を確認・自動生成
        EnsureEventSystemExists();

        // ボタンの選択色・ハイライト色をはっきりと目立つ色に変更する
        AdjustButtonColors(panel);

        // UI が有効化され、初期化が完了するまで1フレーム待つ
        yield return null;

        if (panel == null || UnityEngine.EventSystems.EventSystem.current == null) yield break;

        UnityEngine.UI.Button[] buttons = panel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        foreach (var button in buttons)
        {
            if (button.gameObject.activeInHierarchy && button.interactable)
            {
                // 一旦nullを入れることで確実にフォーカスイベントを発火させる
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
                lastSelectedUI = button.gameObject;
                break;
            }
        }
    }

    private void EnsureEventSystemExists()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            var existing = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existing == null)
            {
                GameObject go = new GameObject("EventSystem_AutoCreated");
                go.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
                go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
                Debug.Log("EventSystem was missing, automatically created one for UI navigation.");
            }
        }
    }

    private void AdjustButtonColors(GameObject panel)
    {
        if (panel == null) return;
        UnityEngine.UI.Button[] buttons = panel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        foreach (var button in buttons)
        {
            // ★追加：ベースの画像が真っ黒だとColorTintが乗らないため、ベースを白に変更する
            UnityEngine.UI.Image img = button.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = Color.white;
            }

            var colors = button.colors;
            // 未選択時は元の黒っぽい色にする
            colors.normalColor = new Color(0.1f, 0.1f, 0.1f, 1f); 
            // 選択時・ハイライト時がはっきりわかるように鮮やかな色に設定する
            colors.selectedColor = new Color(0.2f, 0.6f, 1f, 1f);   // 鮮やかな水色
            colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f); // 少し明るい水色
            colors.pressedColor = new Color(0.1f, 0.4f, 0.8f, 1f);
            
            button.colors = colors;
        }
    }

    public void OnPlayerDied()
    {
        if (isPlayerDead) return;
        isPlayerDead = true;
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 0f;
        gameOverUI.SetActive(true);

        // ★追加：カーソルを再び表示して、クリックできるようにする！
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ★追加：Result画面表示時、最初のボタンを選択状態にする
        SelectFirstActiveButtonInPanel(gameOverUI);

        // ★追加：念のためタイトル文字を「Game Over」にしておく
        if (resultTitleText != null) resultTitleText.text = "Game Over";

        if (survivalTimeText != null)
        {
            survivalTimeText.text = "Survival Time: \n" + survivalTime.ToString("F1") + "s";
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    // --- 敵の数管理 ---
    private int totalEnemyCount = 0;      
    private int defeatedEnemyCount = 0;   

    public void AddEnemyCount()
    {
        totalEnemyCount++; 
        UpdateEnemyCountText();
    }

    public void RemoveEnemyCount()
    {
        defeatedEnemyCount++; 
        
        // 5体倒すごとにパワーアップ権利獲得
        consecutiveKillsForPowerUp++;
        if (consecutiveKillsForPowerUp >= 5)
        {
            powerUpRights++;
            consecutiveKillsForPowerUp -= 5;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPowerUpRightGet();
            ShowNotification("パワーアップ獲得！[Q]キーで発動");
        }

        UpdateEnemyCountText();

        if (defeatedEnemyCount == 5)
        {
            OpenCampusSpawner spawner = FindFirstObjectByType<OpenCampusSpawner>();
            if (spawner != null)
            {
                spawner.OnPracticeWaveCleared();
            }
        }

        if (defeatedEnemyCount >= totalEnemyCount)
        {
            // スポナーが存在しないシーン、またはすでに全スポナーがクリア済みの場合はクリア判定を行う
            // ただし、練習ウェーブ（5体）を倒した直後はまだクリアにしない
            if (defeatedEnemyCount > 5 && (spawners == null || spawners.Count == 0 || currentSpawnerIndex >= spawners.Count))
            {
                StartCoroutine(WaitAndClear(1.0f)); 
            }
        }
    }

    private IEnumerator WaitAndClear(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameClear();
    }

    private void GameClear()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameClear();

        Time.timeScale = 0f;
        gameOverUI.SetActive(true); 

        // ★追加：カーソルを再び表示して、クリックできるようにする！
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ★追加：Result画面表示時、最初のボタンを選択状態にする
        SelectFirstActiveButtonInPanel(gameOverUI);

        // ★追加：クリア時は文字を「Finish」に書き換える！
        if (resultTitleText != null)
        {
            resultTitleText.text = "Finish";
        }
        
        if (survivalTimeText != null)
        {
            survivalTimeText.text = "Survival Time: \n" + survivalTime.ToString("F1") + "s";
        }
    }

    private void UpdateEnemyCountText()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = "Enemies: " + defeatedEnemyCount + " / " + totalEnemyCount + "\n[Q] PowerUps: " + powerUpRights;
        }
    }
}