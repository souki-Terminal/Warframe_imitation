using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 他のスクリプトから簡単にアクセスするための窓口

    [Header("UI設定")]
    public GameObject titlePanel;    // タイトル画面のパネル
    public GameObject gameOverUI;    // ゲームオーバー画面のパネル
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI enemyCountText; // 敵の数を表示するテキスト

    private float survivalTime = 0f;
    private bool isPlayerDead = false;
    private int currentEnemyCount = 0;

    // Restartボタンでシーンを再読み込みした際、タイトルをスキップするかどうかの記憶
    public static bool skipTitle = false; 

    void Awake()
    {
        instance = this; // 自分自身を登録
    }

    void Start()
    {
        gameOverUI.SetActive(false);
        UpdateEnemyCountText();

        if (skipTitle)
        {
            // Restartから来た場合はタイトルを飛ばして即スタート
            skipTitle = false; 
            StartGame();
        }
        else
        {
            // 最初から来た場合（またはGameOverから来た場合）はタイトルを表示
            titlePanel.SetActive(true);
            Time.timeScale = 0f; // 時間を止める
        }
    }

    void Update()
    {
        if (!isPlayerDead && Time.timeScale > 0)
        {
            survivalTime += Time.deltaTime;
        }
    }

    // タイトル画面の「Start」ボタンから呼ぶ
    public void StartGame()
    {
        titlePanel.SetActive(false);
        Time.timeScale = 1f; // 時間を動かし始める
    }

    public void OnPlayerDied()
    {
        if (isPlayerDead) return;
        isPlayerDead = true;
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
{
    // 修正：Time.timeScaleの影響を受けない「現実世界の5秒」を待つ
    yield return new WaitForSecondsRealtime(5f);

    Time.timeScale = 0f;
    gameOverUI.SetActive(true);
    if (survivalTimeText != null)
    {
        survivalTimeText.text = "Game Over\nSurvival Time: \n" + survivalTime.ToString("F1") + "s";
    }
}
    // ゲームオーバー画面の「Restart」ボタンから呼ぶ（タイトルを飛ばして再挑戦）
    public void RestartGame()
    {
        skipTitle = true; // 次回読み込み時にタイトルをスキップさせる
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ゲームオーバー画面の「GameOver」ボタンから呼ぶ（タイトルへ戻る）
    public void ReturnToTitle()
    {
        skipTitle = false; // 次回読み込み時にタイトルを表示させる
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- 敵の数管理 ---
    private int totalEnemyCount = 0;      // ゲーム上のすべての敵の数（分母）
    private int defeatedEnemyCount = 0;   // 倒した敵の数（分子）

    public void AddEnemyCount()
    {
        totalEnemyCount++; // スポーン時に全体の数を増やす
        UpdateEnemyCountText();
    }

    public void RemoveEnemyCount()
    {
        defeatedEnemyCount++; // 倒したら「倒した数」を増やす
        UpdateEnemyCountText();

        // 倒した数が、全体の数に達したらクリア
        if (defeatedEnemyCount >= totalEnemyCount)
        {
            Debug.Log("すべての敵を倒しました！1秒後にクリア画面へ...");
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
        
        if (survivalTimeText != null)
        {
            survivalTimeText.text = "Game Clear!\nSurvival Time: \n" + survivalTime.ToString("F1") + "s";
        }
    }

    private void UpdateEnemyCountText()
    {
        if (enemyCountText != null)
        {
            // 「0 / 1」のような分数表記に変更
            enemyCountText.text = "Enemies: " + defeatedEnemyCount + " / " + totalEnemyCount;
        }
    }
}