using Code.Scripts.EventSystems;
using EventTypes.DialogueEvents;
using StateMachine.Npcs;
using Unity.VisualScripting;
using UnityEngine;

public class MultipleChoiceText : MonoBehaviour
{
    
    [SerializeField] public ChoiceButton[] buttons;

    public void PositiveButton()// calls positive response function from npc
    {
        EventManager.Instance?.Publish(new IDialogueContextEvent
        {
            Context = DialogueContext.Positive
        });
        TextIndex.Instance.StartTextVisible();
    }

    public void NegativeButton()// calls negative response function from npc
    {
        EventManager.Instance?.Publish(new IDialogueContextEvent
        {
            Context = DialogueContext.Negative
        });
        TextIndex.Instance.StartTextVisible();
    }

    public void NeutralButton()// calls neutral response function from npc
    {
        //EventManager.Instance.OnEvent(EventTypes.NEUTRAL);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
}