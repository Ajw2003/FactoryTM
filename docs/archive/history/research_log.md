# Research Log: Remaining Fixes (FactoryTM)
**Date:** 2026-07-19
**Sources Cited:** 
- `c:\Users\aj\Desktop\GameDev\Projects\FactoryTM\Assets\Notes\ToDo.txt`
- `c:\Users\aj\Desktop\GameDev\Projects\FactoryTM\docs\decisions\project_summary.md`
- `c:\Users\aj\Desktop\GameDev\Projects\FactoryTM\docs\decisions\project_phases.md`

## 1. Project Context Summary
FactoryTM is a Unity-based factory automation roguelike. The core architecture relies heavily on an Event-Driven system (`EventManager.cs`), State Machines (`BaseStateMachine.cs`), and Singletons. Recent refactors decoupled the UI and updated the tutorial system to use `TutorialManager` hooked into inventory/currency events (Source: `project_summary.md`, `project_phases.md`).

## 2. Remaining Tasks in 'Fixes' Section
Based on `ToDo.txt` (lines 44-52), the following fixes are either in-progress (`[/]`) or pending:

### In-Progress (`[/]`)
1. **Bullet Rendering:** "fix rendering of bullets rendering wrong and dark instead of properly"
   * *Context:* This is likely caused by the URP Day/Night global lighting or `CRTNIGHT.asset` post-processing volume mentioned in `project_summary.md` section 3. Projectiles are managed via `ObjectPoolManager.cs`.
2. **Objectives UI Placement:** "fix placement of the objectives ui"
   * *Context:* Phase 1 of `project_phases.md` mentions hooking `TutorialManager` to dynamically shift the dialogue box position (e.g., `DialoguePositionMode.PlacementTop`) to avoid overlaps with the Store UI or Building configuration UI.
3. **Objectives UI Updating in Menus:** "ensure the objectives ui updates properly when actions are completed even in menus which it seems to ignore until closed"
   * *Context:* The UI state machine might be freezing background event processing or the `TutorialManager` is not properly receiving/processing events (like `IEvent`) when a menu state (e.g., Store UI) is active.

### Pending
4. **Dodge Roll Lock:** "fix dodge roll being obtained before upgrade is unlocked..."
   * *Context:* According to `project_summary.md` (Input Architecture), the `PlayerInputController.cs` parses raw inputs and publishes `PlayerDodgeEvent`. The listener for this event (likely `PlayerController.cs`) needs to check the `UpgradeManager` state before executing the roll.
5. **Dialogue UI Update:** "fix dialogue ui not updating but showing ui like the objectives ui that would suggest it should."
   * *Context:* Tied to Phase 3 goals in `project_phases.md` ("Refactor the tutorial into a story with an AI"). `TutorialManager` needs proper syncing with the objectives list state.
6. **Enemy Outpost Tutorial UI:** "fix destroying enemy outpost section of the tutorial... it should say the %of hp the spawner has left"
   * *Context:* The tutorial currently hardcodes "4 remaining". It needs to listen to an `IEvent` (e.g., `EnemySpawnerDamageEvent`) to dynamically update a percentage text for the objective instead.
7. **UI Spacing Standardization:** "ensure all ui is adequately spaced at all times and apply a standard spacing..."
   * *Context:* Aligns with Phase 2 UI polish (`project_phases.md`). Requires a structural review of layout group components across all bespoke building panels and the core inventory panel.
8. **Inventory Item Value:** "make resources inventory show value of total items in a given slot instead of the individual price..."
   * *Context:* The `Inventory Panel` (`project_summary.md` UI Architecture) currently displays the single item value. The slot rendering logic needs to be updated to compute `item count * single item value`.
