# Session 4: Object Pooling, Tutorial States, & Fuel/Ammo Limits

This document records the design decisions and changes made during Session 4 (approx. 6 days ago in commit history, corresponding to conversation `e660feac-c22d-47aa-971b-887388f6d363`).

---

## 1. Context & Motivation
As the player automated more of the factory, spawning hundreds of items on conveyors, shooting projectiles, and popping up floating damage text led to garbage collection spikes. The game needed high-performance object reuse. At the same time, the tutorial needed to transition from a linear string-comparison sequence to a robust state-machine-driven tutorial. Finally, automation buildings needed operational costs (fuel for miners/smelters, ammo for turrets) to prevent infinite passive resource generation.

---

## 2. Key Decisions & Implementation Details

### Central Object Pooling (`ObjectPoolManager.cs`)
* **Reusable Buffers:** Created a generic pooling system for conveyors, items, projectiles, and floating text.
* **Pre-allocations:** Instantiates and hides pooling targets at start, reducing frame-rate hitching during gameplay.
* **Floating Text Migration:** Migrated all floating damage numbers and pop-up notifications to utilize the new pooled system.

### Tutorial State Machine (`TutorialManager.cs` Refactor)
* **State Transition Control:** Converted the linear dialogue-driven tutorial flow into a dedicated state machine layout.
* **Bespoke Tutorial States:** Created custom states that listen for specific `IEvent` triggers (like placing a Miner, smelting a bar, or earning $50) before transitioning, preventing sequence breaking.

### Fuel & Ammo Mechanics
* **Miner & Furnace Fuel Limits:** Updated `MinerLogic` and `Furnace` to consume fuel (like coal) at a constant rate during operation. If fuel hits zero, extraction/smelting ceases.
* **Turret Ammo Requirements:** Added ammo capacity to `TurretLogic`. Turrets now consume ammo from their inventory on fire and cease firing when empty.

### Player Building UI State
* **State Class Integration:** Created `PlayerBuildingUiState.cs` and registered it in `PlayerStateMachine.cs` to prevent player movement and other combat inputs from firing while navigating building configurations.

---

## 3. Associated Commits
* `0872918` - Make tutorial manager use states instead of enums for better control.
* `7df4ab2` - Initial implementation of object pooling for projectiles, floating text, items, and game objects.
* `a4ab777` - Migrate all floating text to new system using singleton spawning and ScriptableObjects.
* `c201d11` - Bad optimization fixes for 4 systems (physics, queries, etc.).
* `f8010b2` - Make tutorial use dialogue manager.
* `4d31e35` - Deprecate zone ui code as new UI system is implemented.
