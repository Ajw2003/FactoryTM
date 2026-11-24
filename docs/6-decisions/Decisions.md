# Decisions

A running, dated, append-mostly log of what was decided, why, and what it replaced. Newest first. An entry is
never rewritten; the only allowed edit is flipping its **Status** to `Superseded` with a pointer. Earlier
entries below were transcribed on 2026-11-24 from `history/` and `docs/decisions/` (now in
[archive](../archive/README.md)); the dates are the dates of the work, taken from git.

---

## 2026-11-24 — Adopt the six-tier documentation structure

**Context.** The repo had two overlapping record sets (`history/` and `docs/decisions/`) plus a rules file for
a different assistant (`.agents/AGENTS.md`) that told agents to read `docs/decisions/project_summary.md` first.
None said where the project stood against a definition of done.

**Decision.** Create `docs/1-landing` … `docs/6-decisions`, move the old records to `docs/archive/`, and point
`.agents/AGENTS.md` at `docs/1-landing/README.md`.

**Why.** One entry point, one place for state, and a log that only grows. Old files were moved, not deleted,
so links from commit messages and notes still lead somewhere.

**Status.** Standing.

## 2026-11-24 — Roadmap weights approved; M8 becomes the visual overhaul at 10%

**Context.** The owner reviewed the draft weights and approved them, then asked for a visual overhaul: connected
textures and layers working on everything, and sprites at a similar level of detail.

**Decision.** Fold the overhaul into M8 (now "Visual overhaul and feedback") at 10%, taking 3 points from M4 (12%)
and 2 from M5 (13%). All other weights stand. "Similarly detailed" still needs a style sheet from the owner.

**Why.** Chosen from three options; folding keeps the milestone count at nine and leaves the UI (M7) and
mobile (M9) weights unchanged.

**Status.** Standing. Supersedes the weights in [2026-11-24 — Roadmap weights and acceptance criteria drafted, not confirmed](#2026-11-24--roadmap-weights-and-acceptance-criteria-drafted-not-confirmed).

## 2026-11-24 — Destroying the IDT ends the run

**Context.** Old notes say it; the code ends the game only when the player dies (`ShowGameOver` has one caller).

**Decision.** The rule stays and is to be built. It is open work inside milestone M3.

**Why.** Chosen by the project owner when reviewing the roadmap.

**Status.** Standing.

## 2026-11-24 — Roadmap weights and acceptance criteria drafted, not confirmed

**Context.** Tier 2 needs a definition of done; the repo had a to-do list but no acceptance tests.

**Decision.** Draft nine milestones with proposed weights; mark M7 and M9 acceptance as "not yet defined by the
owner".

**Why.** Inventing precise criteria would be guessing. The weights are proposals; changing them is a Tier 2
change and gets its own entry here.

**Status.** Superseded by [2026-11-24 — Roadmap weights approved; M8 becomes the visual overhaul at 10%](#2026-11-24--roadmap-weights-approved-m8-becomes-the-visual-overhaul-at-10) on 2026-11-24.

## 2026-07-25 — Manager state is encapsulated with `[SerializeField] private` plus getters

**Context.** After the player/building pass, 11 more clusters of hard references were audited again from
scratch. Only three real cross-subsystem writes existed (all into `DayNightManager`); the rest were reads.

**Decision.** Writes get methods (`SetTutorialActive`, `SetTimeRemaining`, `AddRaidEnemyReduction`,
`MarkOutpostCleared`…); polling reads get a PascalCase getter over a `[SerializeField] private` field that keeps
its exact old name. Nine already-PascalCase public fields (`MainTileMap`, `ActiveEnemies`, `Plates`, …) were
left public.

**Why.** Keeping the field name keeps every scene and prefab key valid; `Gamemanager.prefab` alone holds
`minPatchSize: 6` / `maxPatchSize: 12` against code defaults of 3 / 7, which an auto-property would have
silently dropped. The nine fields need `[FormerlySerializedAs]` and a scene rewrite, so they were deferred.

**Status.** Standing.

## 2026-07-25 — Events announce changes; they do not replace polling reads

**Context.** A full audit found player health read or written from 30+ sites, plus four events that were
subscribed to but never published.

**Decision.** Use `EventManager` for "notify me when X changed" (player health, ammo, mode; building damaged
and died). Keep polling reads (turret checking `IsAlive`, tile size in per-frame maths) as tightened properties.
Delete the four subscribed-but-never-published events rather than wire them up. `BuildingLogic.Health` is
`{ get; protected set; }` because three subclasses assign it in `Setup`. Events are an additional channel: the
existing `BuildingManager.NotifyBuildingDamaged()` and `outpost.RemoveBuilding(this)` calls stay.

**Why.** An event wrapped around a per-frame poll adds cost and no decoupling. Auto-property conversion of
`maxHealth`, `ammoReserve` and `canDodgeRoll` would have shipped a locked dodge-roll and 90 starting ammo,
because `Player.prefab` stores `canDodgeRoll: 1` and `ammoReserve: 120`.

**Status.** Standing. Detail: [event-system.md](../4-systems/event-system.md).

## 2026-07-23 — Grass/dirt blending uses a cross-tilemap auto-tiler

**Context.** Unity's `RuleTile` only inspects neighbours on the tilemap it is painted on, so it cannot read a
base layer and paint a different overlay layer.

**Decision.** `GrassDirtBlender` reads the base tilemap (tile present = grass) and paints one of five blend
pieces, rotated, onto a separate overlay tilemap; `GrassDirtBlendController` wires it to the scene.

**Why.** Needed two tilemaps working together; `RuleTile` cannot. One art piece (`GrassRightInner`) was visually
unusable, so edge variant B defaults to the same asset as variant A.

**Status.** Standing.

## 2026-07-22 — All items move to the `ItemData` / `InventorySlot` model

**Context.** Resources were a `ResourceType` dictionary on the UI; buildings had their own storage shapes.

**Decision.** Every item, including buildings, is an `ItemData` with a `specificItemType`. Player, hotbar and every
building's input/output hold stacks in `InventorySlot`s; buildings create their slots through
`BuildingSlotFactory`. Left click takes one, right click takes the stack; the grabbed stack follows the mouse
immediately on a top-level `DragLayer`.

**Why.** One transfer path for every pair of containers instead of one per building. Commits `aa674ab` →
`4159da9`. Detail: [inventory-slots.md](../4-systems/inventory-slots.md).

**Status.** Standing. Supersedes the Chest-as-dictionary design below.

## 2026-07-20 — Buildings have ports: one input, one opposite output (IDT: four inputs)

**Context.** Belts could only deliver to a hard-coded list of building types; Miner, Chest, Turret and Wall
were unreachable.

**Decision.** `BuildingLogic.CanAcceptInputFrom(direction)` is the single contract belts query. The IDT accepts
from all four sides; Furnace, Miner and Chest have one input and one output directly opposite and rotate with
the building; Turret and Wall deliberately get none.

**Why.** Giving turrets and walls fake ports "would have been more confusing than helpful" (turret ammo is a
separate system). Port arrows are procedural triangles, the same technique as the crack overlay.

**Status.** Standing.

## 2026-07-20 — Chest stores counts, not belt objects

**Context.** The first Chest kept deactivated pooled belt items in a queue.

**Decision.** Move to counted stacks that are deposited and withdrawn by drag, releasing a new pooled item
onto the belt when output is needed.

**Why.** Holding hidden pooled objects relied on `OnDisable` side effects and could not be resolved to a prefab
for raw ore.

**Status.** Superseded by [2026-07-22 — All items move to the ItemData / InventorySlot model](#2026-07-22--all-items-move-to-the-itemdata--inventoryslot-model) on 2026-07-22.

## 2026-07-20 — Break the tutorial stack overflow by assigning state before `Exit()`

**Context.** After the "defend first raid" step the game crashed: `Exit()` re-entered `ChangeState`, which
still saw the old state as current.

**Decision.** `BaseStateMachine.ChangeState` sets `CurrentState = newState` before calling `oldState.Exit()`.
Separately, the objective text is refreshed by a dirty flag in `Update`, not synchronously in the inventory and
currency handlers.

**Why.** Prevents unbounded recursion in both loops. Detail: [player-and-states.md](../4-systems/player-and-states.md),
[tutorial.md](../4-systems/tutorial.md).

**Status.** Standing.

## 2026-07-20 — Replace unsupported glyphs rather than add them to the font

**Context.** `█` and `▼` do not exist in the `monogram SDF` font; Unity logged a warning every frame.

**Decision.** Use `#` for the dialogue cursor and `v` for the intake arrow.

**Why.** Smallest fix; no font rebuild.

**Status.** Standing.

## 2026-06-26 — Floating text through a pooled singleton with ScriptableObject settings

**Context.** Damage numbers and pop-ups spawned ad hoc caused allocation spikes.

**Decision.** `FloatingTextManager` spawns pooled text configured by `FloatingTextSettings` assets under
`Resources/FloatingTextSettings`.

**Why.** Part of the pooling pass that introduced `ObjectPoolManager` for projectiles, floating text and items
(`7df4ab2`; see [object-pooling.md](../4-systems/object-pooling.md)). Floating text migration: `a4ab777`.

**Status.** Standing.

## 2026-06-25 — Fuel and ammo limit automation; each building gets its own panel

**Context.** Automation produced unlimited output, and every building shared one generic panel.

**Decision.** Miners and furnaces consume fuel; turrets consume ammo; each building type has a bespoke panel
opened with `E`; the player is frozen by `PlayerBuildingUiState` while one is open; enemy outposts and raids
do not occur during the tutorial.

**Why.** Operating costs stop passive infinite income; bespoke panels show fuel, queues and ammo where they matter.
Commits `299a992` (2026-06-25, bespoke panels) and `0872918` (2026-06-26, tutorial as states); the fuel and
ammo limits were part of the same session's work, with no single commit named in the old notes.

**Status.** Standing. (The 2026-07-20 port work later changed how items reach these panels.)

## 2026-06-24 — Map expansion by zones, with Control-scroll zoom

**Context.** The map was static.

**Decision.** The world is split into unlockable zones (default 30×15 tiles); unlocking costs money and
increases per zone; with four zones in a square, Control+scroll zooms out to a bird's-eye view.

**Why.** Gives scale and exploration. Commits `40a65fb`, `e4098b9`.

**Status.** Standing.

## 2026-06-24 — Enemy buildings use `BuildingData` ScriptableObjects; damage shown as cracks

**Context.** Enemy outposts were built from runtime values; combat had no feedback on structures.

**Decision.** Enemy turrets and spawners use the same `BuildingData` assets as player buildings. Buildings show
a procedural crack overlay that grows as health falls below 75%, 50%, 25%.

**Why.** Designers can tune enemies in the Inspector; the crack overlay avoids a health bar on every building.
Commits `45c3ab8`, `be5cf7d`.

**Status.** Standing.

## 2026-06-10 — Use the new Input System and remove mass-imported scripts

**Context.** A mass import of scripts from other projects brought in a large amount of unused code.

**Decision.** Refactor to the Input System and delete what was unused.

**Why.** Commit `refactor codebase to use new input system and remove a vast amount of unused scripts`.

**Status.** Standing, but `PlacementManager` and `HotbarManager` still poll `UnityEngine.Input`
(see [ProjectState](../3-state/ProjectState.md)).

## 2026-05-10 — A central `BuildingManager` ticks buildings; items do not move themselves

**Context.** Early belts and miners acted independently.

**Decision.** Items and transport are dictionary-driven and a central building manager fires commands each
frame, rather than each item running its own logic.

**Why.** Recorded in the commit message `rework logic to use dictionary based item spawning, selling and transport
and have a central building manager that fires commands rather than the items themselves`.

**Status.** Standing. Detail: [buildings-and-conveyors.md](../4-systems/buildings-and-conveyors.md).
