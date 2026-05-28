using System;
using UnityEngine;
using UnityEngine.AI;

public enum MemberType
{
    lackie,
    enforcer,
    bossman,
    legend,
    kingpin
}

public class CartelMember : MonoBehaviour
{

    public MemberType Rank { get; private set; }
    
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private float speed;

    [SerializeField] private float turnSpeed;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private BaseWeapon weapon;

    private Transform playerTransform;
    private Health health;

    void Start()
    {
        Setup();
        health = GetComponent<Health>();
        if (health == null) health = gameObject.AddComponent<Health>();
        
        health.onDeath.AddListener(OnDeath);
        
        if (weapon != null)
        {
            weapon.isPlayerControlled = false;
        }
    }

    void Update()
    {
        if (GameManager.Instance.player == null) return;
        
        playerTransform = GameManager.Instance.player.transform;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            MoveToPlayer();
            
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
        }
    }

    private void MoveToPlayer()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    private void AttackPlayer()
    {
        if (weapon != null)
        {
            weapon.Shoot(playerTransform.position);
        }
    }

    private void OnDeath()
    {
        Debug.Log($"Cartel Member {Rank} died!");
        Destroy(gameObject);
    }

    public void Setup()
    {
        switch (Rank)
        {
            case MemberType.lackie:
                Rank = MemberType.lackie;
                break;
            case MemberType.enforcer:
                Rank = MemberType.enforcer;
                break;
            case MemberType.bossman:
                Rank = MemberType.bossman;
                break;
            case MemberType.legend:
                Rank = MemberType.legend;
                break;
            case MemberType.kingpin:
                Rank = MemberType.kingpin;
                break;
            default:
                break;
        }
    }
}
