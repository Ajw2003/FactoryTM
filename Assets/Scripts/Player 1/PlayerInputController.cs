using System;
using Code.Scripts.EventSystems;
using Code.Scripts.EventSystems.EventTypes.InputEvents;
using Code.Scripts.Interfaces.EventTypes;
using Code.Scripts.Player.PlayerStateMachine;
using NUnit.Framework.Internal.Execution;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private PlayerInputs _playerInputs;
    private bool _dialogueEnabled;
    private bool _speakingEnabled;

    PlayerStateMachine _playerStateMachine;

    void Awake()
    {
        _playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    void OnEnable()
    {
        if (_playerInputs == null)
            _playerInputs = new PlayerInputs();
        EventManager.Instance?.Subscribe(this, (MovementInputEvent e) => MovementInputs(e.IsEnabled));
        EventManager.Instance?.Subscribe(this,(DialogueInputEvent e) => DialogueInputs(e.IsEnabled));
        EventManager.Instance?.Subscribe(this, (InventoryInputEvent e) => InventoryInputs(e.IsEnabled));
        EventManager.Instance?.Subscribe(this, (SceneChangeEvent e) => OnSceneChange());

        _playerInputs.Enable();
    }

    public void OnSceneChange()
    {
        _playerInputs.Disable();
        // MovementInputs(false);
        // InventoryInputs(false);
        // DialogueInputs(false);
        // CipherInputs(false);
        // TalkToNpcInputs(false);
    }

    private void Start()
    {
        MovementInputs(true);
        InventoryInputs(true);
    }

    void MovementInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            case true:
                _playerInputs.PlayerActions.Move.performed += OnMovePerformed;
                _playerInputs.PlayerActions.Sprint.performed += OnSprintPerformed;
                _playerInputs.PlayerActions.Sprint.canceled += OnSprintCanceled;
                _playerInputs.PlayerActions.Interact.performed += OnInteractPerformed;
                break;
            case false:
                _playerInputs.PlayerActions.Move.performed -= OnMovePerformed;
                _playerInputs.PlayerActions.Sprint.performed -= OnSprintPerformed;
                _playerInputs.PlayerActions.Sprint.canceled -= OnSprintCanceled;
                _playerInputs.PlayerActions.Interact.performed -= OnInteractPerformed;
                break;
        }
    }

    void InventoryInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            case true:
                _playerInputs.PlayerActions.Inventory.performed += OnInventoryPerformed;
                break;
            case false:
                _playerInputs.PlayerActions.Inventory.performed -= OnInventoryPerformed;
                break;
        }
    }

    void EscapeInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            case true:
                _playerInputs.PlayerActions.Escape.performed += Escape;
                break;
            case false:
                _playerInputs.PlayerActions.Escape.performed -= Escape;
                break;
        }
    }

    void DialogueInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            case true:
                _playerInputs.PlayerActions.Skip.performed += SkipDialogue;
                break;
            case false:
                _playerInputs.PlayerActions.Skip.performed -= SkipDialogue;
                break;
        }
    }

    #region ActionHandling
    
// Event Handler Methods

    void OnInventoryPerformed(InputAction.CallbackContext val)
    {
        _playerStateMachine.OpenInventory();
    }
    void OnMovePerformed(InputAction.CallbackContext val)
    {
        _playerStateMachine.Move(val.ReadValue<Vector2>());
    }

    void OnSprintPerformed(InputAction.CallbackContext val)
    {
        _playerStateMachine.Sprint();
    }

    void OnSprintCanceled(InputAction.CallbackContext val)
    {
        _playerStateMachine.NoLongerSprinting();
    }

    void OnInteractPerformed(InputAction.CallbackContext val)
    {
        EventManager.Instance?.Publish(new PlayerInteractEvent());
    }

    void SkipDialogue(InputAction.CallbackContext val)
    {
        TextIndex.Instance.Continue = true;
    }

    void ShowDialogue(InputAction.CallbackContext val)
    {
        EventManager.Instance?.Publish(new NpcDialoguePassThroughEvent());
        EventManager.Instance?.Publish(new PlayerStateOverrideToDialogueEvent());
        TextIndex.Instance?.StartTextVisible();
    }

    void Escape(InputAction.CallbackContext val)
    {
        _playerStateMachine.IncrementEscapeCharge();
    }

    #endregion

    #region FindActions
    
    private InputAction FindAction(string actionName)
    {
        var actionMap = _playerInputs.PlayerActions;

        var property = typeof(PlayerInputs.PlayerActionsActions).GetProperty(actionName);
        if (property != null)
        {
            return property.GetValue(actionMap) as InputAction;
        }

        Debug.LogWarning($"Input action '{actionName}' not found.");
        return null;
    }

    private void HandleInputToggleEvent(InputToggleEvent inputEvent)
    {
        InputAction action = FindAction(inputEvent.ActionName);

        if (action != null)
        {
            if (inputEvent.Enable)
                action.Enable();
            else
                action.Disable();

            //Debug.Log($"Input action '{inputEvent.ActionName}' set to {(inputEvent.Enable ? "Enabled" : "Disabled")}");
        }
        else
        {
            Debug.LogWarning($"Input action '{inputEvent.ActionName}' not found.");
        }
    }

    #endregion
}   