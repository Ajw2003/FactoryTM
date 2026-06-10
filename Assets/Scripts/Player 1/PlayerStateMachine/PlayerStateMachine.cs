using System.Collections;
using Code.Scripts.EventSystems;
using Code.Scripts.EventSystems.EventTypes.EmptyEvents;
using EventTypes.StateEvents;
using StateMachine;
using UnityEngine;

namespace Code.Scripts.Player.PlayerStateMachine
{
    public class PlayerStateMachine : BaseStateMachine
    {
        public PlayerState currentState { get; set; }
        public PlayerState PreviousState { get; set; }

        public SpriteRenderer PlayerSprite => GetComponent<SpriteRenderer>();

        public string PreviousStateName { get; private set; }

        public float walkSpeed;
        public float sprintSpeed;

        public Animator animator;
        public string previousDirection;
        public Rigidbody2D rb2D;
    
        private bool _hasOpenedInventory;

        public string animationNameToPlay;

        private string _previousAnimationName;

        public bool withinNpcTrigger;

        public Vector2 currentMovement;

        public int maxEscapeCharge;

        public int currentEscapeCharge;
        public bool started;

        private Coroutine _escapeChargeRoutine;

        //These scripts are the SPECIFIC SINGLE STATE SCRIPTS
        public PlayerInventoryState InventoryState { get; private set; }
        public PlayerWalkState WalkState { get; private set; }
        public PlayerRunState RunState { get; private set; }
        public PlayerIdleState IdleState { get; private set; }
        public PlayerCaughtState CaughtState { get; private set; }
        public PlayerDeadState DeadState { get; private set; }
        public PlayerDialogueState DialogueState { get; private set; }
        public PlayerPuzzleState PuzzleState { get; private set; }

        private void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            //Instantiating all specific state scripts
            InventoryState = new PlayerInventoryState(this);
            WalkState = new PlayerWalkState(this);
            RunState = new PlayerRunState(this);
            CaughtState = new PlayerCaughtState(this);
            DeadState = new PlayerDeadState(this);
            DialogueState = new PlayerDialogueState(this);
            PuzzleState = new PlayerPuzzleState(this);
            IdleState = new PlayerIdleState(this);
            animator = GetComponent<Animator>();
            rb2D = GetComponent<Rigidbody2D>();
        }

        public void Start()
        {
            EventManager.Instance?.Subscribe(this, (PlayerStateChangeEvent e) => StateChange(e.NextState));
            EventManager.Instance?.Subscribe(this, (PlayerStateOverrideToDialogueEvent e) => ChangeToDialogue());
            EventManager.Instance?.Subscribe(this, (PlayerStateOverrideToIdleEvent e) => ChangeToIdle());
            EventManager.Instance?.Subscribe(this, (PlayerStateOverrideToCaughtEvent e) => ChangeToCaught());
            EventManager.Instance?.Subscribe(this, (IPlayerStateOverrideToPuzzleEvent e) => ChangeToPuzzle());
            EventManager.Instance?.Subscribe(this, (NpcColliderEvent e) => NpcCollisionEntered(e.Entered));
            EventManager.Instance?.Subscribe(this, (TeleportPlayerEvent e) => TeleportPlayer(e.Destination));
            EventManager.Instance?.Subscribe(this, (RespawnPlayerEvent e) => RespawnPlayer(e.Destination));
            ChangeState(IdleState);
            currentState = IdleState;
        }

        public void NpcCollisionEntered(bool entered)
        {
            withinNpcTrigger = entered;
        }

        private void RespawnPlayer(Transform destination)
        {
            if (currentState == DialogueState || currentState == PuzzleState) return;
            EventManager.Instance?.Publish(new PlayerStateChangeEvent
            {
                NextState = DeadState
            });
            EventManager.Instance?.Publish(new LoseAgroEvent());
            gameObject.transform.position = destination.position;
            ChangeToIdle();
        }

        private void TeleportPlayer(Transform destination)
        {
            rb2D.position = destination.position;
            ChangeToIdle();
        }

        public void ChangeToDialogue()
        {
            if(PreviousState == PuzzleState) return;
            rb2D.linearVelocity = new Vector2(0, 0);
            EventManager.Instance?.Publish(new PlayerStateChangeEvent
            {
                NextState = DialogueState
            });
        }

        public void ChangeToIdle()
        {
            rb2D.linearVelocity = new Vector2(0, 0);
            if (_escapeChargeRoutine != null) StopCoroutine(_escapeChargeRoutine);
            started = false;
            currentEscapeCharge = 0;
            EventManager.Instance?.Publish(new PlayerStateChangeEvent
            {
                NextState = IdleState
            });
        }

        public void ChangeToCaught()
        {
            rb2D.linearVelocity = new Vector2(0, 0);
            EventManager.Instance?.Publish(new PlayerStateChangeEvent
            {
                NextState = CaughtState
            });
        }

        public void ChangeToPuzzle()
        {
            rb2D.linearVelocity = new Vector2(0, 0);
            EventManager.Instance?.Publish(new PlayerStateChangeEvent
            {
                NextState = PuzzleState
            });
        }

        public void Sprint() //Called by PlayerInputController script
        {
            if (PreviousState == DialogueState || PreviousState == PuzzleState) return;

            EventManager.Instance?.Publish(new PlayerStateChangeEvent
            {
                NextState = RunState
            });
        }

        public void NoLongerSprinting()
        {
            EventManager.Instance?.Publish(new PlayerStateChangeEvent
            {
                NextState = WalkState
            });
        }

        public void Move(Vector2 movement) //Called by PlayerInputController script
        {
            if (PreviousState == DialogueState || PreviousState == PuzzleState) return;

            if (currentState != WalkState)
                EventManager.Instance?.Publish(new PlayerStateChangeEvent
                {
                    NextState = WalkState
                });

            currentMovement = movement;
        }

        public void OpenInventory() //Called by PlayerInputController script
        {
            if (PreviousState == DialogueState || PreviousState == PuzzleState) return;
            if (_hasOpenedInventory == false)
            {
                EventManager.Instance?.Publish(new PlayerStateChangeEvent
                {
                    NextState = InventoryState
                });
                EventManager.Instance?.Publish(new OtherInventoryEvent
                {
                    PlayerAction = PlayerActions.OpenInventory
                });
                _hasOpenedInventory = true;
            }

            else
            {
                EventManager.Instance?.Publish(new PlayerStateChangeEvent
                {
                    NextState = IdleState
                });
                _hasOpenedInventory = false;
                EventManager.Instance?.Publish(new OtherInventoryEvent
                {
                    PlayerAction = PlayerActions.ExitInventory
                });
            }
        }

        public void IncrementEscapeCharge()
        {
            currentEscapeCharge++;
            if (!started)
            {
                if (_escapeChargeRoutine != null) StopCoroutine(_escapeChargeRoutine);
                _escapeChargeRoutine = StartCoroutine(TryEscape());
                started = true;
            }
        }

        public IEnumerator TryEscape()
        {
            while (currentEscapeCharge < maxEscapeCharge && currentEscapeCharge > 0)
            {
                currentEscapeCharge--;
                yield return new WaitForSeconds(0.5f);
            }

            if (currentEscapeCharge >= maxEscapeCharge)
            {
                ChangeToIdle();
                EventManager.Instance?.Publish(new LoseAgroEvent());
            }

            yield return null;
        }

        public override void Update()
        {
        }

        public void SetAnimationToPlay(string nameOfAnimation) //Excludes blend trees, called by the specific state scripts 
        {
            if(animator == null) return;
            if (!animator.HasState(0, Animator.StringToHash(nameOfAnimation))) return;
            animationNameToPlay = nameOfAnimation;
            animator.Play(animationNameToPlay);
        }

        public void StateChange(PlayerState nextState)
        {
            currentState = nextState;
            PreviousStateName = PreviousState.ToString();
            ChangeState(nextState);
        }
    }
}