# Session Log — 2026-07-20

## 1. Tutorial-End Stack Overflow

### Layman Summary
Right after the "defend first raid" tutorial step finished, the game crashed with a stack overflow. The tutorial's state machine was telling itself to "finish" over and over, infinitely, because it updated its "current state" bookkeeping *after* running the cleanup code for the old state instead of *before*. That let the cleanup code accidentally re-trigger itself forever.

### Technical Specs
- **Root cause file:** `Assets/Scripts/StateMachine/BaseStateMachine.cs`, `ChangeState()`.
- **Chain:** `DefendFirstRaidTutorialState.Exit()` → `TutorialManager.CompleteTutorial()` → `BaseStateMachine.ChangeState(completedState)`. The guard `if (newState == CurrentState) return;` never tripped because `CurrentState` was still the old state when `Exit()` re-entered `ChangeState`, so it called `Exit()` on itself again, forever, until the call stack overflowed.
- **Fix:** Reordered `ChangeState()` to assign `CurrentState = newState` *before* invoking `oldState.Exit()`, so the re-entrant call's guard now correctly short-circuits.
- Confirmed via `Editor.log` — dozens of repeated `StackOverflowException` traces all rooted at `DefendFirstRaidTutorialState.Exit()`.

---

## 2. Unicode Font Warnings

### Layman Summary
Two UI elements used special Unicode glyphs (a solid block `█` for the dialogue "press space" blinking cursor, and a down-arrow `▼` for a building intake indicator) that don't exist in the game's pixel font (`monogram SDF`). Every frame, Unity silently replaced them with blank spaces and logged a warning. Swapped both for plain ASCII characters the font actually has.

### Technical Specs
- `Assets/Scripts/Managers/DialogueManager.cs` — `BlinkPromptCursor()`: `█` → `#`.
- `Assets/Scripts/Managers/BuildingUiManager.cs` — arrow text: `▼` → `v`.

---

## 3. Building Intake/Output Port System

### Layman Summary
Added a real "this side takes items in, that side sends them out" system to buildings, plus small on-screen arrows so it's visually obvious which side is which. The IDT (interdimensional transporter) can now accept items from any of its 4 sides, since it's the end of the line. Every other item-handling building (Furnace, Miner, Chest, Conveyor) gets exactly one input and one output, directly opposite each other, and it rotates together with the building when placed (same R-key rotation players already use).

Turrets and walls were intentionally left out — they don't move items today (turret ammo is a separate system), so giving them fake ports would've been more confusing than helpful.

Chest was previously an empty, unused stub (not even placeable). It's now a real storage building: it pulls items in on its input side (up to a capacity of 20), and pushes them back out its output side onto an empty conveyor. It uses placeholder wall artwork for now — see caveats below.

### Technical Specs
- **`Assets/Scripts/Placeables/BuildingLogic.cs`** (base class) — added:
  - `rotationIndex`, `GetFacingDirection()` (wraps the existing `GameManager.GetDirectionFromRotationIndex`).
  - `CanAcceptInputFrom(Vector2Int incomingDirection)` — virtual, defaults to `false` (no input side); this is the new general contract conveyors query instead of a hardcoded type whitelist.
  - Shared footprint/edge math (`GetActualSize`, `GetEdgeCells`, `GetNeighborCellInDirection`, `GetEdgeSpawnPosition`, `GetOutputSpawnPosition`, `ComputeFootprintCells`) — factored out of near-identical duplicated code that used to live separately in `Miner.cs` and `Furnace.cs`.
  - `CreatePortIndicator(...)` — spawns a small procedurally-generated triangle sprite (same `Texture2D`/`SetPixel` technique already used for the building damage-crack overlay) as a world-space child, tinted green for output / cyan for input.
- **`Assets/Scripts/Placeables/ConveyorLogic.cs`** — `PerformAction()` now asks the target building `CanAcceptInputFrom(direction)` instead of checking a hardcoded `BuildingType` switch (previously only Conveyor/Furnace/Seller could ever receive items — Miner, Chest, Turret, Wall were silently unreachable). Conveyors themselves always return `true` (side-loading allowed, unchanged behavior).
- **`Assets/Scripts/Placeables/Furnace.cs`** — single input side (opposite the output/export direction); `PerformAction()` now only scans the input-edge cell(s) instead of every occupied cell. `CookItem()` reuses the shared output helper instead of duplicated boundary math.
- **`Assets/Scripts/Placeables/Miner.cs`** — output-only by design (it mines from the ground, not from belts); cleaned up to use the shared helpers, added an output arrow.
- **`Assets/Scripts/Placeables/InterDimensionalTransporter.cs`** — `CanAcceptInputFrom` always returns `true`; added a `Setup(data, cell, rotationIndex)` overload; spawns 4 inward-pointing cyan arrows, one per side.
- **`Assets/Scripts/Placeables/Chest.cs`** — rewritten from an empty `MonoBehaviour` stub into a `BuildingLogic` subclass with a `Queue<ConveyorItem>` buffer (`capacity = 20`), single input/output side, and port indicators. Items are held by deactivating the pooled GameObject (which unregisters it from `ItemTracker` via its existing `OnDisable`) *without* returning it to the shared object pool, so the same instance comes back out later — avoids needing a resource-type → prefab lookup for raw ores (only cooked "plates" have one, via `GameManager.FindPlatePrefabWithPrefix`).
- **`Assets/Scripts/Managers/PlacementManager.cs`** — added the missing `BuildingType.Chest` spawn case (`SpawnChestLogic`); IDT spawn now passes the placement rotation through.
- **`Assets/Resources/BuildingData/Chest.asset`** (+ `.meta`) — new `BuildingData` resource; hand-authored since none existed. **Uses the Wall building's sprite/tiles as a placeholder** (clearly labeled in its `description` field).

### Caveats / Follow-ups for you
1. **Chest has no real art yet.** It'll place and function, but visually looks like a Wall until someone drops in real chest sprites and reassigns `rotatedTiles`/`icon` on `Chest.asset` in the Inspector.
2. **Chest isn't wired into the store or starting inventory.** `GameManager.allBuildings` and whatever grants starting buildings are Inspector-serialized scene/prefab references — I didn't touch those blind. Add `Chest.asset` there if you want it purchasable.
3. **Port indicator rotation is a best-effort guess** (`Quaternion.Euler(0,0,angle)` mapped from an upward-pointing generated triangle). Unity's 2D Z-rotation direction can vary by camera setup — worth a quick visual check in the Editor; if arrows are mirrored it's a one-line sign flip in `BuildingLogic.CreatePortIndicator`.
4. Turret and Wall were deliberately left without ports (see summary above).
