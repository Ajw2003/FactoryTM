using System;
using Code.Scripts.EventSystems;
using Code.Scripts.EventSystems.EventTypes.InputEvents;
using Code.Scripts.Interfaces.EventTypes;
using NUnit.Framework.Internal.Execution;
using StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private InputSystem_Actions _playerInputs;
    private bool _dialogueEnabled;
    private bool _speakingEnabled;

    void OnEnable()
    {
        if (_playerInputs == null)
            _playerInputs = new InputSystem_Actions();
        EventManager.Instance?.Subscribe(this, (MovementInputEvent e) => MovementInputs(e.IsEnabled));
        EventManager.Instance?.Subscribe(this, (InventoryInputEvent e) => InventoryInputs(e.IsEnabled));
        EventManager.Instance?.Subscribe(this, (SceneChangeEvent e) => OnSceneChange());

        _playerInputs.Enable();
    }

    public void OnSceneChange()
    {
        _playerInputs.Disable();
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
                _playerInputs.Player.Move.performed += OnMovePerformed;
                _playerInputs.Player.Move.canceled += OnMovePerformed;
                _playerInputs.Player.OpenStore.performed += OnInteractPerformed;
                _playerInputs.Player.Dodge.performed += OnDodgePerformed;
                _playerInputs.Player.Heal.performed += OnHealPerformed;
                break;
            case false:
                _playerInputs.Player.Move.performed -= OnMovePerformed;
                _playerInputs.Player.Move.canceled -= OnMovePerformed;
                _playerInputs.Player.OpenStore.performed -= OnInteractPerformed;
                _playerInputs.Player.Dodge.performed -= OnDodgePerformed;
                _playerInputs.Player.Heal.performed -= OnHealPerformed;
                break;
        }
    }

    void InventoryInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            case true:
                _playerInputs.Player.OpenStore.performed += OnInventoryPerformed;
                break;
            case false:
                _playerInputs.Player.OpenStore.performed -= OnInventoryPerformed;
                break;
        }
    }

    #region ActionHandling
    
// Event Handler Methods

    void OnInventoryPerformed(InputAction.CallbackContext val)
    {
        // For now, keep as is or publish event if needed
        // _playerStateMachine.OpenInventory(); 
    }
    void OnMovePerformed(InputAction.CallbackContext val)
    {
        EventManager.Instance?.Publish(new PlayerMoveEvent(val.ReadValue<Vector2>()));
    }

    void OnInteractPerformed(InputAction.CallbackContext val)
    {
        EventManager.Instance?.Publish(new PlayerInteractEvent());
    }

    void OnDodgePerformed(InputAction.CallbackContext val)
    {
        EventManager.Instance?.Publish(new PlayerDodgeEvent());
    }

    void OnHealPerformed(InputAction.CallbackContext val)
    {
        EventManager.Instance?.Publish(new PlayerHealEvent());
    }

    #endregion

    #region FindActions
    
    private InputAction FindAction(string actionName)
    {
        var actionMap = _playerInputs.Player;

        var property = typeof(InputSystem_Actions.PlayerActions).GetProperty(actionName);
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