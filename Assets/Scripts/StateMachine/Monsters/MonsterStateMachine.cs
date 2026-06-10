using System;
using System.Collections;
using System.Collections.Generic;
using Code.Scripts.EventSystems;
using Code.Scripts.EventSystems.EventTypes.EmptyEvents;
using Code.Scripts.Player.PlayerStateMachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace StateMachine.Monsters
{
    public class MonsterStateMachine : BaseStateMachine
    {
        private MonsterAgroState _agroState;
        private MonsterDeadState _deadState;
        private MonsterEnterState _enterState;
        private MonsterIdleState _idleState;
        private MonsterPatrollingState _patrollingState;
        private MonsterExitState _exitState;
        private MonsterPlayerCaughtState _caughtState;
    
        [SerializeField] private string sName;
        [SerializeField] private int sightRange;
        [SerializeField] public NavMeshAgent agent;
        public int secondsTillFinish;

        public float coolDown;

        public float currentCooldown;

        public MonsterBaseState PreviousState { get; set; }
        [field:SerializeField]public Transform Target { get; set; }
    
        [SerializeField] private bool canAgro = true;

        [field: SerializeField] public Transform RespawnPoint { get; set; }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            _agroState = new MonsterAgroState(this);
            _exitState = new MonsterExitState(this);
            _deadState = new MonsterDeadState(this);
            _enterState = new MonsterEnterState(this);
            _patrollingState = new MonsterPatrollingState(this);
            _idleState = new MonsterIdleState(this);
            _caughtState = new MonsterPlayerCaughtState(this);
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            EventManager.Instance?.Subscribe(this, (LoseAgroEvent e) => LoseAgro());
            EventManager.Instance?.Subscribe(this, (SafeZoneEnteredEvent e) => LoseAgro());
            EventManager.Instance?.Subscribe(this, (SafeZoneLeftEvent e) => BecomeAgro());
            ChangeState(_agroState);

        }

        public void RegisterEvents()// register the events for this monster to listen to, currently just the light puzzle enter and exit and safe zone enter and exit
        {
            EventManager.Instance?.Subscribe(this, (TransformPassThroughEvent e) => SetRespawnPoint(e.transform));
        }

        private void SetRespawnPoint(Transform spawnPoint)
        {
            RespawnPoint = spawnPoint;
        }

        public void UnregisterEvents()
        {

        }

        public void DistanceCheck()// check distance between target and self
        {
            if (Target != null && canAgro)
            {
                var distanceBetween = Vector3.Distance(this.transform.position, Target.transform.position);
                if (distanceBetween <= sightRange && currentCooldown <= 0)
                {
                    if (Target != null)
                    {
                        agent.SetDestination((Target.position));
                    }
                }
            }
        }

        public void SetTarget()
        {
            if (Target == null)
            {
                Target = FindFirstObjectByType<PlayerStateMachine>().transform;
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (currentCooldown > 0 || PreviousState != _agroState) return;
            if (other.GetComponent<PlayerStateMachine>() && PreviousState == _agroState)
            {
                EventManager.Instance?.Publish(new PlayerStateOverrideToCaughtEvent());
                StartCoroutine(nameof(SecondsTillFinishRoutine));
                ScreenShaker.Instance?.Shake(secondsTillFinish, 0.15f);
            }
        }

        public IEnumerator SecondsTillFinishRoutine()
        {
            yield return new WaitForSeconds(secondsTillFinish);
            EventManager.Instance?.Publish(new FadeTransitionEvent());
            currentCooldown = coolDown;
            Invoke(nameof(RespawnPlayer),1.4f);
        }

        private void RespawnPlayer()
        {
            StartCoroutine(nameof(StartCooldown));
            EventManager.Instance?.Publish(new RespawnPlayerEvent()
            {
                Destination = RespawnPoint
            });
        }

        public void BecomeAgro()//change to agro state
        {
            if(currentCooldown > 0) return;
            ChangeState(_agroState);
        }

        public void LoseAgro()// change to searching state
        {
            
            // set target to patrol point and prevent switching back to agro with a boolean after set duration
            ScreenShaker.Instance.StopShake();
            ChangeState(_patrollingState);
            currentCooldown = coolDown;
            StartCoroutine(nameof(StartCooldown));
            StopCoroutine(nameof(SecondsTillFinishRoutine));
        }

        public IEnumerator StartCooldown()
        {
            while (currentCooldown > 0)
            {
                currentCooldown--;
                yield return new WaitForSeconds(1f);
            }
            if(canAgro) BecomeAgro();
        }
    }
}
