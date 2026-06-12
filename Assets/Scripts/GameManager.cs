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

    void Update()
    {
        if (!isPlayerDead && Time.timeScale > 0)
        {
            survivalTime += Time.deltaTime;
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