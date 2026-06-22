using System.Collections;
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

    private float survivalTime = 0f;
    private bool isPlayerDead = false;
    private int currentEnemyCount = 0;

    void Awake()
    {
        instance = this; 
    }

    void Start()
    {
        gameOverUI.SetActive(false);
        UpdateEnemyCountText();
        Time.timeScale = 1f; 
    }

    private GameObject lastSelectedUI;

    void Update()
    {
        if (!isPlayerDead && Time.timeScale > 0)
        {
            survivalTime += Time.deltaTime;
        }

        // リザルト画面表示中の選択状態の維持・キー入力処理
        if (gameOverUI != null && gameOverUI.activeInHierarchy)
        {
            HandleUIInput(gameOverUI);
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

    private void SelectFirstActiveButtonInPanel(GameObject panel)
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
            var colors = button.colors;
            // 選択時・ハイライト時がはっきりわかるように鮮やかな色（薄い青色系など）に設定する
            colors.selectedColor = new Color(0.7f, 0.85f, 1f, 1f);   // 水色
            colors.highlightedColor = new Color(0.8f, 0.9f, 1f, 1f); // 少し明るい水色
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
        UpdateEnemyCountText();

        if (defeatedEnemyCount >= totalEnemyCount)
        {
            StartCoroutine(WaitAndClear(1.0f)); 
        }
    }

    private IEnumerator WaitAndClear(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameClear();
    }

    private void GameClear()
    {
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
            enemyCountText.text = "Enemies: " + defeatedEnemyCount + " / " + totalEnemyCount;
        }
    }
}