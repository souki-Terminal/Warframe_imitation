using UnityEngine;
using UnityEngine.InputSystem; // 1. これが必要

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 5f;
    public float gravity = -9.81f;
    Vector3 velocity;

    void Update()
    {
        // 2. 新しい Input System で入力を取得
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            // WASD や 矢印キーの入力を取得
            float x = 0;
            float z = 0;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;
            
            moveInput = new Vector2(x, z);
        }

        // 3. 移動方向を計算 (x, z を moveInput から取得)
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // 4. 移動の実行
        controller.Move(move * speed * Time.deltaTime);

        // 5. 重力の設定
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}