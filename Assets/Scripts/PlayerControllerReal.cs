using UnityEngine;

[RequireComponent(typeof(CharacterCore))]
public class PlayerControllerReal : MonoBehaviour
{
    private CharacterCore core;

    void Start()
    {
        core = GetComponent<CharacterCore>();
    }

    void Update()
    {
        // 移動入力
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0, z);
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        core.SetMovement(inputDir, isRunning);

        // 攻撃入力 (BoolとTriggerの両方を送る)
        bool isHolding = Input.GetMouseButton(0);
        core.SetAttack(isHolding); // 押しっぱなし情報を送信

        if (Input.GetMouseButtonDown(0))
        {
            core.TriggerAttack(); // クリックした瞬間の情報を送信
        }

        // ジャンプ入力
        if (Input.GetKeyDown(KeyCode.Space))
        {
            core.TriggerJump();
        }
    }
}