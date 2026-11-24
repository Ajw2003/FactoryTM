# Buildings, placement and conveyor flow

## What it owns

Everything that sits on the grid and moves items: the `BuildingLogic` base class and its subclasses, the
registry and tick loop (`BuildingManager`), placement and removal (`PlacementManager`), the grid maths
(`GridManager`), and item movement on belts (`ConveyorLogic`, `ConveyorItem`, `ItemTracker`). It also owns the
input/output "port" rule. It does not own item *stacks* inside buildings (see
[inventory-slots.md](inventory-slots.md)) or pooling of the item objects (see
[object-pooling.md](object-pooling.md)).

## How it works

- **Tick loop.** Every `BuildingLogic` registers itself with `BuildingManager` in `OnEnable` and leaves in
  `OnDisable`. `BuildingManager.Update` calls `PerformAction()` on each, in list order, every frame
  ([BuildingManager.cs:63-69](../../Assets/Scripts/Managers/BuildingManager.cs),
  [BuildingLogic.cs:204-214](../../Assets/Scripts/Placeables/BuildingLogic.cs)).
- **Grid.** `GridManager` converts between world position and cell (`CellToWorldConversion` returns the cell
  *centre*; `WorldToCellConversion` floors) and takes its tile size and bounds from the main tilemap in
  `Start` ([GridManager.cs:33-71](../../Assets/Scripts/Managers/GridManager.cs)).
- **Footprint and rotation.** A building has `myCell` (its origin), `size`, and `rotationIndex` (0-3). For an
  odd rotation the width and height swap ([BuildingLogic.cs:82-86](../../Assets/Scripts/Placeables/BuildingLogic.cs)).
  `occupiedCells` is the rectangle from the origin.
- **Ports.** The *output* side is the facing direction; the single *input* side is directly opposite
  (`GetInputCells` / `GetOutputCells`, [BuildingLogic.cs:70-79](../../Assets/Scripts/Placeables/BuildingLogic.cs)).
  The IDT accepts from all four sides; Furnace, Miner and Chest have one input and one output; Turret and
  Wall have none. A building says whether it takes an item from a given direction by overriding
  `CanAcceptInputFrom`; the default is `false`. Cyan arrows mark inputs and green arrows mark outputs.
- **Belts.** A `ConveyorLogic` finds the building in the cell it points at and caches it. The cache is
  refreshed only when `PlacementManager.PlacementVersion` changes, a counter bumped on every place or
  remove ([ConveyorLogic.cs:37-59](../../Assets/Scripts/Placeables/ConveyorLogic.cs)). Each tick it moves any
  stationary item from its own cell into the target cell, but only if the target cell holds no item
  ([ConveyorLogic.cs:61-82](../../Assets/Scripts/Placeables/ConveyorLogic.cs)). Belt speed is fixed at
  1.25 cells per second to match the tile animation.
- **Item location.** `ItemTracker` is a `Dictionary<cell, List<ConveyorItem>>`. A `ConveyorItem` registers
  when it is initialised, updates its cell in `SetTarget`, and unregisters in `OnDisable`
  ([ConveyorItem.cs:22-77](../../Assets/Scripts/Items/ConveyorItem.cs)).
- **Damage and death.** `TakeDamage` spawns floating text, redraws the procedural crack overlay, raises the
  "your building is under attack" alert (player-owned only) and publishes `BuildingDamagedEvent`. `Die` pays
  out `cost * 1.5` for enemy buildings, tells the outpost, asks `PlacementManager` to remove it, and
  publishes `BuildingDiedEvent` ([BuildingLogic.cs:218-285](../../Assets/Scripts/Placeables/BuildingLogic.cs)).
- **The IDT** (`InterDimensionalTransporter`) is the sink: Coal and Uranium become fuel seconds, everything
  else is sold for `item.value` (doubled while uranium-boosted) but only while fuel remains. With no fuel,
  items wait and an alert shows ([InterDimensionalTransporter.cs:96-170](../../Assets/Scripts/Placeables/InterDimensionalTransporter.cs)).
  It is the only building that takes its own static `Instance`.

## Invariants

1. **A building must be registered while it exists and not after.** Register in `OnEnable`, unregister in
   `OnDisable`; `BuildingManager` iterates a plain list and calls into every entry.
2. **Every place, rotate or remove that changes what a belt points at must bump `PlacementVersion`.** Belts
   never re-check their target otherwise. The bumps are in `PlacementManager` (lines 307, 408, 442, 456,
   467, 478, 492, 504, 516).
3. **One item per belt cell.** A belt only pushes into an empty cell; do not add code that stacks items.
4. **`Health` is read-only outside the building.** Use `TakeDamage`. Subclasses set it in `Setup`
   (`protected set`).
5. **Ownership changes go through `SetEnemyOwned`.** `TurretLogic` overrides it to also flip its weapon's
   `isEnemyFired` flag.
6. **A new building that moves items needs `CanAcceptInputFrom`**, or belts treat it as a wall.

## Traps

- **Silent dead ends.** A belt facing a building whose `CanAcceptInputFrom` returns false just stops. Before
  the port rule existed, Miner, Chest, Turret and Wall were unreachable for exactly this reason.
- **Items on empty cells block placement.** The to-do list records "can't place a tile on a valid empty space
  with items on it" as in progress; the fix is not in the code yet (`Assets/Notes/ToDo.txt`).
- **`BuildingManager.Update` runs even when paused.** `Time.timeScale` is 0, but `PerformAction` is
  `Update`-driven, not time-scaled. Belt movement uses `deltaTime` so items freeze, but sales and fuel
  checks still run. Only `Furnace` ([line 68](../../Assets/Scripts/Placeables/Furnace.cs)) and the IDT's own `Update`
  ([line 73](../../Assets/Scripts/Placeables/InterDimensionalTransporter.cs)) check `PauseManager.IsPaused`.
  Not yet verified in play.
- **Placement polls the old `Input` class** (`Input.GetKeyDown(R)`, mouse buttons;
  [PlacementManager.cs:122,146,258](../../Assets/Scripts/Managers/PlacementManager.cs)). The Input System
  bindings for place/rotate/remove still exist but nothing reads them. See
  [player-and-states.md](player-and-states.md).
- **Spawn order matters for enemy buildings.** Ownership is set *before* `Setup` resolves the turret weapon,
  hence the null guard in `TurretLogic.SetEnemyOwned`.
- **The crack overlay builds a `Texture2D` per building.** `OnDestroy` frees it
  ([BuildingLogic.cs:414-424](../../Assets/Scripts/Placeables/BuildingLogic.cs)); a subclass overriding
  `OnDestroy` must call `base.OnDestroy()`.
- `Seller.cs` is a 7-line stub kept because `BuildingType.Seller` is still the IDT's data type; the starting
  IDT is found by that type in `GameManager.allBuildings`
  ([GameManager.cs:151-159](../../Assets/Scripts/Managers/GameManager.cs)).
