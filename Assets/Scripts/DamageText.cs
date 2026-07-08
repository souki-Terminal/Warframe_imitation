using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float moveSpeed = 2.0f;
    private float alphaSpeed = 1.0f;
    private float destroyTime = 1.0f;
    private Color textColor;
    private float timer = 0f;

    public void Setup(int damageAmount, bool isCritical, bool isPlayerDamage = false)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshPro>();

        textMesh.text = damageAmount.ToString();
        textMesh.alignment = TextAlignmentOptions.Center;
        
        // フォントサイズ・色の設定
        textMesh.fontSize = isCritical ? 6f : 4f;
        
        if (isPlayerDamage)
        {
            textColor = Color.red; // プレイヤーが受けたダメージは赤
        }
        else if (isCritical)
        {
            textColor = new Color(1f, 0.6f, 0f); // 敵へのクリティカルはオレンジ
            textMesh.text += "!";
        }
        else
        {
            textColor = Color.white; // 敵への通常ダメージは白
        }

        textMesh.color = textColor;
        
        // 少しランダムな位置にずらして重なりを防ぐ
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f), Random.Range(-0.5f, 0.5f));

        timer = destroyTime;
    }

    private void Update()
    {
        // 上へ移動
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 徐々にフェードアウト
        if (textMesh != null)
        {
            textColor.a -= alphaSpeed * Time.deltaTime;
            textMesh.color = textColor;
        }

        // カメラの方を向かせる（ビルボード処理）
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (DamageTextManager.instance != null)
            {
                DamageTextManager.instance.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
