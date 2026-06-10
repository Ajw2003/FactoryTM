using StateMachine.Monsters;
using UnityEngine;

public class MonsterPlayerCaughtState : MonsterBaseState
{
    public MonsterPlayerCaughtState(MonsterStateMachine stateMachine) : base(stateMachine)
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void Update()
    {
        //put dragging player anim here
    }

    public override void Enter()
    {
        //put catching player anim here
    }
}
