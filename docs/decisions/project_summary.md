# Project Summary: FactoryTM

FactoryTM is a Unity-based factory automation game where the player controls an alien who crashlands on Earth. The primary objective is to extract resources from the planet, defend the central InterDimensional Transport (IDT), fight off human combatants, and purchase upgrades to scale operations.

---

## 1. Core Gameplay Loop
1. **Extract Resources:** Mine coal, iron, copper, etc., manually or automate the process using Miners and Conveyors.
2. **Export & Earn:** Route resources into the central InterDimensional Transport (IDT) to export them back home and earn money.
3. **Upgrade & Arm:** Purchase weapons, ammo, health packs, and automated structures (Miners, Conveyors, Turrets, Smelters) from the shop.
4. **Defend & Expand:** Defend the IDT against periodic nightly raids by human forces, and buy zone expansions to access richer resource nodes.

---

## 2. Key Architecture & Frameworks

### Pub/Sub Event System (`EventManager.cs`)
* Fully decouples gameplay, UI, and tutorial systems.
* Components publish `IEvent` payloads which are dispatched to weak-referenced listeners.
* Prevents memory leaks and rigid dependencies.

### Base State Machine (`BaseStateMachine.cs` & `IState.cs`)
* Used for clean flow control on characters (idle, move, dodge, combat states) and UI systems.
* Promotes modular, self-contained state code instead of massive `switch` blocks.

### Generic Singletons (`SingletonBase.cs`)
* Managers inherit from `SingletonBase<T>` to guarantee a single lifecycle, with custom control over cross-scene persistence (`persistBetweenScenes`).

### Object Pooling (`ObjectPoolManager.cs`)
* Highly optimized recycling system for frequent game objects: conveyors, projectiles, items, and floating text.
* Eliminates runtime GC allocation spikes.

### Building & UI Manager (`BuildingUiManager.cs`)
* Central controller handling individual, bespoke UI interactions for various placable buildings (e.g., Miners requiring fuel, Smelters processing ores, Turrets requiring ammo).

---

## 3. Directory Layout
* `Assets/Scripts/Managers/` - Singletons managing game loop, UI, audio, raids, day-night cycle, upgrades, and object pools.
* `Assets/Scripts/StateMachine/` - Concrete player states and state machine logic.
* `Assets/Scripts/EventTypes/` - Event payload definitions (e.g., player movement, inventory items, audio actions).
* `Assets/Scripts/Data/` - ScriptableObject configurations for dialogue, upgrades, buildings, and audio.
* `Assets/ScriptableObjects/` - Assets instanced from ScriptableObject definitions.
* `Assets/Notes/` - Design documents, story concepts, and text backlogs.

---

## 4. Current Backlog & Next Steps (From `ToDo.txt`)
* **Difficulty & Scaling:** Adjust prices, ore yields, sale values, enemy spawns, and day lengths dynamically based on money earned or days survived.
* **UI/UX Contrast:** Improve color consistency and text sizing; implement Minecraft-style hotbar item name pop-ups on scroll.
* **Raid Balancing:** Smooth the difficulty curve for early waves; ensure enemy outposts don't spawn in starting zones.
* **Finite Resources:** Fully deplete resource nodes once exhausted and update the HUD indicators.
* **Tutorial Storytelling:** Integrate a guidance AI into the new State-based TutorialManager.
