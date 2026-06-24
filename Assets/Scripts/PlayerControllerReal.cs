using UnityEngine;

[RequireComponent(typeof(CharacterCore))]
public class PlayerControllerReal : MonoBehaviour
{
    private CharacterCore core;
    private Transform cameraTransform;
    private CameraController cameraController;

    void Start()
    {
        core = GetComponent<CharacterCore>();
        // メインカメラを取得しておく
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            cameraController = cameraTransform.GetComponent<CameraController>();
        }
    }

    void Update()
    {
        if (core == null) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 inputDir = Vector3.zero;

        if (cameraTransform != null)
        {
            // ★カメラが向いている方向を基準にして、前後左右を決める
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            
            // 上下方向（Y軸）の傾きを無視して、水平な地面の移動にする
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            // カメラの向きに合わせて入力を合成する
            inputDir = (camForward * z) + (camRight * x);
        }
        else
        {
            inputDir = new Vector3(x, 0, z);
        }

        if (cameraController != null)
        {
            core.SetLockOnTarget(cameraController.LockOnTarget);
        }
        else
        {
            core.SetLockOnTarget(null);
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        core.SetMovement(inputDir, isRunning);

        bool isHolding = Input.GetMouseButton(0);
        core.SetAttack(isHolding);

        if (Input.GetMouseButtonDown(0))
        {
            core.TriggerAttack();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            core.TriggerJump();
        }
    }
}