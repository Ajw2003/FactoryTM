using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSO", menuName = "Scriptable Objects/Dialogue")]
public class DialogueSO : ScriptableObject
{
    public Dialogue[] unlockedDialogues;
    public Dialogue lockedDialogue;
}

[Serializable]
public struct Dialogue
{
    [TextArea(1,10)]public string text;
    public Sprite sprite;
    public Color color;
    public DialogueType type;
    public StoryMarkerSo lockedMarker;
    public StoryMarkerSo publishedMarker;
    public bool HasRequirment => lockedMarker != null;
}

public enum DialogueType
{
    None,
    Monologue,
    Enemy,
    Announcer,
    Npc
}

