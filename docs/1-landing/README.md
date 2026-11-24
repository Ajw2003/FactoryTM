# FactoryTM — landing page

## What this is

FactoryTM is a 2D Unity factory-automation roguelike. You play an alien who crashlands on Earth, mines
resources by hand and with machines, moves them on conveyor belts through furnaces into the
InterDimensionalTransporter (IDT) to sell them home, spends the money on weapons and upgrades, and defends the
base against nightly raids by human combatants. Unity 6000.3.10f1, Universal Render Pipeline, C#. One scene
is in the build (`Assets/Scenes/SampleScene.unity`). Remote: `Ajw2003/FactoryTM`.

Direction (open issues): #1 a UI/UX overhaul that makes everything diegetic; #2 a beginner-friendly edition with
a tower-defense spin that runs on mobile.

## The moving parts

| Part | Where | One line |
|---|---|---|
| Managers | `Assets/Scripts/Managers/` | Singletons for the loop, grid, placement, UI, audio, pooling, upgrades, zones |
| Buildings | `Assets/Scripts/Placeables/` | `BuildingLogic` subclasses: miner, furnace, chest, IDT, turret, wall, conveyor |
| Items and inventory | `Assets/Scripts/Items/`, `Utils/DragLayer.cs` | Slot-based stacks and drag and drop |
| Player and input | `Assets/Scripts/Player/`, `Input/`, `StateMachine/` | Controller, input events, player states |
| Events | `Assets/Scripts/EventTypes/`, `Managers/EventManager.cs` | Publish/subscribe bus |
| Data | `Assets/Scripts/Data/`, `Assets/Resources/` | ScriptableObject definitions and the assets that fill them |
| Enemies and weapons | `Assets/Scripts/Npcs/`, `Weapons/` | Cartel enemies, projectiles, turrets |
| Terrain | `Assets/Scripts/Terrain/` | Grass/dirt blend tiles |
| Notes | `Assets/Notes/` | `ToDo.txt`, `Story.txt` (working notes, not docs) |

## The six tiers

| Tier | Document | Answers |
|---|---|---|
| 1 | this page | What is this, where is everything |
| 2 | [Roadmap](../2-roadmap/Roadmap.md) | What 0–100% means; what "done" looks like |
| 3 | [ProjectState](../3-state/ProjectState.md) | Where it stands now against tier 2 |
| 4 | [Systems](../4-systems/README.md) | How each runtime-critical system works |
| 5 | [Today](../5-today/Today.md) | What is being worked on today and why |
| 6 | [Decisions](../6-decisions/Decisions.md) | Why decisions were made; what they replaced |

## Systems (tier 4)

| System | Owns |
|---|---|
| [Event system](../4-systems/event-system.md) | `EventManager`, `IEvent`, payloads |
| [Day cycle and raids](../4-systems/day-cycle-and-raids.md) | Day, Evening, Upgrade phases; raid scaling |
| [Inventory slots](../4-systems/inventory-slots.md) | `InventorySlot`, `Item`, `DragLayer`, building stock |
| [Buildings and conveyors](../4-systems/buildings-and-conveyors.md) | Tick loop, grid, ports, belts, the IDT |
| [Player and states](../4-systems/player-and-states.md) | Input path, player stats, state machines |
| [Tutorial](../4-systems/tutorial.md) | The eleven-step first run |
| [Object pooling](../4-systems/object-pooling.md) | `ObjectPoolManager` |

## Everything else worth reaching

- [Archive](../archive/README.md) — old session logs, phase plan and history, and why each is inert.
- [`docs/plans/`](../plans/) — plans for specific work (empty today).
- [`docs/generated/`](../generated/) — tool-produced reports (empty today).
- [CHANGELOG.md](../../CHANGELOG.md) — one line per change.
- `Assets/Notes/ToDo.txt` — the owner's own backlog; the roadmap is built from it.
- `.agents/AGENTS.md` — coding conventions for assistants (namespaces, singletons, events, state machines).

## Conventions

- **Cite claims to `file:line`.** A system doc that cannot point at code is a guess.
- **Update the tier that changed**, not every tier. Tier 3 moving is routine; tier 2 moving means the
  definition of done moved.
- **When a decision reverses, fix the body and leave a pointer.** Add a dated entry to Decisions.md and flip the
  old entry's status to `Superseded`; do not stack a banner over a stale table.
- **A document that goes inert moves to `archive/`.** It is not deleted.
- **Serialized fields keep their names.** Scenes and prefabs store values by field name; renaming a field or
  turning it into an auto-property silently resets it.
- **Notify with events, read with properties.** See [event-system.md](../4-systems/event-system.md).
