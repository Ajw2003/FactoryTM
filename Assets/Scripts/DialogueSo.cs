using System;
using System.Collections;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "NpcDialogueSO", menuName = "Scriptable Objects/DialogueSO")]
    public class DialogueSo : ScriptableObject
    {
        [field:SerializeField] public DialogueLine[] MainDialogue{ get; private set; }// each dialogue line is one line of dialogue and all of the data that could need to be refernced by a given line
    
        [field:SerializeField]public DialogueLine[] PositiveMessage{ get; private set; }// the positive messages contains a branch of dialogue triggered by a positive answer to a multiple choice question
    
        [field:SerializeField]public DialogueLine[] NegativeMessage{ get; private set; }// the negative messages contains a branch of dialogue triggered by a negative answer to a multiple choice questions

        [field:SerializeField]public DialogueLine[] NeutralMessage{ get; private set; }//the neutral messages contains a branch of dialogue triggered by a neutral answer to a multiple choice questions
        
        [field:SerializeField]public string Answer { get; private set; }
        
        
    }

    [Serializable]
    public struct DialogueLine// contains an enum for the speaker, an enum for the type of dialogue, a sprite for the npc, a string for the line of dialogue spoken,
                              // an array of strings for the names of options for multiple choice questions, and a string for the answer used in an input prompt
    {
        public Speaker Speaker;
        public DialogueType dialogueType;
        public Sprite sprite;
        [TextArea(1,10)] public string Line;
        [TextArea(1, 10)] public string[] options;
        public AudioClip SoundEffect;
        public InventoryManager.InventoryItem gift;
        public bool hasGifted;
    }
    
    
    public enum Speaker// add npc names here
    {
        Bellboy, Receptionist, HotelGuest, Player, IslandRep, Host, Dealer, ShopOwner, Cultist, God, Tourist, Local
    }

    public enum DialogueType// choose one of these to determine if the dialogue will ask for a input prompt, a multiple choice answer, or simply continue to the next line
    {
        Single,MultipleChoice,EnterAns
    }
    
}
