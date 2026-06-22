using System.Collections;
using Code.Scripts.EventSystems;
using Singleton;
using UnityEngine;

public enum TutorialState
{
    NotStarted,
    IntroDialogue,
    WaitingForBuildings,
    WaitingForFirstDollar,
    CombatDialogue,
    Completed
}

public class TutorialManager : SingletonBase<TutorialManager>
{
    [Header("Dialogue Assets")]
    public DialogueSO introDialogue;
    public DialogueSO combatDialogue;

    [Header("State")]
    public TutorialState currentState = TutorialState.NotStarted;

    private bool hasPlacedMiner = false;
    private bool hasPlacedSeller = false;
    private float initialCurrency = -1;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
    }

    private void Start()
    {
        StartCoroutine(StartTutorialRoutine());
        
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnBuildingPlaced += HandleBuildingPlaced;
        }

        if (CurrencyManager.Instance != null)
        {
            initialCurrency = CurrencyManager.Instance.currentCurrencyValue;
            CurrencyManager.Instance.onCurrencyChange += HandleCurrencyChange;
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
        }
    }

    private void OnDestroy()
    {
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnBuildingPlaced -= HandleBuildingPlaced;
        }
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.onCurrencyChange -= HandleCurrencyChange;
        }
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }
    }

    private IEnumerator StartTutorialRoutine()
    {
        yield return new WaitForSeconds(1f); // Wait a moment for the scene to settle
        
        // Ensure DayNightManager is paused
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.isTutorialActive = true;
        }

        currentState = TutorialState.IntroDialogue;
        PlayDialogue(introDialogue);
    }

    private void PlayDialogue(DialogueSO dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("TutorialManager: DialogueSO is null. Skipping dialogue step.");
            HandleDialogueEnded(); // Skip if not assigned
            return;
        }
        
        var eSet = new DialogueEvent { enabled = true, dialogue = dialogue, index = 0 };
        EventManager.Instance.Publish(eSet);
    }

    private void HandleDialogueEnded()
    {
        if (currentState == TutorialState.IntroDialogue)
        {
            currentState = TutorialState.WaitingForBuildings;
            CheckBuildingsCondition();
        }
        else if (currentState == TutorialState.CombatDialogue)
        {
            CompleteTutorial();
        }
    }

    private void HandleBuildingPlaced(Buildings.BuildingData data)
    {
        if (data.type == Buildings.BuildingType.Miner)
        {
            hasPlacedMiner = true;
        }
        else if (data.type == Buildings.BuildingType.Seller)
        {
            hasPlacedSeller = true;
        }
        
        if (currentState == TutorialState.WaitingForBuildings)
        {
            CheckBuildingsCondition();
        }
    }

    private void CheckBuildingsCondition()
    {
        if (hasPlacedMiner && hasPlacedSeller)
        {
            currentState = TutorialState.WaitingForFirstDollar;
        }
    }

    private void HandleCurrencyChange()
    {
        if (currentState == TutorialState.WaitingForFirstDollar)
        {
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.currentCurrencyValue > initialCurrency)
            {
                currentState = TutorialState.CombatDialogue;
                PlayDialogue(combatDialogue);
            }
        }
    }

    private void CompleteTutorial()
    {
        currentState = TutorialState.Completed;
        
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.CompleteTutorial();
            
            // Force the day to end quickly so the raid starts soon after
            DayNightManager.Instance.timeRemaining = 5f; 
        }
    }
}
