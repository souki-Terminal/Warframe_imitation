using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DamageTextManager : MonoBehaviour
{
    private static DamageTextManager _instance;
    public static DamageTextManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DamageTextManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("DamageTextManager");
                    _instance = obj.AddComponent<DamageTextManager>();
                }
            }
            return _instance;
        }
    }

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (_instance == null) 
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定された位置にダメージテキストを表示します。
    /// </summary>
    public void ShowDamageText(Vector3 position, int damageAmount, bool isCritical, bool isPlayerDamage = false)
    {
        GameObject textObj = null;

        // プールから取得を試みる
        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj != null)
            {
                textObj = obj;
                textObj.SetActive(true);
                break;
            }
        }

        // プールに空きがない場合は新規作成
        if (textObj == null)
        {
            textObj = new GameObject("DamageText");
            textObj.transform.SetParent(transform);
            textObj.AddComponent<DamageText>();
        }

        textObj.transform.position = position;
        
        DamageText dmgText = textObj.GetComponent<DamageText>();
        if (dmgText != null)
        {
            dmgText.Setup(damageAmount, isCritical, isPlayerDamage);
        }
    }

    public void ReturnToPool(GameObject textObj)
    {
        if (textObj != null)
        {
            textObj.SetActive(false);
            pool.Enqueue(textObj);
        }
    }
}
