using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "PlayerDialogueSO", menuName = "Scriptable Objects/PlayerDialogueSO")]
public class PlayerDialogueSO : ScriptableObject
{
    [field:SerializeField, TextArea(1,10)]public string[] CurrentDialogue{ get; private set; }
    
    [field:SerializeField, TextArea(1,10)]public List<string> PositiveDialogue{ get; private set; }
    
    [field:SerializeField, TextArea(1,10)]public List<string> NegativeDialogue{ get; private set; }
    
    [field:SerializeField, TextArea(1,10)]public List<string> NeutralDialogue{ get; private set; }

}
