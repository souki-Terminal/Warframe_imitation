using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("追従するターゲット")]
    public GameObject player;

    [Header("カメラ設定")]
    public float distance = 4.0f;       
    public float sensitivity = 3.0f;    
    public Vector3 targetOffset = new Vector3(0, 1.2f, 0); 

    [Header("Z注目（ロックオン）設定")]
    public float lockOnRange = 15.0f;   
    // ★追加：注目する敵の「高さ（Y軸）」をUnity側で調整できるようにする
    public float lockOnHeightOffset = 1.0f; 

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private Transform lockOnTarget;     

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
        if (Input.GetMouseButtonDown(0) && lockOnTarget == null) Cursor.lockState = CursorLockMode.Locked;

        if (Input.GetMouseButtonDown(1))
        {
            if (lockOnTarget == null) FindNearestEnemy();
            else lockOnTarget = null; 
        }

        if (lockOnTarget == null)
        {
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            currentY = Mathf.Clamp(currentY, -30f, 60f); 
        }
    }

    void LateUpdate()
    {
        if (player == null) return;
        Vector3 lookPosition = player.transform.position + targetOffset;

        if (lockOnTarget != null)
        {
            if (!lockOnTarget.gameObject.activeInHierarchy)
            {
                lockOnTarget = null;
                return;
            }

            // ★修正：敵の足元ではなく、設定した高さ（lockOnHeightOffset）を足した位置を目標にする
            Vector3 targetPos = lockOnTarget.position + new Vector3(0, lockOnHeightOffset, 0);

            Vector3 dirToEnemy = (targetPos - player.transform.position).normalized;
            Vector3 desiredPosition = lookPosition - (dirToEnemy * distance);
            
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10f);
            Quaternion lookRotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

            currentX = transform.rotation.eulerAngles.y;
            currentY = transform.rotation.eulerAngles.x;
            if (currentY > 180) currentY -= 360;
        }
        else
        {
            Vector3 dir = new Vector3(0, 0, -distance);
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            
            transform.position = lookPosition + rotation * dir;
            transform.LookAt(lookPosition);
        }
    }

    void FindNearestEnemy()
    {
        EnemyStatus[] enemies = FindObjectsOfType<EnemyStatus>();
        float nearestDist = lockOnRange;
        Transform nearestObj = null;

        foreach (EnemyStatus enemy in enemies)
        {
            if (!enemy.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(player.transform.position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestObj = enemy.transform;
            }
        }
        lockOnTarget = nearestObj;
    }
}