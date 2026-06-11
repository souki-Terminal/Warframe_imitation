using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // 常にメインカメラの方を向く（UIが裏返らないようにする）
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}