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
    public float lockOnHeightOffset = 1.0f; 
    public float lockOnCameraHeightOffset = 0.5f; 
    
    [Tooltip("カメラをプレイヤーの真後ろから右にどれくらいずらすか")]
    public float lockOnSideOffset = 1.2f; 
    
    [Tooltip("プレイヤーが左右に動いたとき、どれくらい画面端に寄せるか")]
    public float sideTrackingIntensity = 0.5f; 
    
    public float lockOnTrackingSpeed = 20.0f;

    [Header("動的ズーム設定")]
    [Tooltip("カメラがプレイヤーに近づきすぎないための最小距離")]
    public float minCameraDistance = 2.5f;

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private Transform lockOnTarget;     
    public Transform LockOnTarget => lockOnTarget;
    private Vector3 smoothedPlayerPos;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 初期位置の同期
        if (player != null)
        {
            smoothedPlayerPos = player.transform.position + targetOffset;
        }
    }

    void Update()
    {
        // マウス操作の切り替え
        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
        if (Input.GetMouseButtonDown(0) && lockOnTarget == null) Cursor.lockState = CursorLockMode.Locked;

        // ロックオンのオンオフ
        if (Input.GetMouseButtonDown(1))
        {
            FindNearestEnemy();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            lockOnTarget = null;
        }

        // 非ロックオン時の手動カメラ回転計算
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
        
        // プレイヤー位置の補間（カメラのガタつき防止）
        smoothedPlayerPos = Vector3.Lerp(smoothedPlayerPos, player.transform.position + targetOffset, Time.deltaTime * 15f);
        Vector3 playerLookPos = smoothedPlayerPos;

        if (lockOnTarget != null)
        {
            // 敵が消滅した際の処理
            if (!lockOnTarget.gameObject.activeInHierarchy)
            {
                lockOnTarget = null;
                return;
            }

            Vector3 enemyTargetPos = lockOnTarget.position + new Vector3(0, lockOnHeightOffset, 0);

            // 1. プレイヤーの左右移動に基づくオフセット計算
            // プレイヤーが「右」にいれば画面右へ、「左」にいれば画面左へ
            float horizontalOffset = Vector3.Dot(player.transform.right, (player.transform.position - transform.position).normalized) * sideTrackingIntensity;

            // 2. 注視点の計算（敵とプレイヤーの中間点 ＋ 左右オフセット）
            Vector3 targetLookAt = Vector3.Lerp(playerLookPos, enemyTargetPos, 0.5f);
            targetLookAt += player.transform.right * horizontalOffset;

            // 3. カメラの回転（ターゲットを注視）
            Quaternion lookRotation = Quaternion.LookRotation(targetLookAt - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lockOnTrackingSpeed);

            // 4. カメラ位置（敵の周りを維持）
            // ※敵との高低差によるカメラの上下動や急な見下ろしを防ぐため、水平方向のベクトルとして計算します。
            Vector3 dirToEnemy = (enemyTargetPos - playerLookPos);
            dirToEnemy.y = 0;
            dirToEnemy.Normalize();

            Vector3 cameraPosition = playerLookPos - (dirToEnemy * distance) + Vector3.up * lockOnCameraHeightOffset;
            transform.position = Vector3.Lerp(transform.position, cameraPosition, Time.deltaTime * lockOnTrackingSpeed);
        }
        else
        {
            // 非ロックオン時の追従処理
            Vector3 dir = new Vector3(0, 0, -distance);
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            
            transform.position = Vector3.Lerp(transform.position, playerLookPos + rotation * dir, Time.deltaTime * 15f);
            transform.LookAt(playerLookPos);
        }
    }

    void FindNearestEnemy()
    {
        // 現在シーン内の敵を取得
        EnemyStatus[] enemies = FindObjectsByType<EnemyStatus>(FindObjectsSortMode.None);
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