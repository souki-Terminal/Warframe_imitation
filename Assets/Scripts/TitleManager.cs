using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    void Start()
    {
        // タイトル画面が開かれた瞬間に、必ずカーソルを表示して動かせるようにする
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GameStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }
}