using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TypingManager : MonoBehaviour
{
    [Header("UI参照")]
    public TextMeshProUGUI wordText;       // 出題されるひらがな
    public TextMeshProUGUI romajiText;     // ローマ字全体
    public TextMeshProUGUI timerText;      // 残り時間
    public TextMeshProUGUI progressText;   // 現在の問題数 (例: 1/10)
    public TextMeshProUGUI scoreText;      // 現在の正解数
    public Image timerFill;                // 時間制限バー

    [Header("タイピング設定")]
    public WordCategory currentCategory = WordCategory.Fish;

    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private const int MaxQuestions = 5;
    private const float TimeLimit = 5.0f;
    private float currentTime = 0f;
    private bool isPlaying = false;

    private string currentHiragana;
    private string targetRomajiFull; 
    private int currentKanaIndex = 0;
    private string currentTypedRomaji = "";

    private List<string> wordList = new List<string>();

    private static List<WordCategory> categoryQueue = new List<WordCategory>();
    private static WordCategory lastPlayedCategory = (WordCategory)(-1); // 未プレイ状態

    private void Awake()
    {
        // 以前はここで単語リストを生成していたが、StartChallenge時に生成するように変更
    }

    private void InitializeQueue()
    {
        List<WordCategory> allCategories = new List<WordCategory> 
        { 
            WordCategory.Fish, WordCategory.Vegetable, WordCategory.Country, WordCategory.Vehicle 
        };

        // シャッフル
        for (int i = 0; i < allCategories.Count; i++) 
        {
            int r = Random.Range(i, allCategories.Count);
            var temp = allCategories[i];
            allCategories[i] = allCategories[r];
            allCategories[r] = temp;
        }

        // 可能であれば、前回の最後のカテゴリと今回の最初のカテゴリが被らないように調整
        if (allCategories.Count > 1 && allCategories[0] == lastPlayedCategory)
        {
            var temp = allCategories[0];
            allCategories[0] = allCategories[1];
            allCategories[1] = temp;
        }

        categoryQueue = allCategories;
    }

    private int typingChallengeCount = 0;

    private void GenerateWordList()
    {
        if (categoryQueue == null || categoryQueue.Count == 0)
        {
            InitializeQueue();
        }
        
        currentCategory = categoryQueue[0];
        categoryQueue.RemoveAt(0);
        lastPlayedCategory = currentCategory;
        
        // 強化回数 = タイピングチャレンジに突入した回数
        int totalEnhancements = typingChallengeCount;
        
        wordList = TypingWordProvider.GetWordList(currentCategory, totalEnhancements);

        // 次回のためにカウントを増やす
        typingChallengeCount++;
    }

    private Dictionary<string, string[]> kanaToRomaji = new Dictionary<string, string[]>()
    {
        {"あ", new[]{"a"}}, {"い", new[]{"i"}}, {"う", new[]{"u", "wu"}}, {"え", new[]{"e"}}, {"お", new[]{"o"}},
        {"か", new[]{"ka", "ca"}}, {"き", new[]{"ki"}}, {"く", new[]{"ku", "cu", "qu"}}, {"け", new[]{"ke"}}, {"こ", new[]{"ko", "co"}},
        {"さ", new[]{"sa"}}, {"し", new[]{"shi", "si", "ci"}}, {"す", new[]{"su"}}, {"せ", new[]{"se", "ce"}}, {"そ", new[]{"so"}},
        {"た", new[]{"ta"}}, {"ち", new[]{"chi", "ti"}}, {"つ", new[]{"tsu", "tu"}}, {"て", new[]{"te"}}, {"と", new[]{"to"}},
        {"な", new[]{"na"}}, {"に", new[]{"ni"}}, {"ぬ", new[]{"nu"}}, {"ね", new[]{"ne"}}, {"の", new[]{"no"}},
        {"は", new[]{"ha"}}, {"ひ", new[]{"hi"}}, {"ふ", new[]{"fu", "hu"}}, {"へ", new[]{"he"}}, {"ほ", new[]{"ho"}},
        {"ま", new[]{"ma"}}, {"み", new[]{"mi"}}, {"む", new[]{"mu"}}, {"め", new[]{"me"}}, {"も", new[]{"mo"}},
        {"や", new[]{"ya"}}, {"ゆ", new[]{"yu"}}, {"よ", new[]{"yo"}},
        {"ら", new[]{"ra"}}, {"り", new[]{"ri"}}, {"る", new[]{"ru"}}, {"れ", new[]{"re"}}, {"ろ", new[]{"ro"}},
        {"わ", new[]{"wa"}}, {"を", new[]{"wo"}},
        {"が", new[]{"ga"}}, {"ぎ", new[]{"gi"}}, {"ぐ", new[]{"gu"}}, {"げ", new[]{"ge"}}, {"ご", new[]{"go"}},
        {"ざ", new[]{"za"}}, {"じ", new[]{"ji", "zi"}}, {"ず", new[]{"zu"}}, {"ぜ", new[]{"ze"}}, {"ぞ", new[]{"zo"}},
        {"だ", new[]{"da"}}, {"ぢ", new[]{"di"}}, {"づ", new[]{"du"}}, {"で", new[]{"de"}}, {"ど", new[]{"do"}},
        {"ば", new[]{"ba"}}, {"び", new[]{"bi"}}, {"ぶ", new[]{"bu"}}, {"べ", new[]{"be"}}, {"ぼ", new[]{"bo"}},
        {"ぱ", new[]{"pa"}}, {"ぴ", new[]{"pi"}}, {"ぷ", new[]{"pu"}}, {"ぺ", new[]{"pe"}}, {"ぽ", new[]{"po"}},
        {"ん", new[]{"nn", "n"}},
        {"ぁ", new[]{"xa", "la"}}, {"ぃ", new[]{"xi", "li"}}, {"ぅ", new[]{"xu", "lu"}}, {"ぇ", new[]{"xe", "le"}}, {"ぉ", new[]{"xo", "lo"}},
        {"ゃ", new[]{"xya", "lya", "ya"}}, {"ゅ", new[]{"xyu", "lyu", "yu"}}, {"ょ", new[]{"xyo", "lyo", "yo"}},
        {"っ", new[]{"xtu", "ltu", "xtsu", "ltsu"}}, {"ー", new[]{"-"}},
        // 拗音（2文字）
        {"きゃ", new[]{"kya"}}, {"きゅ", new[]{"kyu"}}, {"きょ", new[]{"kyo"}},
        {"しゃ", new[]{"sha", "sya"}}, {"しゅ", new[]{"shu", "syu"}}, {"しょ", new[]{"sho", "syo"}},
        {"ちゃ", new[]{"cha", "tya", "cya"}}, {"ちゅ", new[]{"chu", "tyu", "cyu"}}, {"ちょ", new[]{"cho", "tyo", "cyo"}},
        {"にゃ", new[]{"nya"}}, {"にゅ", new[]{"nyu"}}, {"にょ", new[]{"nyo"}},
        {"ひゃ", new[]{"hya"}}, {"ひゅ", new[]{"hyu"}}, {"ひょ", new[]{"hyo"}},
        {"みゃ", new[]{"mya"}}, {"みゅ", new[]{"myu"}}, {"みょ", new[]{"myo"}},
        {"りゃ", new[]{"rya"}}, {"りゅ", new[]{"ryu"}}, {"りょ", new[]{"ryo"}},
        {"ぎゃ", new[]{"gya"}}, {"ぎゅ", new[]{"gyu"}}, {"ぎょ", new[]{"gyo"}},
        {"じゃ", new[]{"ja", "zya", "jya"}}, {"じゅ", new[]{"ju", "zyu", "jyu"}}, {"じょ", new[]{"jo", "zyo", "jyo"}},
        {"ぢゃ", new[]{"dya"}}, {"ぢゅ", new[]{"dyu"}}, {"ぢょ", new[]{"dyo"}},
        {"びゃ", new[]{"bya"}}, {"びゅ", new[]{"byu"}}, {"びょ", new[]{"byo"}},
        {"ぴゃ", new[]{"pya"}}, {"ぴゅ", new[]{"pyu"}}, {"ぴょ", new[]{"pyo"}},
        // 促音（っ＋文字）
        {"っか", new[]{"kka"}}, {"っき", new[]{"kki"}}, {"っく", new[]{"kku"}}, {"っけ", new[]{"kke"}}, {"っこ", new[]{"kko"}},
        {"っさ", new[]{"ssa"}}, {"っし", new[]{"sshi", "ssi"}}, {"っす", new[]{"ssu"}}, {"っせ", new[]{"sse"}}, {"っそ", new[]{"sso"}},
        {"った", new[]{"tta"}}, {"っち", new[]{"cchi", "tti"}}, {"っつ", new[]{"ttsu", "ttu"}}, {"って", new[]{"tte"}}, {"っと", new[]{"tto"}},
        {"っぱ", new[]{"ppa"}}, {"っぴ", new[]{"ppi"}}, {"っぷ", new[]{"ppu"}}, {"っぺ", new[]{"ppe"}}, {"っぽ", new[]{"ppo"}}
    };

    public void StartChallenge()
    {
        // 毎回お題カテゴリと難易度（強化回数）を再計算して単語リストを取得する
        GenerateWordList();

        // 背景暗幕の追加（初回のみ）
        if (GameManager.instance != null && GameManager.instance.typingGamePanel != null)
        {
            Image bg = GameManager.instance.typingGamePanel.GetComponent<Image>();
            if (bg == null)
            {
                bg = GameManager.instance.typingGamePanel.AddComponent<Image>();
                bg.color = new Color(0, 0, 0, 0.85f);
            }
        }

        currentQuestionIndex = 0;
        correctAnswers = 0;
        isPlaying = true;
        NextQuestion();
    }

    private void NextQuestion()
    {
        if (currentQuestionIndex >= MaxQuestions)
        {
            EndChallenge();
            return;
        }

        currentQuestionIndex++;
        currentTime = TimeLimit;
        
        string word = wordList[Random.Range(0, wordList.Count)];
        currentHiragana = word;
        currentKanaIndex = 0;
        currentTypedRomaji = "";

        UpdateUI();
    }

    void Update()
    {
        if (!isPlaying) return;

        currentTime -= Time.unscaledDeltaTime; 
        if (currentTime <= 0)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayTypingTimeUp();
            NextQuestion();
            return;
        }

        if (timerText != null) timerText.text = currentTime.ToString("F1") + " s";
        if (timerFill != null) timerFill.fillAmount = currentTime / TimeLimit;

        if (Input.anyKeyDown)
        {
            string inputStr = Input.inputString.ToLower();
            foreach (char c in inputStr)
            {
                if ((c >= 'a' && c <= 'z') || c == '-')
                {
                    ProcessInput(c.ToString());
                }
            }
        }
    }

    private void ProcessInput(string inputChar)
    {
        if (currentKanaIndex >= currentHiragana.Length) return;

        int matchLength = 1;
        string currentKana = currentHiragana[currentKanaIndex].ToString();

        // 2文字の組み合わせ（拗音・促音など）を優先してチェック
        if (currentKanaIndex + 1 < currentHiragana.Length)
        {
            string twoChar = currentHiragana.Substring(currentKanaIndex, 2);
            if (kanaToRomaji.ContainsKey(twoChar))
            {
                currentKana = twoChar;
                matchLength = 2;
            }
        }

        string[] validPatterns = kanaToRomaji[currentKana];

        string testStr = currentTypedRomaji + inputChar;

        bool isValidPrefix = false;
        bool isFullMatch = false;

        foreach (string pattern in validPatterns)
        {
            if (pattern == testStr)
            {
                isFullMatch = true;
                break;
            }
            if (pattern.StartsWith(testStr))
            {
                isValidPrefix = true;
            }
        }

        if (isFullMatch)
        {
            currentTypedRomaji = "";
            currentKanaIndex += matchLength;
            if (currentKanaIndex >= currentHiragana.Length)
            {
                WordCleared();
            }
            else
            {
                UpdateUI();
            }
        }
        else if (isValidPrefix)
        {
            currentTypedRomaji = testStr;
            UpdateUI();
        }
    }

    private void WordCleared()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTypingSuccess();
        correctAnswers++;
        NextQuestion();
    }

    private void UpdateUI()
    {
        if (progressText != null) progressText.text = $"問題: {currentQuestionIndex} / {MaxQuestions}";
        if (scoreText != null) scoreText.text = $"正解: {correctAnswers}";

        string typedKana = currentHiragana.Substring(0, currentKanaIndex);
        string untypedKana = currentHiragana.Substring(currentKanaIndex);
        if (wordText != null) wordText.text = $"<color=#888888>{typedKana}</color>{untypedKana}";

        string displayRomaji = "";
        for (int i = 0; i < currentHiragana.Length; )
        {
            string k = currentHiragana[i].ToString();
            int step = 1;

            if (i + 1 < currentHiragana.Length)
            {
                string twoChar = currentHiragana.Substring(i, 2);
                if (kanaToRomaji.ContainsKey(twoChar))
                {
                    k = twoChar;
                    step = 2;
                }
            }

            string defaultRomaji = kanaToRomaji[k][0];
            
            if (i < currentKanaIndex)
            {
                displayRomaji += $"<color=#888888>{defaultRomaji}</color>";
            }
            else if (i == currentKanaIndex)
            {
                string activePattern = defaultRomaji;
                foreach(var pat in kanaToRomaji[k]) {
                    if (pat.StartsWith(currentTypedRomaji)) { activePattern = pat; break; }
                }
                
                string rem = activePattern.Length >= currentTypedRomaji.Length ? activePattern.Substring(currentTypedRomaji.Length) : "";
                displayRomaji += $"<color=#888888>{currentTypedRomaji}</color><color=#FF5555>{rem}</color>";
            }
            else
            {
                displayRomaji += defaultRomaji;
            }
            
            i += step;
        }
        
        if (romajiText != null) romajiText.text = displayRomaji;
    }

    private void EndChallenge()
    {
        isPlaying = false;
        if (GameManager.instance != null && GameManager.instance.typingGamePanel != null)
        {
            GameManager.instance.typingGamePanel.SetActive(false);
        }

        int powerUpOptions = correctAnswers; 
        if (powerUpOptions > 6) powerUpOptions = 6;

        PowerUpManager pm = GetComponent<PowerUpManager>();
        if (pm != null)
        {
            bool isPerfect = (correctAnswers == MaxQuestions);
            pm.ShowChoices(powerUpOptions, true, isPerfect);
        }
    }
}
