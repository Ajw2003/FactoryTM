using UnityEngine;

//This script excludes blend trees that blend animations; script only for singular animations

[CreateAssetMenu(fileName = "PlayerMovementChangesBetweenAnimations", menuName = "Player Scriptable Objects/Set Movement Changes Between Animations")]
public class PlayerMovementChangesBetweenAnimations : ScriptableObject
{
    //These float values should be in time with how fast the animations happen between each other
    public float DurationOfSpeedChanging; //Time span to change from walking speed to sprinting speed. 
    public float SpeedOfMovementChanging; //How fast to change from walking speed to sprinting speed
    public float TimeBetweenCrouchAndStandingAnimations; //Time to wait after standing/crouching animations and/or blend tree completes to adjust movement speed
}
