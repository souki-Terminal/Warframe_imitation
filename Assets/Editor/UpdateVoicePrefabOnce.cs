#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class UpdateVoicePrefabOnce
{
    [InitializeOnLoadMethod]
    public static void UpdatePrefab()
    {
        if (SessionState.GetBool("VoicePrefabUpdated", false)) return;
        SessionState.SetBool("VoicePrefabUpdated", true);

        string prefabPath = "Assets/Resources/AudioManager.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            AudioManager manager = prefab.GetComponent<AudioManager>();
            if (manager != null)
            {
                manager.voiceJump = new AudioClip[] {
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ0001.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ0002.wav")
                };
                
                manager.voiceDamage = new AudioClip[] {
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1091.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1092.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1093.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1094.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1095.wav")
                };

                manager.voiceAttack = new AudioClip[] {
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1101.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/UnityChan/Voice/univ1102.wav")
                };

                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log("AudioManager Prefab voices updated via script!");
            }
        }
    }
}
#endif
