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

    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private const int MaxQuestions = 10;
    private const float TimeLimit = 10.0f;
    private float currentTime = 0f;
    private bool isPlaying = false;

    private string currentHiragana;
    private string targetRomajiFull; 
    private int currentKanaIndex = 0;
    private string currentTypedRomaji = "";

    private List<string> wordList = new List<string>();

    private void Awake()
    {
        GenerateWordList();
    }

    private void GenerateWordList()
    {
        string[] prefixes = new string[] { 
            "あか", "あお", "しろ", "くろ", "おお", "こ", "やま", "かわ", "うみ", "そら", 
            "はる", "なつ", "あき", "ふゆ", "あさ", "ひる", "よる", "ほん", "いぬ", "ねこ", 
            "とり", "さかな", "にく", "やさい", "くるま", "ふね", "まち", "むら", "くに", "ひと", 
            "いえ", "へや", "まど", "つくえ", "いす", "かばん", "ぼうし", "ふく", "くつ", "とけい",
            "あめ", "ゆき", "かぜ", "くも", "ほし", "つき", "たいよう", "はな", "き", "くさ"
        };
        string[] suffixes = new string[] { 
            "やま", "かわ", "うみ", "いぬ", "ねこ", "とり", "さかな", "にく", "みず", "おちゃ", 
            "ごはん", "ぱん", "りんご", "みかん", "ばなな", "ぶどう", "いちご", "すいか", "めろん", "もも", 
            "なし", "かき", "さくら", "うめ", "きく", "ゆり", "ばら", "ひまわり", "あさがお", "こすもす", 
            "たんぽぽ", "すみれ", "あじさい", "つばき", "ぼたん", "もみじ", "いちょう", "まつ", "たけ", "かぜ",
            "いし", "すな", "つち", "みち", "はし", "いえ", "むら", "まち", "えき", "みせ"
        };

        wordList.Clear();
        foreach (string p in prefixes)
        {
            foreach (string s in suffixes)
            {
                wordList.Add(p + s);
            }
        }
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
        {"っ", new[]{"xtu", "ltu", "xtsu", "ltsu"}}, {"ー", new[]{"-"}}
    };

    public void StartChallenge()
    {
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
                if (c >= 'a' && c <= 'z')
                {
                    ProcessInput(c.ToString());
                }
            }
        }
    }

    private void ProcessInput(string inputChar)
    {
        if (currentKanaIndex >= currentHiragana.Length) return;

        string currentKana = currentHiragana[currentKanaIndex].ToString();
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
            currentKanaIndex++;
            if (currentKanaIndex >= currentHiragana.Length)
            {
                correctAnswers++;
                NextQuestion();
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

    private void UpdateUI()
    {
        if (progressText != null) progressText.text = $"問題: {currentQuestionIndex} / {MaxQuestions}";
        if (scoreText != null) scoreText.text = $"正解: {correctAnswers}";

        string typedKana = currentHiragana.Substring(0, currentKanaIndex);
        string untypedKana = currentHiragana.Substring(currentKanaIndex);
        if (wordText != null) wordText.text = $"<color=#888888>{typedKana}</color>{untypedKana}";

        string displayRomaji = "";
        for (int i = 0; i < currentHiragana.Length; i++)
        {
            string k = currentHiragana[i].ToString();
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

        int powerUpOptions = correctAnswers / 2; 
        if (powerUpOptions > 5) powerUpOptions = 5;

        PowerUpManager pm = GetComponent<PowerUpManager>();
        if (pm != null)
        {
            bool isPerfect = (correctAnswers == MaxQuestions);
            pm.ShowChoices(powerUpOptions, true, isPerfect);
        }
    }
}
