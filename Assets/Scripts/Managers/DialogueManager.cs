using System;
using System.Collections;
using Code.Scripts.EventSystems;
using Singleton;
using TMPro;
using UnityEngine;

public class DialogueManager : SingletonBase<DialogueManager>
{
    public event Action OnDialogueEnded;
    public DialogueSO currentDialogue;
    public int currentDialogueIndex;
    public float delayBetweenCharacters;
    public string currentDialogueText;
    
    private bool _currentlyWriting;
    private Coroutine _currentCoroutine;
    
    [SerializeField] private GameObject dialogueBox;

    private bool _canWrite;
    
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        currentDialogueIndex = 0;
        SetDialogue();
        _currentCoroutine = StartCoroutine(DialogueCoroutine());
        EventManager.Instance.Subscribe(this,(DialogueEvent e) => ToggleUi(e.enabled));
        EventManager.Instance.Subscribe(this,(DialogueEvent e) => SetDialogueSo(e.dialogue));
    }

    private IEnumerator DialogueCoroutine()
    {
        _canWrite = true;
        foreach (var character in currentDialogueText)
        {
            yield return new WaitForSeconds(delayBetweenCharacters);
            text.text += character;
            _currentlyWriting = true;
        }

        _currentlyWriting = false;

    }

    private void SetDialogue()
    {
        currentDialogueText = currentDialogue.dialogues[currentDialogueIndex].text;
        text.text = null;
    }

    private void SetDialogueSo(DialogueSO newDialogue)
    {
        currentDialogue = newDialogue;
    }

    private void IncrementDialogueIndex()
    {
        if (_currentlyWriting)
        {
            StopCoroutine(_currentCoroutine);
            text.text = null;
            text.text = currentDialogueText;
            _currentlyWriting = false;
            return;
        }

        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        if (currentDialogueIndex + 1 >= currentDialogue.dialogues.Length)
        {
            ToggleUi(false);
            OnDialogueEnded?.Invoke();
            return;
        }
        var newIndex = currentDialogueIndex + 1;
        currentDialogueIndex = newIndex;
        SetDialogue();
        _currentCoroutine = StartCoroutine(DialogueCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canWrite)
        {
            IncrementDialogueIndex();
            _canWrite = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _canWrite = !_canWrite;
            ToggleUi(_canWrite);
        }
    }

    private void ToggleUi(bool enabled)
    {
        switch (enabled)
        {
            case false :
                _canWrite = false;
                text.text = null;
                dialogueBox.SetActive(false);
                _currentlyWriting = false;
                StopCoroutine(_currentCoroutine);
                break;
            case true :
                currentDialogueIndex = 0;
                SetDialogue();
                dialogueBox.SetActive(true);
                _currentCoroutine = StartCoroutine(DialogueCoroutine());
                _canWrite = true;
                break;
        }
    }
}
