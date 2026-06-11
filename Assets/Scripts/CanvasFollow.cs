using UnityEngine;

public class CanvasFollow : MonoBehaviour 
{
    [Header("追従する骨（HeadやSkullなどを入れる）")]
    public Transform targetBone; 
    
    [Header("頭のどれくらい上に表示するか")]
    public Vector3 offset = new Vector3(0, 0.5f, 0); // Y軸に0.5ズラす

    void LateUpdate() 
    {
        if(targetBone != null) 
        {
            // 骨の位置 ＋ ズラす分（offset） の位置に移動させる
            transform.position = targetBone.position + offset;
        }
    }
}