# Player, input and state machines

## What it owns

The player character and everything that decides what the player may do right now: raw input
(`PlayerInputController`), the player's stats and actions (`PlayerController`), and the state machines that gate
input (`BaseStateMachine`, `PlayerStateMachine` and the six player states). The tutorial is also built on
`BaseStateMachine`, but it has its own doc: [tutorial.md](tutorial.md).

## How it works

- **Input path.** The Input System fires callbacks in `PlayerInputController`, which publishes events
  (`PlayerMoveEvent`, `PlayerDodgeEvent`, `PlayerHealEvent`, `PlayerOpenStoreEvent`) and ignores input while
  `PauseManager.IsPaused` ([PlayerInputController.cs:116-161](../../Assets/Scripts/Input/PlayerInputController.cs)).
  `PlayerController` subscribes in `Start`
  ([PlayerController.cs:114-117](../../Assets/Scripts/Player/PlayerController.cs)).
- **Gating input.** The input controller itself listens for `MovementInputEvent` and `InventoryInputEvent`
  (enable/disable flags). Each player state publishes the pair it wants in `Enter()`: Idle enables both,
  Dodge and Dead disable both, Store and BuildingUi disable movement but keep inventory input
  ([PlayerIdleState.cs:23-24](../../Assets/Scripts/StateMachine/PlayerIdleState.cs),
  [PlayerBuildingUiState.cs:23-24](../../Assets/Scripts/StateMachine/PlayerBuildingUiState.cs)).
- **State machine.** `BaseStateMachine.ChangeState` ignores a change to the current state, assigns
  `CurrentState` **first**, then calls `Exit()` on the old state and `Enter()` on the new one
  ([BaseStateMachine.cs:17-28](../../Assets/Scripts/StateMachine/BaseStateMachine.cs)).
  `PlayerStateMachine` adds a `stateChanged` event and builds the six state objects in `Initialize(player)`
  ([PlayerStateMachine.cs:27-36](../../Assets/Scripts/StateMachine/PlayerStateMachine.cs)).
- **Stats and mutation.** The player's tunable stats are `[SerializeField] private` fields with read-only
  properties (`MaxHealth`, `AmmoReserve`, `CanDodgeRoll`…). They change only through methods:
  `ApplyArmorUpgrade`, `GrantHealthPacks`, `AddAmmoReserve`, `ConsumeAmmoReserve`, `UnlockDodgeRoll`,
  `ApplyStaminaBoost` ([PlayerController.cs:131-165](../../Assets/Scripts/Player/PlayerController.cs)).
  `Health` is `{ get; private set; }`, seeded in `Awake`, changed by `TakeDamage` / `UseHealthPack`, each of
  which publishes `PlayerHealthChangedEvent`.
- **Modes.** `PlayerMode` is `Combat` or `Building`; toggling publishes `PlayerModeChangedEvent`.
- **Death.** `Die()` switches to `deadState` and calls `UiManager.ShowGameOver()`
  ([PlayerController.cs:545-549](../../Assets/Scripts/Player/PlayerController.cs)).

## Invariants

1. **Serialized stat fields keep their exact field names.** `Player.prefab` stores `canDodgeRoll: 1` and
   `ammoReserve: 120`, which differ from the code defaults (`false`, `90`). Renaming a field, or turning it
   into an auto-property, silently resets those values (decided 2026-07-25).
2. **Change player stats through the methods above**, so the matching event fires. A direct field write
   leaves the HUD stale.
3. **Dodge roll is granted only by the upgrade path** (`UnlockDodgeRoll`, called from `UpgradeManager`), never by
   the tutorial.
4. **Do not reorder `ChangeState`.** Assigning `CurrentState` before `Exit()` is what stops an `Exit()` that
   re-enters `ChangeState` from recursing.
5. **A state that disables input in `Enter()` must be left by a state that re-enables it**; nothing else
   re-enables movement.

## Traps

- **The stack overflow of 2026-07-20.** `DefendFirstRaidTutorialState.Exit()` called back into
  `ChangeState`, which still saw the old state as current and called `Exit()` again forever. Fixed by the
  ordering in invariant 4. Same shape can recur in any state whose `Exit()` triggers a transition.
- **`PlacementManager` and `HotbarManager` still poll the old `UnityEngine.Input`** instead of using the
  Input System actions. The five input handlers in `PlayerInputController` that used to publish
  place/remove/rotate/next/previous events were deleted as dead code; the bindings remain but nothing
  reads them ([PlacementManager.cs:122](../../Assets/Scripts/Managers/PlacementManager.cs)). House
  standard is the Input System only, so this is an open cleanup (see
  [ProjectState](../3-state/ProjectState.md)).
- **`PlayerController.OnPlayerDodge` is a plain C# event**, not an `IEvent`; its one subscriber is the
  tutorial. `PlayerModeChangedEvent` has no subscriber yet.
- **Singleton quirk.** `PlayerController` does not use `SingletonBase`; its `Awake` destroys a duplicate
  but then still falls through and creates a state machine on the doomed object
  ([PlayerController.cs:66-82](../../Assets/Scripts/Player/PlayerController.cs)). Harmless while one exists.
- The player spawns two cells north of the IDT's centre cell so the two do not overlap
  ([PlayerController.cs:96-100](../../Assets/Scripts/Player/PlayerController.cs)); a different starting
  building layout needs that offset revisited.
