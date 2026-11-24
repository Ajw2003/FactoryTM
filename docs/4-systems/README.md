# Tier 4 — runtime-critical systems

One document per system whose failure stops the product working. Each follows the same order: what it owns,
how it works, invariants, traps. Written from the code on branch `EventRefactor` (2026-11-24); they cite
`file:line` so a drift check is mechanical.

| System | Doc | Owns |
|---|---|---|
| Event system | [event-system.md](event-system.md) | `EventManager`, `IEvent`, event payloads |
| Day cycle and raids | [day-cycle-and-raids.md](day-cycle-and-raids.md) | Day / Evening / Upgrade phases, raid scaling, the pause for upgrade choice |
| Inventory slots | [inventory-slots.md](inventory-slots.md) | `InventorySlot`, `Item`, `DragLayer`, `ItemData`, building stock |
| Buildings and conveyors | [buildings-and-conveyors.md](buildings-and-conveyors.md) | `BuildingLogic`, tick loop, grid, ports, belts, the IDT |
| Player and states | [player-and-states.md](player-and-states.md) | Input path, `PlayerController`, `BaseStateMachine`, player states |
| Tutorial | [tutorial.md](tutorial.md) | The eleven-step scripted first run |
| Object pooling | [object-pooling.md](object-pooling.md) | `ObjectPoolManager`, `PooledObject` |

## Considered and deliberately left out

The test was "if this is wrong, does the product stop working?"

| Candidate | Why it is not a system doc (yet) |
|---|---|
| Terrain blending (`GrassDirtBlender`, `GrassDirtBlendController`) | Cosmetic. A wrong tile looks bad; nothing stops working. Revisit if blending becomes runtime-driven by resource stripping. |
| Upgrades and store (`UpgradeManager`, `StoreUiScript`, 1,392 lines) | Important, but the store UI is slated for the UI overhaul (issue #1); documenting its current shape would be written to be thrown away. `UpgradeManager` is summarised in [Roadmap](../2-roadmap/Roadmap.md). |
| Zones (`ZoneManager`) | Small and self-contained: a set of unlocked zone coordinates, a cost curve, a camera move. Add a doc if zone rules grow. |
| Enemy outposts and `CartelMember` AI | Behaviour, not infrastructure. `EnemyOutpost` clearing is covered in the tutorial doc where it matters. |
| UI managers (`UiManager`, `BuildingUiManager`, `DialogueManager`, `PauseManager`) | In scope of the UI overhaul (issue #1). `BuildingUiManager` is 874 lines and likely to be rewritten. |
| Audio, weapons and projectiles, floating text, resource nodes | Each is self-contained; failure is local, not fatal. |
| `SingletonBase<T>` | Its one quirk (auto-create) is noted in [ProjectState](../3-state/ProjectState.md); a doc of its own would restate 87 lines of code. Rule of use: [`SingletonBase`](../../Assets/Scripts/Utils/Singleton.cs) creates the object on demand if none exists, and returns the stale instance once the app is quitting. |
