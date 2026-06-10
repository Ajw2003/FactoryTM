using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovements", menuName = "Player Scriptable Objects/Create Custom Player Movement Values")]
public class PlayerMovements : ScriptableObject
{
    public float WalkingSpeed;
    public float SprintingSpeed;
    public float CrouchingSpeed;
    public float RotationSpeed; //The higher the number, the faster the rotation
    public float SecondsForFreezingPlayer;
}

//The purpose of having this scriptable obejct script is so that various values for the player can be saved
//for different variations of the player to avoid needing to change values constantly in the inspector