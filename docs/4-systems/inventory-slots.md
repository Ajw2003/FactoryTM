# Inventory slots and drag-and-drop

## What it owns

How a stack of items is held and moved by the player and by buildings. It owns `InventorySlot` (one
stack of one item type), `ItemData` (the ScriptableObject describing an item), the draggable `Item`
cursor, `DragLayer` (the always-on-top canvas that carries it), and `BuildingSlotFactory` (makes slots
that belong to a building). It also owns the separate **building stock** list in `InventoryManager`
(how many of each placeable building the player owns). It does not own items moving on belts (see
[buildings-and-conveyors.md](buildings-and-conveyors.md)) or money (`CurrencyManager`).

## How it works

- `ItemData` has a sprite, name, description and a `specificItemType` enum
  ([ItemData.cs](../../Assets/Scripts/Items/ItemData.cs)). That enum mixes buildings (`Miner`, `IDT`,
  `Conveyor`…) and ores (`Coal`, `Iron`…); identity of a stack is the enum value, not the asset.
- `InventorySlot.TryAdd(data, amount)` accepts the item if the slot is empty or already holds the same
  `specificItemType`, and if `allowedTypes` (when set) contains it
  ([InventorySlot.cs:82-101](../../Assets/Scripts/Items/InventorySlot.cs)). `TryRemove` returns how many
  were actually removed and clears the slot at zero. Both raise `OnSlotChanged`.
- **Grab**: pointer-down on a filled slot takes 1 (left click) or the whole stack (right click) out of the
  slot immediately and hands it to `DragLayer.BeginDrag`
  ([InventorySlot.cs:154-164](../../Assets/Scripts/Items/InventorySlot.cs)).
- **Drop**: on pointer-up, `DragLayer.EndDrag` → `Item.CheckCollision` publishes
  `CollisionItemExchangeEvent` with the cursor's rectangle. Each slot checks overlap and tries to add;
  the first to succeed calls `MarkClaimed()`. If nobody claims it, the stack goes back to `originSlot`
  ([Item.cs:94-107](../../Assets/Scripts/Items/Item.cs)).
- **Building slots**: `BuildingSlotFactory.CreateSlot(owner)` instantiates the slot prefab under the
  building, inactive ([BuildingSlotFactory.cs:12-19](../../Assets/Scripts/Items/BuildingSlotFactory.cs)).
  `BuildingUiManager` reparents the active slots into the shared panel while that building is open. The
  IDT has a `FuelSlot` (only Coal and Uranium) and a `SellSlot`; Furnace, Miner and Chest also own slots.
- **Building stock**: `InventoryManager.items` is a list of `{BuildingData, count}`. Adding or removing
  raises `onInventoryChange`, which `InventoryItemUiButton` ([line 37](../../Assets/Scripts/Managers/InventoryItemUiButton.cs))
  and `TutorialManager` ([line 207](../../Assets/Scripts/Managers/TutorialManager.cs)) listen to.

## Invariants

1. **Quantity lives in `InventorySlot.itemCount`, never in the UI text.** The label is written from the
   count each change.
2. **A grab always resolves.** Whatever is taken out of a slot must be either claimed by another slot or
   given back to `originSlot`. `OnPointerUp` calls `EndDrag` unconditionally for exactly this reason
   ([InventorySlot.cs:181-184](../../Assets/Scripts/Items/InventorySlot.cs)).
3. **`DragLayer.IsDragging` gates world input.** Weapon fire ([PlayerWeapon.cs:84](../../Assets/Scripts/Weapons/PlayerWeapon.cs))
   and building placement ([PlacementManager.cs:90](../../Assets/Scripts/Managers/PlacementManager.cs))
   check it so a UI drag does not also shoot or place. New world-input code must do the same.
4. **Slots that can be created inactive initialise in `Awake`, not `Start`**
   ([InventorySlot.cs:37-55](../../Assets/Scripts/Items/InventorySlot.cs)).
5. **Fuel restrictions are set through `allowedTypes`** and checked by `TryAdd`; do not special-case the
   building.

## Traps

- **Dropping on two overlapping slots.** Both hear the event; the `IsClaimed` check
  ([Item.cs:36-39](../../Assets/Scripts/Items/Item.cs)) is the only thing preventing double counting.
- **`OnItemSold` can be called with `null`.** Selling from the IDT `SellSlot` calls
  `OnItemSold?.Invoke(null)` ([InterDimensionalTransporter.cs:224](../../Assets/Scripts/Placeables/InterDimensionalTransporter.cs));
  a subscriber that reads the item crashes. Belt sales pass the real `ConveyorItem`.
- **`InventoryManager.RemoveBuilding` keeps zero-count rows** (the removal is commented out,
  [InventoryManager.cs:57-60](../../Assets/Scripts/Managers/InventoryManager.cs)). `HasBuilding` is
  count-based, so this is safe, but anything iterating `items` sees empty rows.
- **Two inventories, one word.** "Inventory" means the slot-based item stacks *and* the building stock
  list. The tutorial's `onInventoryChange` listens to the second.
- The inventory was rewritten 2026-07-20 to 2026-07-22 (commits `aa674ab` to `4159da9`); older notes in
  `docs/archive/history/` describe the previous dictionary-based design and are no longer accurate.
