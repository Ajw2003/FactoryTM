using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Code.Scripts.Audio;
using Code.Scripts.EventSystems;
using Code.Scripts.EventSystems.EventTypes;
using Code.Scripts.Interfaces.EventTypes;
using Code.Scripts.Player.PlayerStateMachine;
using EventTypes.DialogueEvents;
using EventTypes.InventoryEvents;
using Singleton;
using SOs;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TextIndex : SingletonBase<TextIndex>
{
    private string _currentString;
    private DialogueSo _dialogueSo;
    private DialogueLine[] _dialogueLines;
    [SerializeField] private float timePerCharacter = 0.025f;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private AudioClip speaking;
    [SerializeField] private float characterPitch;
    [SerializeField] private float secondaryDelay;
    [field: SerializeField] public GameObject canvas { get; set; }
    [SerializeField] private GameObject textBox;
    [SerializeField] private GameObject multipleChoiceTextBox;
    [SerializeField] private GameObject enterAnswerTextBox;
    private MultipleChoiceText _multipleChoiceText;
    private TextInput _textInput;
    public bool hasBeenEnabled;
    public bool canSpeak;
    public bool Continue;
    public bool SpokenTo;
    private Button _button;
    private int _currentTextIndex;
    private DialogueType _type;
    public DialogueContext dialogueContext;
    private float _totalTime;
    private WaitForSeconds _waitForSeconds;
    private WaitForSeconds _timeToNextLine;
    private PlayerStateMachine _stateMachineRef;
    public Coroutine CurrentVisableText;
    [SerializeField] private float timeTillTextDisappears;
    [SerializeField] private float timeBetweenCharacters;
    [SerializeField] Image charaSprite;

    private PlayerStateMachine _playerStateMachine;

    [SerializeField] private CharacterProfiles profiles;

    private void OnEnable()
    {
        EventManager.Instance?.Subscribe(this, (IDialogueContextEvent e) => ChangeDialogueContext(e.Context));
        EventManager.Instance?.Subscribe(this, (DialogueSoEvent e) => SetDialogueSo(e.DialogueSo));
        _multipleChoiceText = multipleChoiceTextBox.GetComponent<MultipleChoiceText>();
        _textInput = enterAnswerTextBox.GetComponent<TextInput>();
        _button = _textInput.GetComponentInChildren<Button>();
        _playerStateMachine = FindFirstObjectByType<PlayerStateMachine>();
    }

    private CanvasGroup _canvasGroup;

    void Start()
    {
        canvas.SetActive(true);
        _canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = canvas.AddComponent<CanvasGroup>();
        }
        _waitForSeconds = new WaitForSeconds(timeBetweenCharacters);
        multipleChoiceTextBox.SetActive(false);
        enterAnswerTextBox.SetActive(false);
        textBox.SetActive(false);
        HideCanvas(); // Initially hide the canvas
        _stateMachineRef = FindFirstObjectByType<PlayerStateMachine>();
    }
    
    public void CustomEventTrigger()
    {
        if (EventManager.DebugLoggingEnabled) Debug.Log($"NpcStateMachine: CustomEventTrigger() called.");
    }

    public void CanSpeak()
    {
        canSpeak = true;
    }

    public void ShowCanvas()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void HideCanvas()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    

    private void DialogueDisplayOverride(DialogueSo dialogueSo)
    {
        if (dialogueSo == null) return;
        if (CurrentVisableText != null) return;
        SetDialogueSo(dialogueSo);
        canSpeak = true;
        StartTextVisible();
        ShowCanvas();
    }

    public void SetDialogueSo(DialogueSo dialogueSo)
    {
        _dialogueSo = dialogueSo;
        _dialogueLines = _dialogueSo.MainDialogue;
        if (CurrentVisableText != null)
        {
            StopCoroutine(CurrentVisableText);
            CurrentVisableText = null;
        }

        // Debug.Log("Set");
    }

    public void StartTextVisible()
    {
        CurrentVisableText = StartCoroutine(TextVisible());
        SpokenTo = true;
    }

    public void ChangeDialogueContext(DialogueContext context)
    {
        if (CurrentVisableText != null)
        {
            StopCoroutine(CurrentVisableText);
            CurrentVisableText = null;
        }

        dialogueContext = context;
        switch (dialogueContext)
        {
            case DialogueContext.Main:
                if (_dialogueSo.MainDialogue.Length <= 0) return;
                _currentTextIndex = 0;
                _dialogueLines = _dialogueSo.MainDialogue;
                break;
            case DialogueContext.Neutral:
                if (_dialogueSo.NeutralMessage.Length <= 0) return;
                _currentTextIndex = 0;
                _dialogueLines = _dialogueSo.NeutralMessage;
                _textInput.correctPhrase = _dialogueSo.Answer;
                break;
            case DialogueContext.Negative:
                if (_dialogueSo.NegativeMessage.Length <= 0) return;
                _currentTextIndex = 0;
                _dialogueLines = _dialogueSo.NegativeMessage;
                break;
            case DialogueContext.Positive:
                if (_dialogueSo.PositiveMessage.Length <= 0) return;
                _currentTextIndex = 0;
                _dialogueLines = _dialogueSo.PositiveMessage;
                break;
        }
    }
    

    public void DisableUi()
    {
        _currentTextIndex = 0;
        StopAllCoroutines();
        hasBeenEnabled = false;
        canSpeak = false;
        EventManager.Instance?.Publish(new DialogueInputEvent
        {
            IsEnabled = false
        });
    }

    public IEnumerator TextVisible() // this does the magic, basically just showing text character by character via a coroutine
    {
        EnableText();
        _currentTextIndex = 0;
        foreach (var dialogueLine in _dialogueLines)
        {
            Continue = false;
            var message = _dialogueLines[_currentTextIndex].Line;
            _totalTime = message.Length * timePerCharacter;
            _timeToNextLine = new WaitForSeconds(_totalTime);
            _type = _dialogueLines[_currentTextIndex].dialogueType;
            Speaker currentSpeaker = _dialogueLines[_currentTextIndex].Speaker;
            var character = profiles.GetCharacter(currentSpeaker);
            
            charaSprite.sprite = character.sprite;
            speakerName.text = character.charSpeaker.ToString();
            speakerName.color = character.dialogueColor;
            _text.color = character.dialogueColor;
            characterPitch = character.charPitch;

            if (_dialogueLines[_currentTextIndex].gift != null)
            {
                var gift = _dialogueLines[_currentTextIndex].gift;
                if (gift != null) // Add null checks for gift and its itemData
                {
                    EventManager.Instance?.Publish(new InventoryItemEvent { Item = gift, Exists = true });
                    EventManager.Instance?.Publish(new AddItemEvent{Item = gift});
                    Debug.Log(gift);
                }
            }
            
            for (var i = 0; i < message.Length; i++)
            {
                if (string.IsNullOrEmpty(message)) Debug.LogWarning("Message is empty!");
                _currentString = message.Substring(0, i + 1);
                _text.text = _currentString;
                char currentChar = message[i];
                
                if ("aeiou".Contains(Char.ToLower(currentChar)) && speaking)
                {
                    {
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.sfxSource.pitch = characterPitch + UnityEngine.Random.Range(-AudioManager.Instance.pitchVariation, AudioManager.Instance.pitchVariation);
                            EventManager.Instance?.Publish(new AudioClipEvent { Channel = AudioChannel.Sfx, Clip = speaking });
                        }
                    }
                }

                if (Continue) break;
                yield return _waitForSeconds;
            }

            _text.text = _dialogueLines[_currentTextIndex].Line;
            Continue = false;
            
            var timeIncrementer = 0f;
            var timeLimit = _timeToNextLine;
            while (timeIncrementer < _totalTime)
            {
                if (Continue) break;
                timeIncrementer += 0.01f;
                yield return new WaitForSeconds(.01f);
            }

            if (_currentTextIndex < _dialogueLines.Length - 1)
            {
                _currentTextIndex++;
                if (_dialogueLines[_currentTextIndex].SoundEffect)
                {
                    EventManager.Instance?.Publish(new AudioClipEvent
                    {
                        Clip = _dialogueLines[_currentTextIndex].SoundEffect 
                    });
                }
            }
            else
            {
                break;
            }
        }

        switch (_type)
        {
            case DialogueType.Single:
                Invoke(nameof(DisableText), timeTillTextDisappears);
                if (CurrentVisableText != null)
                {
                    StopCoroutine(CurrentVisableText); //stop the coroutine once done
                }
                CurrentVisableText = null;
                break;
            case DialogueType.EnterAns:
                EnableText();
                break;
            case DialogueType.MultipleChoice:
                EnableText();
                break;
        }
    }

    public void DisableText()
    {
        switch (_type)
        { 
            case DialogueType.Single:
                textBox.SetActive(false);
                HideCanvas();
                EventManager.Instance.Publish(new TextFinished());
                if (CurrentVisableText != null)
                {
                    StopCoroutine(CurrentVisableText);
                    CurrentVisableText = null;
                }

                EventManager.Instance?.Publish(new DialogueInputEvent
                {
                    IsEnabled = false
                });
                if(_playerStateMachine.PreviousState == _playerStateMachine.PuzzleState) return;
                EventManager.Instance?.Publish(new PlayerStateOverrideToIdleEvent());
                break;
            case DialogueType.EnterAns:
                _button.onClick.RemoveListener(DelayedDisable);
                Invoke(nameof(DelayedDisable), secondaryDelay);
                break;
            case DialogueType.MultipleChoice:
                foreach (var button in _multipleChoiceText.buttons)
                {
                    button.button.onClick.RemoveListener(DelayedDisable);
                }
                Invoke(nameof(DelayedDisable), secondaryDelay);
                break;
        }

        textBox.SetActive(false);
    }

    public void DelayedDisable()
    {
        multipleChoiceTextBox.SetActive(false);
        enterAnswerTextBox.SetActive(false);
    }

    public void ResetIndex()
    {
        _currentTextIndex = 0;
    }

    public void EnableText()
    {
        EventManager.Instance?.Publish(new DialogueInputEvent
        {
            IsEnabled = true
        });
        switch (_type)
        {
            case DialogueType.Single:
                EventManager.Instance?.Publish(new PlayerStateOverrideToDialogueEvent());
                textBox.SetActive(true);
                break;
            
            case DialogueType.EnterAns:
                textBox.SetActive(true);
                enterAnswerTextBox.SetActive(true);
                if (!hasBeenEnabled)
                {
                    _button.onClick.AddListener(DelayedDisable);
                    _textInput.correctPhrase = _dialogueSo.Answer;
                    hasBeenEnabled = true;
                }
                break;
            
            case DialogueType.MultipleChoice:
                textBox.SetActive(true);
                multipleChoiceTextBox.SetActive(true);
                if (!hasBeenEnabled && _multipleChoiceText.buttons != null && _multipleChoiceText != null)
                {
                    int index = -1;
                    foreach (var button in _multipleChoiceText.buttons)
                    {
                        index = (index + 1) % _multipleChoiceText.buttons.Length;
                        if (index > _dialogueSo.MainDialogue[_currentTextIndex].options.Length) break;
                        button.button.onClick.AddListener(DelayedDisable);
                        button.buttonText.text = _dialogueSo.MainDialogue[_currentTextIndex].options[index];
                    }

                    hasBeenEnabled = true;
                }
                break;
        }
    }
}