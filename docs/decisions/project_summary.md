# Project Summary: FactoryTM

FactoryTM is a Unity-based factory automation game where the player controls an alien who crashlands on Earth. The primary objective is to extract resources from the planet, defend the central InterDimensional Transport (IDT), fight off human combatants, and purchase upgrades to scale operations.

---

## 1. Core Gameplay & Roguelike Loop

FactoryTM blends factory automation with a challenging roguelike progression structure:

1. **Extract Resources:** Mine coal, iron, copper, etc., manually or automate the process using Miners and Conveyors.
2. **Export & Earn:** Route resources into the central InterDimensional Transport (IDT) to export them back home and earn money.
3. **Defend against Raids:** Survive periodic, scaling nightly raids from human combatants trying to destroy your operation.
4. **Post-Raid Upgrade Selection:** 
   - At the end of each evening raid, the `UpgradeManager` triggers a choice of **3 random research upgrades** (stat boosts, new weapon profiles, building tiers, or blueprints).
5. **Shop Storefront:** 
   - **One-time Use/Permanent Upgrades:** Unlocked weapons (e.g., swapping combat arsenals) or building tier-ups (which increase efficiency).
   - **Repurchasable Consumables:** Consumable resources (like ammo and health packs) or standard building items (conveyors, miners) can be bought repeatedly.
6. **Permadeath / Game Over:** 
   - The game features permanent failure. If the player's health drops to zero OR if the IDT (InterDimensional Transport) is destroyed, the game immediately ends via `ShowGameOver()`, forcing a complete restart of the run.

---

## 2. Key Architecture & Frameworks

### Pub/Sub Event System (`EventManager.cs`)
* Fully decouples gameplay, UI, and tutorial systems.
* Components publish `IEvent` payloads which are dispatched to weak-referenced listeners.
* Prevents memory leaks and rigid dependencies.

### Input Architecture
* Built on Unity's new **Input System** and managed by `PlayerInputController.cs`.
* Raw hardware inputs (Move, Dodge, Heal, Place, Rotate, Remove, Scroll) are parsed and immediately published as `IEvent` payloads (e.g., `PlayerMoveEvent`, `PlayerDodgeEvent`) via the `EventManager`.
* Decoupled scripts (like `PlayerController`) subscribe to these events.
* State changes (like opening the Store UI or entering dialogue) can publish toggle events to enable/disable specific input actions.

### UI Architecture
* **Jade HUD (`BuildingUiManager.cs`):** A contextual hover overlay that displays dynamic info, remaining ore counts, and placement cues when hovering over nodes, buildings, or conveyor items.
* **Bespoke Building Panels:** Walking up to a machine and pressing `E` displays a custom, context-sensitive overlay (e.g., fuel bars and smelting queues for Furnaces, fuel timers for the IDT, and ammo levels for Turrets).
* **Inventory Panel:** A dynamic slot grid that displays current player inventory items, supporting drag-and-drop actions to feed fuel or sell raw ores.

### Base State Machine (`BaseStateMachine.cs` & `IState.cs`)
* Used for clean flow control on characters (idle, move, dodge, combat states) and UI systems.
* Promotes modular, self-contained state code instead of massive `switch` blocks.
* Includes `PlayerBuildingUiState.cs` to freeze player movements while operating machines.

### Grid Architecture (`GridManager.cs`)
* The game world utilizes a continuous grid system instead of locked zones.
* Based on an adjustable chunk size (default `30x15` tiles) which maintains a fixed aspect ratio suitable for screen sizes.
* Calculations for distances, spawns, and enemy outposts use absolute radius checks and dynamic tile scaling based on the base grid configuration.

### Generic Singletons (`SingletonBase.cs`)
* Managers inherit from `SingletonBase<T>` to guarantee a single lifecycle, with custom control over cross-scene persistence (`persistBetweenScenes`).

### Object Pooling (`ObjectPoolManager.cs`)
* Highly optimized recycling system for frequent game objects: conveyors, projectiles, items, and floating text.
* Eliminates runtime GC allocation spikes.

---

## 3. Aesthetics & Visual Identity

FactoryTM uses a cohesive, high-contrast retro aesthetic to draw the player in:

* **CRT Filter & Post-Processing:** Uses bespoke Universal Render Pipeline (URP) volume assets (`CRT_MainMenu.asset`, `CRTNIGHT.asset`) to apply classic arcade CRT scanlines and screen aberrations.
* **Day/Night Sun Lighting:** Directional light rotations and shadow intensities transition smoothly between active phases. During the night, a high-contrast dark post-process volume activates, which can be mitigated with night vision effects.
* **Damage feedback:** Procedural pixel-art cracking overlays render on building sprites as their health drops below 75%, 50%, and 25%.
* **Pixel Typography:** Utilizes retro typography like `Pixelify Sans` and `Thaleah PixelFont` to reinforce the nostalgic arcade sci-fi visual language.

---

## 4. Directory Layout
* `Assets/Scripts/Managers/` - Singletons managing game loop, UI, audio, raids, day-night cycle, upgrades, and object pools.
* `Assets/Scripts/StateMachine/` - Concrete player states and state machine logic.
* `Assets/Scripts/EventTypes/` - Event payload definitions (e.g., player movement, inventory items, audio actions).
* `Assets/Scripts/Data/` - ScriptableObject configurations for dialogue, upgrades, buildings, and audio.
* `Assets/ScriptableObjects/` - Assets instanced from ScriptableObject definitions.
* `Assets/Notes/` - Design documents, story concepts, and text backlogs.

---

## 5. Current Backlog & Next Steps (From `ToDo.txt`)
* **Difficulty & Scaling:** Adjust prices, ore yields, sale values, enemy spawns, and day lengths dynamically based on money earned or days survived.
* **UI/UX Contrast:** Improve color consistency and text sizing; implement Minecraft-style hotbar item name pop-ups on scroll.
* **Raid Balancing:** Smooth the difficulty curve for early waves; ensure enemy outposts don't spawn in starting zones.
* **Finite Resources:** Fully deplete resource nodes once exhausted and update the HUD indicators.
* **Tutorial Storytelling:** Integrate a guidance AI into the new State-based TutorialManager.
