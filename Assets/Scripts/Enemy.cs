using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterCore))]
public class Enemy : MonoBehaviour
{
    public GameObject goal;
    private NavMeshAgent agent;
    private CharacterCore core;

    public float attackRange = 2.0f; 
    public float attackCooldown = 1.5f; 
    private float lastAttackTime = 0f; 

    void Start()
    {
        core = GetComponent<CharacterCore>();
        agent = GetComponent<NavMeshAgent>();

        if (goal == null) goal = GameObject.Find("unitychan"); 
        if (agent == null) Debug.LogError($"{gameObject.name} に NavMeshAgent がありません！");
    }

    void Update()
    {
        if (goal == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, goal.transform.position);

        // 攻撃範囲に入ったか判定
        if (distance <= attackRange)
        {
            core.SetAttack(true); // 範囲内にいる間はON(Bool)を送信

            // クールダウンごとに単発攻撃(Trigger)を送信
            if (Time.time - lastAttackTime > attackCooldown)
            {
                core.TriggerAttack(); 
                lastAttackTime = Time.time;
            }
        }
        else
        {
            core.SetAttack(false); // 範囲外ならOFF
        }
        
        agent.destination = goal.transform.position;
    }
}