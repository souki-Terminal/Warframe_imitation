using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移の処理に必須です
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("シーン遷移設定")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI色設定")]
    [SerializeField] private Color defaultSelectedColor = new Color(0.7f, 0.85f, 1f, 1f);
    [SerializeField] private Color defaultHighlightedColor = new Color(0.8f, 0.9f, 1f, 1f);

    private GameObject lastSelected;

    void Start()
    {
        StartCoroutine(SelectDefaultButtonRoutine());
    }

    private System.Collections.IEnumerator SelectDefaultButtonRoutine()
    {
        // EventSystem の存在を確認・自動生成
        EnsureEventSystemExists();

        // 画面内のボタンの色を調整してハイライトを明確にする
        AdjustButtonColors();

        // 初期化待ちのため1フレーム待機
        yield return null;

        SelectDefaultButton();
    }

    private void EnsureEventSystemExists()
    {
        if (EventSystem.current == null)
        {
            var existing = FindFirstObjectByType<EventSystem>();
            if (existing == null)
            {
                GameObject go = new GameObject("EventSystem_AutoCreated");
                go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                go.AddComponent<StandaloneInputModule>();
#endif
            }
        }
    }

    private void AdjustButtonColors()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            var colors = button.colors;
            // 選択時・ハイライト時がはっきりわかるように色を設定する
            colors.selectedColor = defaultSelectedColor;
            colors.highlightedColor = defaultHighlightedColor;
            button.colors = colors;
        }
    }

    void Update()
    {
        EnsureEventSystemExists();

        if (EventSystem.current == null) return;

        // 1. マウスクリック等で選択が解除された場合、十字キー・矢印キー入力時に再選択する
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                if (lastSelected != null && lastSelected.activeInHierarchy)
                {
                    EventSystem.current.SetSelectedGameObject(lastSelected);
                }
                else
                {
                    SelectDefaultButton();
                }
            }
        }
        else
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }

        // 2. Enterキー押下時のクリック処理（Submitアクションのフォールバック）
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
            if (selectedObj != null)
            {
                Button button = selectedObj.GetComponent<Button>();
                if (button != null && button.interactable)
                {
                    button.onClick.Invoke();
                }
            }
        }
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current != null)
        {
            Button firstButton = FindFirstObjectByType<Button>();
            if (firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
                lastSelected = firstButton.gameObject;
            }
        }
    }

    // ボタンから呼び出せるように public をつけます
    public void OnClickStartButton()
    {
        // 設定されたゲームシーン名に合わせて遷移します
        SceneManager.LoadScene(gameSceneName);
    }

    // ゲーム終了ボタン用の処理を追加
    public void OnClickQuitButton()
    {
#if UNITY_EDITOR
        // エディタ上でのプレイを終了する
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルドされたゲームを終了する
        Application.Quit();
#endif
    }
}