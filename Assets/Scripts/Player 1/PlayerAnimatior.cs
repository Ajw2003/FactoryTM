using UnityEngine;

//This script will controll all the animations / blend trees that will be played for the player 
//I made this script so that the "PlayerController" script wouldn't look so busy and unreadable with
//A bunch of code that does different things. I decided to section off functionality into their own scripts
//With this, these scripts are going to be attached to the same player object, so I can use
//"GetComponent<>" to avoid referencing in the Inspector

//All methods are public as they'll each be called by the "PlayerController"

public class PlayerAnimatior : MonoBehaviour
{
    [Header ("Scriptable Object")]
    [SerializeField]
    private AllPlayerAnimationNames allAnimationNames; //Excludes blend trees that blend animations

    public string AnimationNameToPlay; //Influenced by values from "allAnimationNames", called by "PlayerController"

    private Animator _playerAnimator;
    private string _previousAnimationName;

    void Awake()
    {
        _playerAnimator = GetComponent<Animator>();
    }

    public void PlayIdleAnimation() 
    {
        foreach (string animationName in allAnimationNames.namesOfSinglePlayerAnimations)
        {
            if (allAnimationNames.namesOfSinglePlayerAnimations.Contains("Idle")) 
            {
                AnimationNameToPlay = "Idle"; 
                _playerAnimator.Play(AnimationNameToPlay);
                break;
            }  
        } 
    }

    public void PlayWalkAnimation() 
    {
        foreach (string animationName in allAnimationNames.namesOfSinglePlayerAnimations)
        {
            if (allAnimationNames.namesOfSinglePlayerAnimations.Contains("Walk")) 
            {
                AnimationNameToPlay = "Walk"; 
                _playerAnimator.Play(AnimationNameToPlay);
                break;
            }
        }
    }

    public void PlayRunAnimation()
    {
        foreach (string animationName in allAnimationNames.namesOfSinglePlayerAnimations)
        {
            if (allAnimationNames.namesOfSinglePlayerAnimations.Contains("Run")) //Name of animation that does the sprinting animation
            {
                AnimationNameToPlay = "Run"; //Name of animation that does the sprinting animation
                 _playerAnimator.Play(AnimationNameToPlay);
                break;
            }
        } 
    }

    public void PlayCrouchAnimation()
    {
        foreach (string animationName in allAnimationNames.namesOfSinglePlayerAnimations)
        {
            if (allAnimationNames.namesOfSinglePlayerAnimations.Contains("Crouching"))
            {
                AnimationNameToPlay = "Crouching";
                _playerAnimator.Play(AnimationNameToPlay);
                break;
            }
        } 
    }

    public void PlayCrouchWalkAnimation() 
    {
        foreach (string animationName in allAnimationNames.namesOfSinglePlayerAnimations)
        {
            if (allAnimationNames.namesOfSinglePlayerAnimations.Contains("CrouchWalk"))
            {
                AnimationNameToPlay = "CrouchWalk";
                _playerAnimator.Play(AnimationNameToPlay);
                break;
            }
        } 
    }

    public void PlayAnimation()
    {
        if (string.IsNullOrEmpty(AnimationNameToPlay) || AnimationNameToPlay == _previousAnimationName)
            return;

        // Reset the previous trigger
        if (!string.IsNullOrEmpty(_previousAnimationName))
            _playerAnimator.ResetTrigger(_previousAnimationName);

        // Set the new animation trigger
        _playerAnimator.SetTrigger(AnimationNameToPlay);
        _previousAnimationName = AnimationNameToPlay;
    }

    public void PlayBlendTree()
    {

    }
}
