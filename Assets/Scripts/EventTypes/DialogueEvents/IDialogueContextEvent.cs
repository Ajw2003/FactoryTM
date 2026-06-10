using EventSystems;
using EventTypes.DialogueEvents;
using UnityEngine;

public class IDialogueContextEvent :IEvent
{
    public DialogueContext Context { get; set; }
}
