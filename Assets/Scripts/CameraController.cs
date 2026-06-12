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
    
    // ★追加：BotW風の「肩越し視点」にするための横ずらし
    [Tooltip("カメラをプレイヤーの真後ろから右にどれくらいずらすか")]
    public float lockOnSideOffset = 1.2f; 
    
    public float lockOnTrackingSpeed = 20.0f;

    [Header("動的ズーム設定")]
    // ★変更：前回の 1.5f だと近すぎるため、最小距離を広げました
    [Tooltip("カメラがプレイヤーに近づきすぎないための最小距離")]
    public float minCameraDistance = 2.5f;

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
            FindNearestEnemy();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            lockOnTarget = null;
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
        Vector3 playerLookPos = player.transform.position + targetOffset;

        if (lockOnTarget != null)
        {
            if (!lockOnTarget.gameObject.activeInHierarchy)
            {
                lockOnTarget = null;
                return;
            }

            Vector3 enemyTargetPos = lockOnTarget.position + new Vector3(0, lockOnHeightOffset, 0);

            // プレイヤーから敵への方向ベクトル（前方向）を計算
            Vector3 forwardDir = new Vector3(enemyTargetPos.x - playerLookPos.x, 0, enemyTargetPos.z - playerLookPos.z);
            float distToEnemy = forwardDir.magnitude;

            if (distToEnemy < 0.01f)
            {
                forwardDir = player.transform.forward;
            }
            else
            {
                forwardDir.Normalize();
            }

            // カメラを置く基準となる「後ろ方向」と「右方向」を計算
            Vector3 backDir = -forwardDir;
            // 外積（Cross）を使って、前方向に対して直角な右方向のベクトルを作る
            Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

            // ★変更箇所：ズームを少しマイルドにし、極端に背中に張り付かないようにする
            float currentCameraDist = distance;
            if (distToEnemy < distance)
            {
                // 近づく割合を半分にして、一定以上の距離（minCameraDistance）は保つ
                currentCameraDist = Mathf.Max(minCameraDistance, (distToEnemy + distance) * 0.5f);
            }

            // ★変更箇所：プレイヤーの背後（backDir）から、さらに右側（rightDir）にずらしてカメラを配置
            Vector3 desiredPosition = playerLookPos + (backDir * currentCameraDist) + (rightDir * lockOnSideOffset);
            desiredPosition.y = playerLookPos.y + lockOnCameraHeightOffset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * lockOnTrackingSpeed);

            // カメラの視点は今まで通り「プレイヤーと敵のちょうど真ん中」を見つめる
            Vector3 centerPoint = Vector3.Lerp(playerLookPos, enemyTargetPos, 0.5f);
            Quaternion lookRotation = Quaternion.LookRotation(centerPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lockOnTrackingSpeed);

            currentX = transform.rotation.eulerAngles.y;
            currentY = transform.rotation.eulerAngles.x;
            if (currentY > 180) currentY -= 360;
        }
        else
        {
            Vector3 dir = new Vector3(0, 0, -distance);
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            
            transform.position = Vector3.Lerp(transform.position, playerLookPos + rotation * dir, Time.deltaTime * 15f);
            transform.LookAt(playerLookPos);
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