using System.Collections;
using System.Collections.Generic;
using Code.Scripts.EventSystems;
using Code.Scripts.Player.PlayerStateMachine;
using EventTypes.DialogueEvents;
using SOs;
using UnityEngine;

namespace StateMachine.Npcs
{
    public class NpcStateMachine : BaseStateMachine
    {
        private string _currentString;
        private bool _hasBeenEntered = false;
        private bool _hasSpoken;
        private int _currentSoIndex;
        private DialogueContext _dialogueContext;
        
        [SerializeField] private bool _isDealer; //replace with better logic later
        [SerializeField] private List<DialogueSo> dialogueSos;
        [SerializeField] private DialogueSo currentDialogueSo;
        

        public void MainContext()
        {
            _dialogueContext = DialogueContext.Main;
            EventManager.Instance?.Publish(new IDialogueContextEvent { Context = _dialogueContext });
        }

        public void PositiveContext()
        {
            _dialogueContext = DialogueContext.Positive;
            EventManager.Instance?.Publish(new IDialogueContextEvent { Context = _dialogueContext });
        }

        public void NegativeContext()
        {
            _dialogueContext = DialogueContext.Negative;
            EventManager.Instance?.Publish(new IDialogueContextEvent { Context = _dialogueContext });
        }

        public void NeutralContext()
        {
            _dialogueContext = DialogueContext.Neutral;
            EventManager.Instance?.Publish(new IDialogueContextEvent { Context = _dialogueContext });
        }

        public void IncrementDialogueSos()
        {
            _currentSoIndex = (_currentSoIndex +1) %dialogueSos.Count;
            currentDialogueSo = dialogueSos[_currentSoIndex];
        }

        public void DecrementDialogueSos()
        {
            _currentSoIndex = (_currentSoIndex - 1) % dialogueSos.Count;
            currentDialogueSo = dialogueSos[_currentSoIndex];
        }

        public virtual void OnTriggerEnter2D(Collider2D other)// show text boxes and or prompts and subscribe from event which should only be listened to with regards to said answers
        {
            if (other.GetComponent<PlayerStateMachine>() && !_hasBeenEntered)
            {
                _hasBeenEntered = true;
                EventManager.Instance?.Subscribe(this, (NpcDialoguePassThroughEvent e) => EnableAndChangeCurrentDialogue());
                TextIndex.Instance?.ShowCanvas();
                TextIndex.Instance?.CanSpeak();
                
            }
            
        }

        public void DisplayOverride()
        {
            StartCoroutine(Display());
        }

        private IEnumerator Display()
        {
            yield return new WaitForSeconds(0.1f);
            TextIndex.Instance.StartTextVisible();

            TextIndex.Instance.ShowCanvas();
            yield return null;
        }

        public void EnableAndChangeCurrentDialogue()
        {
            EventManager.Instance?.Publish(new DialogueSoEvent{DialogueSo = currentDialogueSo });
            EventManager.Instance?.Publish(new IDialogueContextEvent{Context = _dialogueContext});

            _hasSpoken = true;
        }

        public void OnTriggerExit2D(Collider2D other)// hide text boxes and or prompts and unsubscribe from event which should only be listened to with regards to said answers
        {
            
            if (other.GetComponent<PlayerStateMachine>())
            {
                _hasBeenEntered = false;
                EventManager.Instance?.Unsubscribe<NpcDialoguePassThroughEvent>(this);
                TextIndex.Instance?.DisableUi();
                TextIndex.Instance?.HideCanvas();
                if (_hasSpoken && _dialogueContext == DialogueContext.Main)
                {
                    _dialogueContext = DialogueContext.Neutral;
                    EventManager.Instance?.Publish(new IDialogueContextEvent{Context = DialogueContext.Neutral});
                }
            }
        }
        
        
    }
}
