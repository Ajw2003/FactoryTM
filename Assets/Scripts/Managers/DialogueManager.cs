using System;
using System.Collections;
using Singleton;
using TMPro;
using UnityEngine;

public class DialogueManager : SingletonBase<DialogueManager>
{
    public DialogueSO currentDialogue;
    public int currentDialogueIndex;
    public float delayBetweenCharacters;
    public string currentDialogueText;
    
    private bool _currentlyWriting;
    private Coroutine _currentCoroutine;
    
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        currentDialogueIndex = 0;
        SetDialogue();
        _currentCoroutine = StartCoroutine(DialogueCoroutine());
    }

    private IEnumerator DialogueCoroutine()
    {
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
        var newIndex = (currentDialogueIndex + 1) % currentDialogue.dialogues.Length;
        currentDialogueIndex = newIndex;
        SetDialogue();
        _currentCoroutine = StartCoroutine(DialogueCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncrementDialogueIndex();
        }
    }
}
