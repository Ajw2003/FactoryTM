using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSprint", menuName = "Player Scriptable Objects/Create Custom Values for the Player's Stamina")]
public class PlayerSprint : ScriptableObject
{
    public float StaminaCost; //Increment of how much "currentStamina" will decrease over time
    public float RegainedStaminaAmount; //Increment of how much "currentStamina" will increase over time
    public float MaxStamina;
}

//The purpose of having this scriptable obejct script is so that various values for the player can be saved
//for different variations of the player to avoid needing to change values constantly in the inspector