# Implementation Plan for FactoryTM Fixes

## 1. Bullet Rendering
* **Layman Summary:** Projectiles appear dark and incorrectly rendered, likely due to global lighting or post-processing conflicts.
* **Technical Specs:** Inspect `Assets/Scripts/Weapons/Projectiles/BaseProjectile.cs` and the Projectile prefabs. Ensure the `SpriteRenderer` material is set to a URP-compatible material (e.g., Unlit or properly receiving 2D lights). If the issue is related to `CRTNIGHT.asset` post-processing, check if the projectile requires a different sorting layer or emission setting.
* **Edge Cases:** If bullets use custom shaders, ensure they support Unity 6 URP 2D lighting. 

## 2. Objectives UI Placement
* **Layman Summary:** The Objectives (Tutorial) UI overlaps with other menus like the Store or Building configuration. It needs to dynamically shift out of the way.
* **Technical Specs:** In `Assets/Scripts/Managers/DialogueManager.cs`, utilize `DialoguePositionMode` (e.g., `PlacementTop` or `ShopLeftTop`) when `BuildingUiManager.IsPanelOpen` or `StoreUi` is active. Trigger `DialogueManager.Instance.SetPositionMode()` dynamically when state changes in `PlayerController.StateMachine` (e.g., entering `storeState` or `buildingUiState`).
* **Edge Cases:** Rapidly opening/closing menus might cause the UI to jitter. Add a short lerp or transition buffer if possible.

## 3. Objectives UI Updating in Menus
* **Layman Summary:** The Objectives text does not update while a menu (like the Store) is open until the menu is closed.
* **Technical Specs:** `Assets/Scripts/Managers/TutorialManager.cs` relies on `UpdateObjectiveText()` tied to `InventoryManager` and `CurrencyManager` events. Ensure these events are still firing and being processed when menus are open (e.g., ensure `Time.timeScale` being 0 doesn't pause UI updates for text, or that the `TutorialManager` isn't skipping updates if the Player is in `storeState`).
* **Edge Cases:** Event execution order might be out of sync if the menu UI blocks the event bus.

## 4. Dodge Roll Lock
* **Layman Summary:** The player can dodge roll before they actually purchase the Dodge Roll upgrade in the shop.
* **Technical Specs:** Remove `PlayerController.Instance.canDodgeRoll = true;` from `Assets/Scripts/Managers/TutorialManager.cs` (approx line 290) if it is granting the ability prematurely. Ensure it is only granted in `Assets/Scripts/Managers/UpgradeManager.cs` (approx line 237) when the upgrade is unlocked.
* **Edge Cases:** Ensure removing it from the tutorial doesn't soft-lock a tutorial step that expects the player to dodge roll. If dodging is required for the tutorial, create a temporary override logic in `PlayerController`.

## 5. Dialogue UI Update
* **Layman Summary:** The dialogue UI is static and not reflecting current objective progress, appearing frozen.
* **Technical Specs:** In `Assets/Scripts/Managers/TutorialManager.cs`, ensure that whenever `UpdateObjectiveText()` is called, a corresponding update or event is sent to `DialogueManager.Instance` to refresh the active `DialogueSO` text rendering.
* **Edge Cases:** Updating the text too frequently (e.g., every frame when mining) might cause UI flicker or performance hits. Throttle updates if necessary.

## 6. Enemy Outpost Tutorial UI
* **Layman Summary:** The tutorial objective for destroying the enemy outpost is hardcoded to say "4 remaining" instead of an actual percentage of the spawner's HP.
* **Technical Specs:** Update `Assets/Scripts/Managers/TutorialManager.cs` to subscribe to a health change event from `EnemySpawnerLogic.cs`. Calculate the percentage `(currentHP / maxHP) * 100` and format the objective text as `$"Destroy the Outpost ({percent}% remaining)"`.
* **Edge Cases:** If there are multiple spawners, aggregate their health. Ensure the event un-subscribes properly upon destruction.

## 7. UI Spacing Standardization
* **Layman Summary:** Apply a consistent visual spacing format across all UI panels to ensure it looks organized and professional.
* **Technical Specs:** Perform a pass on `HorizontalLayoutGroup` and `VerticalLayoutGroup` components across Prefabs in `Assets/Prefabs/UI`. Standardize the `Spacing` parameter (e.g., set all to 10 or 15 pixels consistently) and `Padding`.
* **Edge Cases:** Some bespoke UI panels (like hotbar or specific building configurations) might break if a uniform spacing is strictly applied. Allow exceptions where functional.

## 8. Inventory Item Value
* **Layman Summary:** The inventory slots display the price of a single item instead of the total value of all items in that slot.
* **Technical Specs:** In the UI script responsible for rendering inventory slots (likely connected to `InventoryManager` or `BuildingUiManager`), update the text assignment logic: `totalValue = slot.itemCount * itemConfig.individualPrice`.
* **Edge Cases:** Very large stacks could result in values that overflow the UI text box. Format the number (e.g., "1.2k") if necessary.
