# Project state — 2026-11-24

Measured against [Roadmap](../2-roadmap/Roadmap.md). Source: the code on branch `EventRefactor` at commit
`2e87a28`, git history, `Assets/Notes/ToDo.txt`, the archived session logs, and open issues #1 and #2.

## Headline

**0% verified.** No milestone's acceptance criterion has been run and recorded as passed. That is not the same
as "nothing works": the game was clearly played during development (the last commit is "fix hearts not
existing"). It means nobody has written down a pass against the criteria, and the history says so in places
("Not playtested", "none of this has been run in the Unity Editor").

**Code exists for 8 of 9 milestones**; M9 (mobile edition) has no work in the repo. There are no automated
tests anywhere in the project (no test assembly under `Assets/`), and no CI.

| # | Milestone | Weight | Code | Acceptance checked | Notes |
|---|---|---|---|---|---|
| M1 | Core factory loop | 15% | Built | No | One known open bug: placement over loose belt items |
| M2 | Slot inventory and building I/O | 10% | Built | No | Rewritten 2026-07-20 to 07-22; store Inventory tab is read-only in practice |
| M3 | Combat, raids, day cycle | 15% | Built, one gap | No | IDT-destroyed game over not in code |
| M4 | Upgrades, store, balance | 15% | Partly | No | Upgrades and shop exist; balance, difficulty, day-length scaling, starting store rules not done |
| M5 | Tutorial and story | 15% | Partly | No | Eleven-step machine exists; narrator, story, itemised objectives not started |
| M6 | Code architecture | 10% | In progress | No | See below |
| M7 | UI / UX overhaul | 10% | Not started | No | Issue #1 open, acceptance not defined |
| M8 | World visuals and feedback | 5% | Partly | No | Blend tiles done on 2026-07-23; vegetation loss, particles, sounds not done |
| M9 | Mobile edition | 5% | Not started | No | Issue #2 open, no device target |

## Per milestone

**M1.** Miners, furnace, conveyors, chest and IDT all exist and use the shared port rule
([buildings-and-conveyors.md](../4-systems/buildings-and-conveyors.md)). Not built: the fix for items on a
cell blocking placement; belt splitters and multi-lane belts (listed in the to-do notes as ideas).

**M2.** Complete as designed: slots, drag layer, building slots, hotbar integration
([inventory-slots.md](../4-systems/inventory-slots.md)). Not built: depositing from the store's Inventory tab.

**M3.** The loop works as described in [day-cycle-and-raids.md](../4-systems/day-cycle-and-raids.md).
Not built: enemy wandering versus raiders, a gentler early spawn curve, enemies being unable to attack from
zones the player does not own.

**M4.** `UpgradeManager` loads upgrades from `Resources/Upgrades` and applies effects. Not built: essential
upgrades first, starting-store restriction, difficulty settings, achievements, day length that changes.

**M5.** The state machine and its steps exist ([tutorial.md](../4-systems/tutorial.md)). Not built: AI
narrator, itemised objectives, sections for land expansion, ammo/health and shooting; short objective text.

**M6.** Done so far: the two highest-impact clusters (player health, building health) now use events; dead
events and the `InventoryEvents`/`InputEvents` folders were removed; 23+ manager fields encapsulated with
`[SerializeField] private` plus getters. Compile check reported 0 errors (history entries dated 2026-07-25).
Not done: nine PascalCase public fields (`MainTileMap`, `ActiveEnemies`, `Plates`…) still public; the old
`UnityEngine.Input` polling in `PlacementManager` and `HotbarManager`; the plain C# events listed in
[event-system.md](../4-systems/event-system.md); `PlayerController.OnPlayerDodge`.

**M7, M9.** Nothing built. Both are epics with no acceptance criteria from the owner yet.

**M8.** Grass/dirt blending: `GrassDirtBlender` and an editor tool exist (commits `b761ccd` to `d486cbf`).
Not built: runtime vegetation removal, per-building particles, sound for every action.

## The one thing that is not what it looks like

**"Destroy the IDT and you lose."** The old project summary and the to-do list both describe it as a rule of
the game. The code does not do it: `UiManager.ShowGameOver()` is called from exactly one place,
the player's death ([PlayerController.cs:549](../../Assets/Scripts/Player/PlayerController.cs)). A reader of
the old docs would assume the IDT is the central stake of every raid; in the current build only the player's
health is.

## Cross-cutting issues that belong to no milestone

- **No tests, no CI.** Compile checks were done by hand with the editor's bundled compiler.
- **Input standard.** `PlacementManager` and `HotbarManager` read the old `Input` class; house standard is
  the Input System. Counted under M6 but easy to forget.
- **`Assets/Scripts.zip`** (55 files, about 39 KB packed) and `Assets/_Recovery` sit inside the project; their
  contents were not compared with the live source and could be stale copies.
- **`scratch/fix_spacing.py`** rewrote files with Windows line endings once (fixed by passing `newline=''`);
  if it is run again on prefabs it must keep that flag.
- **Singleton auto-create.** `SingletonBase.Instance` creates an empty object of that type if none exists, so a
  missing scene reference shows up as a blank manager, not an error.
- **Unity MCP / editor tooling was not reachable** during this documentation pass, so nothing here was
  confirmed by running the project.
