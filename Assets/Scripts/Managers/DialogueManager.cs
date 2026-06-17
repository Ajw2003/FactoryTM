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
    
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        currentDialogueIndex = 0;
        SetDialogue();
        StartCoroutine(DialogueCoroutine());
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
        if(_currentlyWriting) return;
        StopCoroutine(DialogueCoroutine());
        var newIndex = (currentDialogueIndex + 1) % currentDialogue.dialogues.Length;
        currentDialogueIndex = newIndex;
        SetDialogue();
        StartCoroutine(DialogueCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncrementDialogueIndex();
        }
    }
}
