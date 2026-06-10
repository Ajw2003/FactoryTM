using System.Collections.Generic;
using UnityEngine;

//This script excludes blend trees that blend animations; script only for singular animations
//This SO will contain the names of the animations that are to be placed as strings

//June 10
//This SO will be referenced by the PlayerStateMachine script and state specific scripts 
//to choose which animation to play, based one of the strings in the "namesOfSinglePlayerAnimations" list 

//Ex: 
//If in the "namesOfSinglePlayerAnimations" list contains a string called "walking",
//The PlayerStateMachine will trigger the animation called "walking" like so:

/*
public string animationName;
public animator animator;

foreach (string name in namesOfSinglePlayerAnimations)
{
    if (namesOfSinglePlayerAnimations.Contains("walking")) 
    {
        animationName = "walking";
    }
}

animator.SetTrigger(animationName);
*/

[CreateAssetMenu(fileName = "AllPlayerAnimationNames", menuName = "Player Scriptable Objects/Name All Player's Single Animations")]
public class AllPlayerAnimationNames : ScriptableObject
{
    public List<string> namesOfSinglePlayerAnimations = new List<string>();
}

//The purpose of having this scriptable obejct script is so that various values for the player can be saved
//for different variations of the player to avoid needing to change values constantly in the inspector
