using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移の処理に必須です
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    private GameObject lastSelected;

    void Start()
    {
        // シーン開始時にデフォルトのボタンを選択状態にする
        SelectDefaultButton();
    }

    void Update()
    {
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
        // "GameScene" の部分は、ご自身の実際のゲームシーン名に合わせて変更してください
        SceneManager.LoadScene("GameScene");
    }
}