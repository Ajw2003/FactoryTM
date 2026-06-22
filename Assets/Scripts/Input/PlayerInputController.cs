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

    void OnDisable()
    {
        if (_playerInputs != null)
        {
            PermanentInputs(false);
            MovementInputs(false);
            InventoryInputs(false);
            _playerInputs.Disable();
        }
    }

    public void OnSceneChange()
    {
        _playerInputs.Disable();
    }

    private void Start()
    {
        PermanentInputs(true);
        MovementInputs(true);
        InventoryInputs(true);
    }

    void PermanentInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            case true:
                _playerInputs.Player.OpenStore.started += OnOpenStorePerformed;
                break;
            case false:
                _playerInputs.Player.OpenStore.started -= OnOpenStorePerformed;
                break;
        }
    }

    void MovementInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            case true:
                _playerInputs.Player.Move.performed += OnMovePerformed;
                _playerInputs.Player.Move.canceled += OnMovePerformed;
                _playerInputs.Player.Dodge.performed += OnDodgePerformed;
                _playerInputs.Player.Heal.performed += OnHealPerformed;
                _playerInputs.Player.Place.performed += OnPlacePerformed;
                _playerInputs.Player.Remove.performed += OnRemovePerformed;
                _playerInputs.Player.Rotate.performed += OnRotatePerformed;
                _playerInputs.Player.Next.performed += OnNextPerformed;
                _playerInputs.Player.Previous.performed += OnPreviousPerformed;
                break;
            case false:
                _playerInputs.Player.Move.performed -= OnMovePerformed;
                _playerInputs.Player.Move.canceled -= OnMovePerformed;
                _playerInputs.Player.Dodge.performed -= OnDodgePerformed;
                _playerInputs.Player.Heal.performed -= OnHealPerformed;
                _playerInputs.Player.Place.performed -= OnPlacePerformed;
                _playerInputs.Player.Remove.performed -= OnRemovePerformed;
                _playerInputs.Player.Rotate.performed -= OnRotatePerformed;
                _playerInputs.Player.Next.performed -= OnNextPerformed;
                _playerInputs.Player.Previous.performed -= OnPreviousPerformed;
                break;
        }
    }

    void InventoryInputs(bool isEnabled)
    {
        switch (isEnabled)
        {
            // case true:
            //     _playerInputs.Player.Inventory.performed += OnInventoryPerformed;
            //     break;
            // case false:
            //     _playerInputs.Player.Inventory.performed -= OnInventoryPerformed;
            //     break;
        }
    }

    #region ActionHandling

    // Event Handler Methods

    void OnInventoryPerformed(InputAction.CallbackContext val)
    {
        // For now, keep as is or publish event if needed
        // _playerStateMachine.OpenInventory(); 
    }

    void OnOpenStorePerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
        EventManager.Instance?.Publish(new PlayerOpenStoreEvent());
    }
    void OnMovePerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused)
        {
            EventManager.Instance?.Publish(new PlayerMoveEvent(Vector2.zero));
            return;
        }
        EventManager.Instance?.Publish(new PlayerMoveEvent(val.ReadValue<Vector2>()));
    }

    void OnPlacePerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
        EventManager.Instance?.Publish(new PlayerPlaceEvent());
        EventManager.Instance?.Publish(new PlayerInteractEvent()); // Legacy/Fallback
    }

    void OnRemovePerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
        EventManager.Instance?.Publish(new PlayerRemoveEvent());
    }

    void OnRotatePerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
        EventManager.Instance?.Publish(new PlayerRotateEvent());
    }

    void OnNextPerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
        EventManager.Instance?.Publish(new PlayerNextItemEvent());
    }

    void OnPreviousPerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
        EventManager.Instance?.Publish(new PlayerPreviousItemEvent());
    }

    void OnDodgePerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
        EventManager.Instance?.Publish(new PlayerDodgeEvent());
    }

    void OnHealPerformed(InputAction.CallbackContext val)
    {
        if (PauseManager.IsPaused) return;
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