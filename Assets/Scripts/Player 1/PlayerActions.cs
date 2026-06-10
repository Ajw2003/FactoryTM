using UnityEngine;

//This script is to act a parameter type
//This script is an enum. Reason for this is to have vairable types for the "PlayerController" and "PlayerStateMachine" scripts to use. 
//The variable types below are values that can be passed through the methods that have a "PlayerActions" parameter type

//Whenever the "PlayerEvent" triggers an event call by calling upon its method called "NotifyPlayerListeners(PlayerActions)", 
//it'll call upon "PlayerListeners" script's method, OnPlayerNotify(), while passing in the "PlayerActions" parameter

public enum PlayerActions 
{
    Sprint,
    NotSprinting,
    Crouch,
    NotCrouching,
    OpenInventory,
    ExitInventory,
}
