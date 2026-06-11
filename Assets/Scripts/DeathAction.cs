using System.Collections;
using UnityEngine;

public class DeathAction : MonoBehaviour
{
    [Header("アニメーション設定")]
    [Tooltip("Animatorのパラメーター名")]
    public string dieParameterName = "Die";
    [Tooltip("Triggerとして呼び出す場合はチェック、Bool値(true)として呼ぶ場合は外す")]
    public bool useTrigger = true;

    [Header("消滅設定")]
    [Tooltip("死亡後にオブジェクトを削除します（敵はオン、プレイヤーはオフを推奨）")]
    public bool destroyOnDeath = true;
    [Tooltip("死亡アニメーションが再生されてから消滅するまでの時間（秒）")]
    public float destroyDelay = 2.0f;

    private bool isDead = false;

    public void ExecuteDeath()
    {
        if (isDead) return;
        isDead = true;

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            if (useTrigger) anim.SetTrigger(dieParameterName);
            else anim.SetBool(dieParameterName, true);
        }

        // ★追加：CharacterCoreなどの移動スクリプトが動こうとするのを強制停止する
        MonoBehaviour charCore = GetComponent("CharacterCore") as MonoBehaviour;
        if (charCore != null) charCore.enabled = false;

        // 床抜け防止処理（CharacterController用）
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; 

        // 床抜け防止処理（Rigidbody用：重力を切る）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; 

        // 敵のみコライダー（当たり判定）を消す
        Collider col = GetComponent<Collider>();
        if (col != null && destroyOnDeath)
        {
            col.enabled = false;
        }

        if (destroyOnDeath)
        {
            StartCoroutine(DestroyRoutine());
        }
    }
    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}