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
    public Dialogue[] dialogues;
}

[Serializable]
public struct Dialogue
{
    [TextArea(1,10)]public string text;
    public Sprite sprite;
    public Color color;
    public DialogueType type;
}

public enum DialogueType
{
    None,
    Monologue,
    Enemy,
    Announcer,
    Npc
}

