using System;
using System.Collections;
using System.Collections.Generic;
using Code.Scripts.EventSystems;
using Managers;
using Singleton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryTest : MonoBehaviour
{

    public DialogueSO currentDialogueSo;
    public int currentDialogueIndex;
    public float delayBetweenCharacters = 0.1f;
    public string currentDialogueText;

    private bool _currentlyWriting;
    private Coroutine _currentCoroutine;

    private GameObject _dialogueBox;
    private bool _canWrite;
    private TMP_Text _text;
    private Dialogue _currentDialogue;
    private Dialogue _previousDialogue;
    private Coroutine currentDialogueCoroutine;

    private void Start()
    {
        EventManager.Instance?.Subscribe(this, (NpcDialogueEvent e) => SetDialogue(e.Dialogue));
        
        _dialogueBox = new GameObject("DialoguePanel_Programmatic", typeof(RectTransform), typeof(Image));
        _dialogueBox.transform.SetParent(this.transform,false);
    
        RectTransform panelRt = _dialogueBox.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.anchoredPosition = new Vector2(0f, 20f);
        panelRt.sizeDelta = new Vector2(880f, 180f);
    
        Image panelImg = _dialogueBox.GetComponent<Image>();
        panelImg.color = new Color(0.01f, 0.05f, 0.01f, 0.95f);
        panelImg.raycastTarget = false;
    
        Outline outline = _dialogueBox.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);
        GameObject textGo = new GameObject("MainText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(_dialogueBox.transform, false);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0f, 0.18f);
        textRt.anchorMax = new Vector2(1f, 0.8f);
        textRt.offsetMin = new Vector2(30f, 5f);
        textRt.offsetMax = new Vector2(-30f, -5f);
    
        _text = textGo.GetComponent<TextMeshProUGUI>();
        _text.fontSize = 36;
        _text.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        _text.alignment = TextAlignmentOptions.TopLeft;
        _text.text = "";
        _text.raycastTarget = false;
        
        
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncrementIndex();
        }
    }

    private void SetDialogue(DialogueSO dialogue)
    {
        currentDialogueSo = dialogue;
        currentDialogueIndex = 0;
        CheckDialogue(dialogue);

    }

    private void CheckDialogue(DialogueSO dialogue)
    {
        if (currentDialogueSo.unlockedDialogues[currentDialogueIndex %currentDialogueSo.unlockedDialogues.Length ].HasRequirment)
        {
            if (StoryManager.Instance.CheckMarker(currentDialogueSo.unlockedDialogues[currentDialogueIndex %currentDialogueSo.unlockedDialogues.Length].lockedMarker))
            {
                _currentDialogue = currentDialogueSo.unlockedDialogues[currentDialogueIndex %currentDialogueSo.unlockedDialogues.Length];
            }
            else
            {
                _currentDialogue = currentDialogueSo.lockedDialogue;
            }
        }
        else
        {
            _currentDialogue = currentDialogueSo.unlockedDialogues[currentDialogueIndex %currentDialogueSo.unlockedDialogues.Length];
        }

        currentDialogueText = _currentDialogue.text;
        if(currentDialogueCoroutine != null) StopAllCoroutines();
        currentDialogueCoroutine = StartCoroutine(DialogueCoroutine());
    }

    private void IncrementIndex()
    {
        if (currentDialogueSo.unlockedDialogues.Length - 1 >= currentDialogueIndex)
        {
            currentDialogueIndex++;
            _previousDialogue = _currentDialogue;
            CheckDialogue(currentDialogueSo);
        }
        else
        {
            EventManager.Instance?.Publish(new NpcDialogueFinishedEvent{Dialogue =  currentDialogueSo});
        }
            
    }

    private IEnumerator DialogueCoroutine()
    {
        _canWrite = true;
        _currentlyWriting = true;

        if (_dialogueBox != null && !_dialogueBox.activeSelf)
        {
            _dialogueBox.SetActive(true);
        }

        if (_text != null) _text.text = "";
        foreach (var character in currentDialogueText)
        {
            yield return new WaitForSeconds(delayBetweenCharacters);
            if (_text != null) _text.text += character;
        }
        if(!StoryManager.Instance.CheckMarker(_currentDialogue.lockedMarker))
        {
            EventManager.Instance?.Publish(new StoryMarkerUnlockEvent{storyMarkerSo = _currentDialogue.publishedMarker});
        }
        _currentlyWriting = false;
    }
}
    