using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("追従するターゲット")]
    public GameObject player; // 追従対象（Player）を入れる枠
    
    private Vector3 offset;   // プレイヤーとカメラの距離（位置関係）

    void Start()
    {
        // もしInspectorでplayerを入れ忘れていた場合のエラーを防ぐ
        if (player != null)
        {
            // ゲーム開始時の「カメラとプレイヤーの距離」を記憶する
            offset = transform.position - player.transform.position;
        }
    }

    // カメラの追従は LateUpdate を使うのが鉄則
    void LateUpdate()
    {
        // ターゲットが設定されていない、または死亡して消滅(Destroy)した場合は処理を止める
        if (player == null) return; 

        // カメラの位置を「プレイヤーの現在の位置 ＋ 最初からあった距離」に毎フレーム更新する
        transform.position = player.transform.position + offset;
    }
}