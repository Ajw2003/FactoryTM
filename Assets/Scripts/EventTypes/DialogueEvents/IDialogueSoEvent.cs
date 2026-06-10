using EventSystems;
using SOs;
using UnityEngine;

namespace EventTypes.DialogueEvents
{
    public class DialogueSoEvent : IEvent
    {
        public DialogueSo DialogueSo { get; set; }
        public AudioClip soundEffect;
    }

    public enum DialogueContext 
    {
        Main,
        Neutral,
        Positive,
        Negative
    }
}