#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class AudioSetupAuto
{
    [InitializeOnLoadMethod]
    public static void SetupAudioManagerPrefab()
    {
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        string prefabPath = "Assets/Resources/AudioManager.prefab";
        
        // すでにPrefabが存在している場合は上書き処理をスキップ（手動変更を維持するため）
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            return;
        }

        GameObject obj = new GameObject("AudioManager");
        AudioManager manager = obj.AddComponent<AudioManager>();

        manager.bgmTitle = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/GB-Action-C05/GB-Action-C05-2(Stage4-Loop130).mp3");
        manager.bgmGame = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/GB-Action-D08/GB-Action-D08-2(Boss2-Loop175).mp3");
        manager.bgmClear = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/GB-Fighting-B15/GB-Fighting-B15-2(ED2-Loop175).mp3");

        manager.sePlayerSwing = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/剣の素振り3.mp3");
        manager.sePlayerHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/剣で斬る2.mp3");
        manager.seEnemyHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/ナイフで切る.mp3");
        manager.seEnemyDefeat = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/ゴブリンの鳴き声3.mp3");
        manager.seWaveClear = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/SNES-Action01/SNES-Action01-18(Message).mp3");
        manager.seTypingSuccess = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/クイズ正解3.mp3");
        manager.seTypingMiss = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/クイズ不正解2.mp3");
        manager.seTypingTimeUp = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/SNES-Action01/SNES-Action01-11(Damage).mp3");
        manager.sePowerUpRightGet = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/SNES-Action01/SNES-Action01-16(Item).mp3");
        manager.sePowerUpGet = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/SNES-Action01/SNES-Action01-15(Item).mp3");
        manager.seGameClear = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/SNES-Action01/SNES-Action01-19(Message).mp3");
        manager.seButtonClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SE/Sys_Set02/Sys_Set02-sentaku.mp3");

        manager.voiceDamage = new AudioClip[]
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1091.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1092.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1093.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1094.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1095.wav")
        };

        manager.voiceJump = new AudioClip[]
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ0001.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ0002.wav")
        };

        manager.voiceAttack = new AudioClip[]
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1101.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1102.wav")
        };

        PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        GameObject.DestroyImmediate(obj);
        Debug.Log("AudioManager Prefab has been successfully auto-generated in Assets/Resources/ !");
    }
}
#endif
