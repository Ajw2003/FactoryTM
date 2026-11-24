# Object pooling

## What it owns

Reuse of frequently created objects: belt items, projectiles, floating text and anything else a caller asks
the pool for. It owns `ObjectPoolManager` and the `PooledObject` tag component. It does not own what a
pooled object does when it is reused; each owner resets its own state.

## How it works

- Pools are keyed by `prefab.GetInstanceID()`; each key has a `Queue<GameObject>`
  ([ObjectPoolManager.cs:15, 27](../../Assets/Scripts/Managers/ObjectPoolManager.cs)).
- `GetPooledObject(prefab, position, rotation)` dequeues until it finds a non-destroyed object, repositions it
  and activates it. If the queue is empty it `Instantiate`s a new one and attaches a `PooledObject` carrying
  the prefab key ([ObjectPoolManager.cs:23-57](../../Assets/Scripts/Managers/ObjectPoolManager.cs)).
- `ReturnToPool(obj)` deactivates the object and enqueues it under its stored key. An object **without** a
  `PooledObject` is destroyed instead ([ObjectPoolManager.cs:59-83](../../Assets/Scripts/Managers/ObjectPoolManager.cs)).
- There is no pre-warm and no size cap: pools grow on demand and never shrink.

## Invariants

1. **A pooled object is "gone" when inactive.** Anything that tracks it must unregister in `OnDisable`.
   `ConveyorItem` does ([ConveyorItem.cs:70-77](../../Assets/Scripts/Items/ConveyorItem.cs)); `ItemTracker`
   would otherwise hold a dead reference to a reused object.
2. **Return to the pool, do not `Destroy`**, for anything created by `GetPooledObject`.
3. **Pool callers reset state on reuse.** The pool only sets position, rotation and active.
4. **Object identity is the prefab asset.** Two different prefab assets are two pools even if they look the
   same.

## Traps

- **Double return.** `ReturnToPool` checks `queue.Contains(obj)` before enqueueing, which prevents the same
  object being handed out twice. That check is a linear scan of the queue, so a very large pool makes each
  return slower.
- **Not everything is pooled.** Callers today are belt items (Furnace, Miner, Chest, IDT, `GameManager`,
  `PlacementManager`, `PlayerController`), floating text (`FloatingText.cs`) and projectiles (`BaseWeapon`,
  `BaseProjectile`). Enemies are **not**: `RaidManager.SpawnEnemy` uses plain `Instantiate`
  ([RaidManager.cs:163](../../Assets/Scripts/Managers/RaidManager.cs)). Check the call site before assuming
  a type is pooled.
- **Held-but-hidden items.** The first Chest design deactivated a belt item to "store" it without returning
  it, relying on `OnDisable` to unregister it from `ItemTracker`. Chest now keeps counts in
  `InventorySlot`s and returns the object to the pool. Do not reintroduce a design where something other than
  the pool holds a deactivated pooled object; the pool cannot tell it is spoken for.
- `ObjectPoolManager` does not persist between scenes, so pools are lost on reload and any held reference to a
  pooled object from another scene is dead.
