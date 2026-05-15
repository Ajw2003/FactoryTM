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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        
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
