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
    
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        SetDialogue();
        StartCoroutine(DialogueCoroutine());
    }

    private IEnumerator DialogueCoroutine()
    {
        foreach (var character in currentDialogueText)
        {
            yield return new WaitForSeconds(delayBetweenCharacters);
            text.text += character;
        }
        
    }

    private void SetDialogue()
    {
        currentDialogueIndex = 0;
        currentDialogueText = currentDialogue.dialogues[currentDialogueIndex].text;
        text.text = null;
    }

    private void IncrementDialogueIndex()
    {
        currentDialogueIndex+= 1 % currentDialogue.dialogues.Length;
    }
}
