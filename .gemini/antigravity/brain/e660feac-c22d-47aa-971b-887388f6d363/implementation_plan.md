# Code Architecture & Cleanup (Phase 6) Implementation Plan

This plan addresses the technical debt and convention discrepancies identified in the project, ensuring all code aligns with the rules defined in `AGENTS.md`.

## User Review Required

> [!WARNING]
> **Namespace Refactor Impact:** Adding namespaces to dozens of scripts (`Managers`, `Ui`, `Weapons`, etc.) will require updating `using` statements across the entire project. While Unity relies on `.meta` file GUIDs to track scripts, drastically changing namespaces across a live project can occasionally cause "Missing Script" warnings in the inspector or prefabs. I will perform the changes carefully, but you should verify prefabs after this task is complete.

> [!IMPORTANT]
> **PlayerStateMachine Refactor:** The current `PlayerStateMachine` is a standard serializable class instantiated via `new PlayerStateMachine(this)`. To adhere to the rule that all State Machines must inherit from `BaseStateMachine` (which is a `MonoBehaviour`), I will need to change how `PlayerController` attaches it. It will be added as a component at runtime (`gameObject.AddComponent<PlayerStateMachine>()`) instead of being instantiated with `new`.

## Open Questions
- For the event namespaces, `InputToggleEvent.cs` currently uses `namespace Code.Scripts.EventSystems.EventTypes.InputEvents`. Should I standardize everything to just `EventTypes.InputEvents` (and similar sub-namespaces), or prefer the fully qualified `Code.Scripts...`? My plan assumes the shorter `EventTypes.*` as it is cleaner and mentioned in the rules.

## Proposed Changes

### ScriptableObject Placement
Move misplaced ScriptableObject definition files into the correct `Data` folder.

#### [MODIFY] Script Locations
- Move `Assets/Scripts/Nodes/ResourceNodeDefinition.cs` (and its `.meta`) -> `Assets/Scripts/Data/ResourceNodeDefinition.cs`
- Move `Assets/Scripts/Ui/FloatingTextSettings.cs` (and its `.meta`) -> `Assets/Scripts/Data/FloatingTextSettings.cs`
- Move `Assets/Scripts/Weapons/WeaponStats.cs` (and its `.meta`) -> `Assets/Scripts/Data/WeaponStats.cs`

---

### Singletons
Ensure all managers adhere to the Singleton convention.

#### [MODIFY] ZoneUiManager.cs (file:///c:/Users/aj/Desktop/GameDev/Projects/FactoryTM/Assets/Scripts/Managers/ZoneUiManager.cs)
- Change inheritance from `MonoBehaviour` to `SingletonBase<ZoneUiManager>`.
- Add `using Singleton;` to the file.

---

### State Machines
Enforce `BaseStateMachine` inheritance.

#### [MODIFY] PlayerStateMachine.cs (file:///c:/Users/aj/Desktop/GameDev/Projects/FactoryTM/Assets/Scripts/StateMachine/PlayerStateMachine.cs)
- Change class definition to `public class PlayerStateMachine : StateMachine.BaseStateMachine`.
- Replace the constructor `public PlayerStateMachine(PlayerController player)` with an `public void Initialize(PlayerController player)` method.

#### [MODIFY] PlayerController.cs (file:///c:/Users/aj/Desktop/GameDev/Projects/FactoryTM/Assets/Scripts/Player/PlayerController.cs)
- Update instantiation logic to `StateMachine = gameObject.AddComponent<PlayerStateMachine>(); StateMachine.Initialize(this);`.

---

### Event Namespaces
Standardize event definitions.

#### [MODIFY] Various Event Scripts
- Update `AudioClipEvent.cs`, `DialogueEvent.cs`, `FadeTransitionEvent.cs`, `GlobalVolumeEvent.cs`, `InventoryInputEvent.cs`, `MovementInputEvent.cs`, `RespawnPlayerEvent.cs`, `SceneChangeEvent.cs`, `StopAudioEvent.cs`, `TransformPassThroughEvent.cs` to use `namespace EventTypes.[AppropriateCategory]`.
- Fix `InputToggleEvent.cs` to use `namespace EventTypes.InputEvents`.

---

### Global Namespace Overhaul
Wrap globally scoped scripts in their appropriate domain namespaces.

#### [MODIFY] All Scripts in specific domains
- **Managers:** Wrap scripts in `namespace Managers`
- **Ui:** Wrap scripts in `namespace Ui`
- **Weapons:** Wrap scripts in `namespace Weapons`
- **Placeables:** Wrap scripts in `namespace Placeables`
- **Nodes:** Wrap scripts in `namespace Nodes`
- **Iterative Build Fixing:** After applying namespaces, I will iteratively run `dotnet build` and add missing `using Managers;`, `using Ui;`, etc., to all scripts that report compilation errors until the project builds successfully.

## Verification Plan

### Automated Tests
- Run `dotnet build "c:\Users\aj\Desktop\GameDev\Projects\FactoryTM\Assembly-CSharp.csproj"` iteratively until compilation succeeds with zero errors.

### Manual Verification
- The user will be asked to open the Unity Editor, verify that no scripts are missing on the Player or Manager prefabs, and confirm that the game enters Play Mode without errors.
