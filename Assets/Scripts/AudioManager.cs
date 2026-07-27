using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 1.0f;
    [Range(0f, 1f)] public float seVolume = 1.0f;
    [Range(0f, 1f)] public float voiceVolume = 1.0f;

    [Header("BGM")]
    public AudioClip bgmTitle;
    public AudioClip bgmGame;
    public AudioClip bgmClear;
    
    [Header("SE")]
    public AudioClip sePlayerSwing;
    public AudioClip sePlayerHit;
    public AudioClip seEnemyHit;
    public AudioClip seEnemyDefeat;
    public AudioClip seWaveClear;
    public AudioClip seTypingSuccess;
    public AudioClip seTypingMiss;
    public AudioClip seTypingTimeUp;
    public AudioClip sePowerUpRightGet;
    public AudioClip sePowerUpGet;
    public AudioClip seGameClear;
    public AudioClip seButtonClick;

    [Header("Voices")]
    public AudioClip[] voiceDamage;
    public AudioClip[] voiceJump;
    public AudioClip[] voiceAttack;

    private AudioSource bgmSource;
    private AudioSource seSource;
    private AudioSource voiceSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("AudioManager");
            if (prefab != null)
            {
                Instantiate(prefab);
            }
            else
            {
                Debug.LogWarning("AudioManager Prefab not found in Resources. Please ensure it is generated.");
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            // Setup AudioSources
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.loop = false;
            seSource.playOnAwake = false;

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private AudioListener fallbackListener;

    private void Update()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (seSource != null) seSource.volume = seVolume;
        if (voiceSource != null) voiceSource.volume = voiceVolume;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        // 他のリスナーが1つ以上あるのに、さらにAudioManagerにも付いている場合は削除する
        if (listeners.Length > 1 && fallbackListener != null)
        {
            Destroy(fallbackListener);
            fallbackListener = null;
        }
        // シーンに1つもリスナーが存在しない場合は、AudioManagerに追加する
        else if (listeners.Length == 0 && fallbackListener == null)
        {
            fallbackListener = gameObject.AddComponent<AudioListener>();
        }
    }

    // --- BGM Methods ---
    public void PlayTitleBGM()
    {
        PlayBGM(bgmTitle);
    }

    public void PlayGameBGM()
    {
        PlayBGM(bgmGame);
    }

    public void PlayClearBGM()
    {
        PlayBGM(bgmClear);
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    public void FadeOutBGM(float duration, System.Action onFadeComplete = null)
    {
        StartCoroutine(FadeOutRoutine(duration, onFadeComplete));
    }

    private IEnumerator FadeOutRoutine(float duration, System.Action onFadeComplete)
    {
        float startVolume = bgmSource.volume;
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVolume; // 次の再生のために音量を元に戻す
        onFadeComplete?.Invoke();
    }

    // --- SE Methods ---
    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip);
    }

    public void PlayPlayerSwing() => PlaySE(sePlayerSwing);
    public void PlayPlayerHit() => PlaySE(sePlayerHit); // Player attacking enemy hit sound
    public void PlayEnemyHit() => PlaySE(seEnemyHit);   // Enemy attacking player hit sound
    public void PlayEnemyDefeat() => PlaySE(seEnemyDefeat);
    public void PlayWaveClear() => StartCoroutine(SEWithBGMStopRoutine(seWaveClear, true));
    public void PlayTypingSuccess() => PlaySE(seTypingSuccess);
    public void PlayTypingMiss() => PlaySE(seTypingMiss);
    public void PlayTypingTimeUp() => PlaySE(seTypingTimeUp);
    public void PlayPowerUpRightGet() => PlaySE(sePowerUpRightGet);
    public void PlayPowerUpGet() => PlaySE(sePowerUpGet);
    public void PlayGameClear() => StartCoroutine(GameClearRoutine());
    public void PlayButtonClick() => PlaySE(seButtonClick);

    private IEnumerator SEWithBGMStopRoutine(AudioClip clip, bool resumeBGM)
    {
        if (clip == null) yield break;
        PauseBGM();
        seSource.PlayOneShot(clip);
        yield return new WaitForSecondsRealtime(clip.length);
        if (resumeBGM) ResumeBGM();
    }

    private IEnumerator GameClearRoutine()
    {
        StopBGM();
        if (seGameClear != null)
        {
            seSource.PlayOneShot(seGameClear);
            yield return new WaitForSecondsRealtime(seGameClear.length);
        }
        PlayClearBGM();
    }

    // --- Voice Methods ---
    private float lastDamageVoiceTime = -1f;
    private float lastAttackVoiceTime = -1f;
    private float lastJumpVoiceTime = -1f;
    private bool hasPlayedJumpVoiceInAir = false;

    public void PlayDamageVoice()
    {
        if (Time.time - lastDamageVoiceTime < 1.0f) return;
        lastDamageVoiceTime = Time.time;
        PlayRandomVoice(voiceDamage);
    }

    public void PlayJumpVoice()
    {
        // 連続再生防止（0.5秒）と空中での複数回再生防止
        if (Time.time - lastJumpVoiceTime < 0.5f) return;
        if (hasPlayedJumpVoiceInAir) return;
        
        lastJumpVoiceTime = Time.time;
        hasPlayedJumpVoiceInAir = true;
        PlayRandomVoice(voiceJump);
    }

    public void ResetJumpVoice()
    {
        hasPlayedJumpVoiceInAir = false;
    }

    public void PlayAttackVoice()
    {
        if (Time.time - lastAttackVoiceTime < 1.0f) return;
        lastAttackVoiceTime = Time.time;
        PlayRandomVoice(voiceAttack);
    }

    private void PlayRandomVoice(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        int index = Random.Range(0, clips.Length);
        if (clips[index] != null)
        {
            voiceSource.PlayOneShot(clips[index]);
        }
    }
}
