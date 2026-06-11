using UnityEngine;

public class ShrinkingHealthBar : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("体力バーのRectTransformをアサインしてください")]
    public RectTransform healthBarRect; 

    [Header("ステータス")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    // 初期状態のバーの長さを記憶しておく変数
    private float maxWidth; 

    void Start()
    {
        currentHealth = maxHealth;
        
        // ゲーム開始時のバーの幅（Width）を最大幅として取得
        maxWidth = healthBarRect.sizeDelta.x; 
    }

    // ダメージ処理用のメソッド（敵が攻撃を受けた時に呼び出す）
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        // 体力が0未満にならないように制限
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
        
        UpdateHealthBar();
    }

    // 体力バーの長さを更新する処理
    private void UpdateHealthBar()
    {
        // 現在の体力が最大体力の何％か（0.0 ～ 1.0）を計算
        float healthRatio = currentHealth / maxHealth;
        
        // 最大幅に割合を掛けて、新しい幅を算出
        float newWidth = maxWidth * healthRatio;
        
        // 高さ（y）はそのまま維持し、幅（x）だけ新しい値に上書きする
        healthBarRect.sizeDelta = new Vector2(newWidth, healthBarRect.sizeDelta.y);
    }
}