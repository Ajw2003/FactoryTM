# Session 2: Day/Night, Enemy ScriptableObjects, & Building Damage Cracks

This document records the design decisions and changes made during Session 2 (approx. 8 days ago in commit history).

---

## 1. Context & Motivation
To ground the visual aesthetic and structure of the game, several systems required refactoring:
1. The **Day/Night Cycle** was purely textual and did not affect the scene lighting.
2. **Enemy Buildings** were spawned via ad-hoc runtime code, making balance and level-design difficult to control.
3. **Combat Visuals** lacked immediate feedback regarding structural damage on player and enemy structures.

---

## 2. Key Decisions & Implementation Details

### Day/Night Sun Adjustments
* **Dynamic Directional Light:** Updated `DayNightManager.cs` to rotate and adjust the color intensity of the scene's main directional light (the "sun") based on the active phase (Dawn, Day, Dusk, Night).
* **Night Vision Effect:** Integrated a global volume post-processing change to clear the screen curve effect and toggle a night-vision filter during the night phase.

### Enemy Building Configuration Refactoring
* **Transition to ScriptableObjects:** Deprecated runtime-generated values for enemy outposts.
* **Bespoke ScriptableObject Definitions:** Enemy structures (turrets, spawners) now use the same `BuildingData` format as player structures, allowing designers to configure attack range, fire rate, health, and custom projectile archetypes directly in the Inspector.

### Procedural Building Damage Cracks
* **Visual Combat Feedback:** Added procedural cracking overlays to building sprites.
* **Scale-Based Cracking:** As a building’s health drops below thresholds (e.g., 75%, 50%, 25%), damage cracks become progressively more visible, giving the player instant feedback on building status without cluttering the screen with health bars.

---

## 3. Associated Commits
* `6bff3c1` - Add ability for DayNightManager to adjust the sun light with respect to the current phase.
* `45c3ab8` - Change enemy building code to use ScriptableObjects instead of runtime generation.
* `36a4004` - Make enemy turret settings adjustable via Turret ScriptableObjects.
* `be5cf7d` - Procedural damage cracking on buildings.
* `42a214e` - Fix store UI and remove unused upgrades.
* `e2bdb9f` - Fix monogram asset and DayNight manager Canvas lookup.
