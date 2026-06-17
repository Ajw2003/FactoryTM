using EventSystems;
using UnityEngine;

public class DialogueEvent : IEvent
{
    public bool enabled;
    public DialogueSO dialogue;
    public int index;
}
