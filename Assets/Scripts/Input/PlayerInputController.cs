using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using System;
using Code.Scripts.EventSystems;
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

    // Place/Remove/Rotate/Next/Previous stay bound to their Input System actions but no longer
    // publish events: PlacementManager and HotbarManager poll raw Input directly. Migrating
    // those two off raw Input is a separate change.
    void OnPlacePerformed(InputAction.CallbackContext val)
    {
    }

    void OnRemovePerformed(InputAction.CallbackContext val)
    {
    }

    void OnRotatePerformed(InputAction.CallbackContext val)
    {
    }

    void OnNextPerformed(InputAction.CallbackContext val)
    {
    }

    void OnPreviousPerformed(InputAction.CallbackContext val)
    {
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
}
