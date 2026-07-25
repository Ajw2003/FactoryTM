# FactoryTM To-Do List Analysis & Execution Plan

I have analyzed your `ToDo.txt` and broken down the tasks into a logical execution order based on game development best practices. We will start with critical bug fixes and foundational refactors, move on to core mechanics and UI polish, expand the narrative/tutorial systems, and finally tackle game balance and advanced features.

Here is the breakdown of how I would approach each task and the recommended order of completion.

## Phase 1: Critical Bug Fixes & Refactors
*These tasks fix broken logic or simplify systems that other features rely on. They should be done first to ensure a stable foundation.*

1. **Fix tile placement with items / conveyors moving items to empty spaces**
   - **How**: We need to update the `Conveyor` script's item movement logic to check if the target grid space has a valid receiver (another conveyor or an input inventory) before passing the item. For building placement, update `PlayerBuildingUiState` to detect loose items on the target grid and either block placement or automatically destroy/refund the items.
2. **Deprecate the seller building**
   - **How**: We will remove the seller building from the `BuildingManager`'s purchasable list and the `UpgradeManager` pools. Instead of deleting the code immediately, we can mark the class as `[Obsolete]` and ensure the InterDimensional Transport (IDT) is fully capable of handling all resource exporting and money generation.
3. **Make objectives and current dialogue scale to fit inside the console window**
   - **How**: This requires adjusting the UI Canvas. We will use Unity's `ContentSizeFitter` and `VerticalLayoutGroup` on the console window, or we can have the `StoreManager` publish an event when opened that the Tutorial UI listens to, scaling down its bounds dynamically.
4. **Fix objectives not being large enough / copy-pastes**
   - **How**: Update the Tutorial `ScriptableObject` data structure to include a `ShortObjectiveText` string (e.g., "Build 3 Miners"). We will update the HUD to display this short string instead of the full dialogue text.

## Phase 2: Core Mechanics & UI Polish
*With bugs fixed, we can implement the missing core mechanics and improve the visual experience.*

1. **Fully implement finite resources**
   - **How**: Add a `currentYield` integer to the resource node script. Each time a miner extracts an ore, decrement this value. When it reaches zero, destroy or disable the node and publish an `OreDepletedEvent`. The Jade HUD (`BuildingUiManager`) will listen to node interactions and display `currentYield / maxYield`.
2. **Minecraft-style hotbar UI**
   - **How**: Update the inventory UI script. When the `PlayerInputController` fires a scroll event, update the highlighted slot. We will use the `ObjectPoolManager` to spawn a floating text UI element (or a dedicated static text element above the hotbar) displaying the active item's name, which fades out after 2 seconds.
3. **Make all text larger and ignore blur/post-process effects**
   - **How**: Increase the base font size for the `Pixelify Sans` and `Thaleah` assets. To bypass the CRT post-processing, we will render the UI Canvas on a separate Overlay Camera that does not have the post-processing volume applied.
4. **Improve UI contrast, color choices, and general style unification**
   - **How**: Establish a unified color palette in the `index.css` (or Unity equivalent UI styling). We will use dark, opaque backgrounds with bright, high-contrast borders for UI panels so they pop against the dark game world and CRT effects.

## Phase 3: Tutorial & Narrative System
*Now that the core game plays well, we can guide the player through it with the narrative AI.*

1. **Refactor the tutorial into a story with an AI**
   - **How**: Update `TutorialManager.cs` to use the `BaseStateMachine`. Each state will represent a tutorial phase that triggers AI dialogue. We will update `Story.txt` and create new `ScriptableObjects` for the AI's lines.
2. **Make objectives an itemized list that fires events**
   - **How**: Change the objective data structure to contain a list of sub-objectives, each linked to a specific `IEvent` type (e.g., `BuildingPlacedEvent`). The `TutorialManager` will subscribe to the `EventManager` and mark sub-objectives complete when the corresponding events are fired.
3. **Make completing all objectives trigger actions**
   - **How**: Once all sub-objectives in a tutorial state are met, the `TutorialManager` will publish a `TutorialStateCompleteEvent`. Other systems (like the Store UI) can listen for this to automatically close or unlock new items.
4. **Add tutorial sections (IDT, land expansion, purchasing ammo/health, shooting)**
   - **How**: Add these as new states in the `TutorialManager`. For dynamic ones like ammo/health, the manager will listen for `PlayerHealthChangedEvent` or `AmmoChangedEvent` and trigger the tutorial dialogue if the values drop below a certain threshold.

## Phase 4: Game Balance & Store Tuning
*With all features in place, we can tune the economy and progression loop.*

1. **Starting store menu logic (weapons/consumables only)**
   - **How**: Add a `hasChosenStartingWeapon` boolean flag to the `StoreManager` or player save data. When false, the store UI only populates the weapons and consumables tabs. Purchasing a weapon fires an event that sets this flag to true and refreshes the store to its standard state.
2. **Make essential upgrades appear first**
   - **How**: Add a priority tier or "guaranteed early" boolean to the upgrade `ScriptableObjects`. The `UpgradeManager` will be modified to ensure these essential upgrades are forced into the 3 random choices until they are purchased.
3. **Lower the curve for enemy spawn rates in the beginning**
   - **How**: Adjust the `AnimationCurve` or mathematical formula in the `WaveManager`/`RaidManager` to have a much flatter start, giving the player more breathing room in the first few days before scaling aggressively.
4. **Add logic to change length of days dynamically**
   - **How**: In the `DayNightManager`, default the day timer to 90 seconds. At the start of a new day, calculate the next day's length using a formula that factors in `daysSurvived` and `moneyEarned` (e.g., `BaseTime - (Days * X) + (Money * Y)`), clamping it to a minimum and maximum duration.
5. **Balance prices, yields, spawns, and damage**
   - **How**: Data entry. Adjust the values on the various `ScriptableObjects` (Enemies, Buildings, Upgrades) and playtest repeatedly to find the right economic flow.

## Phase 5: Advanced Features & Meta
*The final polish steps to add replayability and depth.*

1. **Implement enemy wandering and differentiation**
   - **How**: Create a generic `EnemyStateMachine`. Derive two distinct behaviors: one for aggressive Raiders that pathfind directly to the player/IDT, and one for Wanderers that pick random points around the map, only attacking if provoked or if a building blocks their path.
2. **Add difficulty settings**
   - **How**: Create a `DifficultyManager` singleton with a `ScriptableObject` defining multipliers for enemy health, damage, and spawn rates. Apply these multipliers globally when enemies are instantiated.
3. **Add achievements**
   - **How**: Create an `AchievementManager` that subscribes to various events (`EnemiesKilledEvent`, `TotalMoneyEarnedEvent`, etc.). When specific thresholds are met, it will unlock the achievement, save it to persistent data, and spawn a UI notification toast.
