# FactoryTM Task Phases & Execution Plan

This document contains the categorized breakdown of the To-Do list for FactoryTM. As tasks are completed, this document should be updated to reflect progress.

## Phase 1: Critical Bug Fixes & Refactors
*These tasks fix broken logic or simplify systems that other features rely on. They should be done first to ensure a stable foundation.*

- [ ] **Fix tile placement with items / conveyors moving items to empty spaces**
  - *How*: Update `Conveyor.cs` item movement logic to check for a valid receiver. Update `PlayerBuildingUiState` to detect loose items on the target grid and block placement or destroy/refund them.
- [ ] **Deprecate the seller building**
  - *How*: Remove from `BuildingManager` purchasable list and `UpgradeManager`. Mark class as `[Obsolete]` and ensure IDT handles all exporting.
- [ ] **Make objectives and current dialogue scale to fit inside the console window**
  - *How*: Adjust UI Canvas using `ContentSizeFitter` and `VerticalLayoutGroup`, or dynamically resize bounds via `EventManager` when Store UI opens.
- [ ] **Fix objectives not being large enough / copy-pastes**
  - *How*: Update Tutorial `ScriptableObject` data with a `ShortObjectiveText` string. Update the HUD to display this short string instead of full dialogue.

## Phase 2: Core Mechanics & UI Polish
*With bugs fixed, implement missing core mechanics and improve the visual experience.*

- [ ] **Fully implement finite resources**
  - *How*: Add `currentYield` to resource nodes. Decrement on extraction. When zero, disable node and publish `OreDepletedEvent`. Jade HUD listens and updates display.
- [ ] **Minecraft-style hotbar UI**
  - *How*: Update inventory UI on scroll event. Spawn a temporary floating text UI element showing the active item's name.
- [ ] **Make all text larger and ignore blur/post-process effects**
  - *How*: Increase base font size. Render the UI Canvas on a separate Overlay Camera without the post-processing volume.
- [ ] **Improve UI contrast, color choices, and general style unification**
  - *How*: Establish unified color palette. Use dark, opaque backgrounds with bright, high-contrast borders for UI panels.

## Phase 3: Tutorial & Narrative System
*Guide the player through the game with the narrative AI.*

- [ ] **Refactor the tutorial into a story with an AI**
  - *How*: Update `TutorialManager.cs` to use `BaseStateMachine`. Trigger AI dialogue per state. Update `Story.txt` and `ScriptableObjects`.
- [ ] **Make objectives an itemized list that fires events**
  - *How*: Change objective data to a list of sub-objectives linked to `IEvent` types. `TutorialManager` subscribes and marks them complete on event fire.
- [ ] **Make completing all objectives trigger actions**
  - *How*: On completing all sub-objectives in a state, `TutorialManager` publishes `TutorialStateCompleteEvent` to trigger actions (like closing Store UI).
- [ ] **Add tutorial sections (IDT, land expansion, purchasing ammo/health, shooting)**
  - *How*: Add as new states in `TutorialManager`. Trigger ammo/health dynamically based on low player health/ammo events.

## Phase 4: Game Balance & Store Tuning
*Tune the economy and progression loop.*

- [ ] **Starting store menu logic (weapons/consumables only)**
  - *How*: Add `hasChosenStartingWeapon` flag. When false, show only weapons/consumables. On purchase, fire event to set true and refresh store.
- [ ] **Make essential upgrades appear first**
  - *How*: Add priority tier to upgrade `ScriptableObjects`. `UpgradeManager` forces these into the 3 random choices until purchased.
- [ ] **Lower the curve for enemy spawn rates in the beginning**
  - *How*: Adjust `AnimationCurve` in `WaveManager`/`RaidManager` for a flatter start.
- [ ] **Add logic to change length of days dynamically**
  - *How*: In `DayNightManager`, calculate day length based on `daysSurvived` and `moneyEarned`.
- [ ] **Balance prices, yields, spawns, and damage**
  - *How*: Adjust values on `ScriptableObjects` and playtest.

## Phase 5: Advanced Features & Meta
*Final polish steps for replayability and depth.*

- [ ] **Implement enemy wandering and differentiation**
  - *How*: Create `EnemyStateMachine` with two behaviors: Raiders (aggressive pathfinding) and Wanderers (random movement, defensive).
- [ ] **Add difficulty settings**
  - *How*: Create `DifficultyManager` singleton with multipliers for enemy health, damage, and spawn rates.
- [ ] **Add achievements**
  - *How*: Create `AchievementManager` listening to thresholds (e.g., `EnemiesKilledEvent`). Trigger UI toast and save.
