using Code.Scripts.EventSystems;
using EventTypes.DialogueEvents;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TextInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField; 
    [SerializeField] private Button submitButton;  
    [SerializeField] private TMP_Text feedbackText;    
    
     public string correctPhrase;

    void Start()
    {
        submitButton.onClick.AddListener(CheckInput); // Attach event to button
    }

    public void CheckInput()
    {
        string userInput = inputField.text.Trim(); // Get user input
        Debug.Log($"User input: '{userInput}', Correct phrase: '{correctPhrase}'");

        if (userInput.Equals(correctPhrase, System.StringComparison.OrdinalIgnoreCase))// get input ignoring case of letters
        {
            //feedbackText.text = "Correct input!"; // Display success message
            //feedbackText.color = Color.green;    // Change text color
            EventManager.Instance?.Publish(new IDialogueContextEvent
            {
                Context = DialogueContext.Positive
            });
            TextIndex.Instance?.StopAllCoroutines();
            TextIndex.Instance?.StartTextVisible();
            EventManager.Instance?.Publish(new CorrectEvent());
        }
        else
        {
            //feedbackText.text = "Incorrect input, try again."; // Error message
            //feedbackText.color = Color.red;      // Change text color
            EventManager.Instance?.Publish(new IDialogueContextEvent
            {
                Context = DialogueContext.Negative
            });
            TextIndex.Instance?.StopAllCoroutines();
            TextIndex.Instance?.StartTextVisible();
        }

        inputField.text = ""; // Clear input field after submission
    }
    
    
}
