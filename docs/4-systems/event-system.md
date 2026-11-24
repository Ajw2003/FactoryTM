# Event system

## What it owns

A single publish/subscribe bus, so that gameplay code can announce "this changed" without knowing who
is listening. It owns the `IEvent` marker, the `EventManager` singleton, and the event payload classes
under `Assets/Scripts/EventTypes/`. It does not own the *polling* reads (a turret checking
`building.IsAlive` each frame) and it does not own the older plain C# `event` delegates listed under
"Two mechanisms" below.

## How it works

- `IEvent` is an empty marker interface ([IEvent.cs](../../Assets/Scripts/EventTypes/IEvent.cs)).
- `EventManager` keeps a `Dictionary<Type, List<EventSubscription>>`
  ([EventManager.cs:21](../../Assets/Scripts/Managers/EventManager.cs)). A subscription is a
  `WeakReference` to the subscriber plus the callback delegate.
- Subscribe: `EventManager.Instance?.Subscribe(this, (MyEvent e) => Handle(e.Value))`
  ([EventManager.cs:41](../../Assets/Scripts/Managers/EventManager.cs)).
- Publish: `EventManager.Instance?.Publish(new MyEvent(...))`. It walks the list **backwards**, skips
  entries marked for removal, and wraps each callback in a `try/catch` that logs and carries on, so one
  broken subscriber cannot stop the others ([EventManager.cs:124](../../Assets/Scripts/Managers/EventManager.cs)).
- Unsubscribing while a publish is running only *marks* the entry; real removal happens in
  `CleanupPendingRemovals` once the publish ends ([EventManager.cs:69](../../Assets/Scripts/Managers/EventManager.cs)).
- The manager does not persist between scenes (`persistBetweenScenes = false`,
  [EventManager.cs:27](../../Assets/Scripts/Managers/EventManager.cs)), and `Clear()` runs on destroy.

### Events that exist today

| Event | Published by | Heard by |
|---|---|---|
| `PlayerMoveEvent`, `PlayerDodgeEvent`, `PlayerHealEvent`, `PlayerOpenStoreEvent` | `PlayerInputController` | `PlayerController` |
| `MovementInputEvent`, `InventoryInputEvent` (enable/disable) | player states (`Enter()`) | `PlayerInputController` |
| `PlayerHealthChangedEvent` | `PlayerController` | `UiManager` (subscribes in `Awake`) |
| `PlayerAmmoChangedEvent` | `PlayerController` | `PlayerWeapon` |
| `PlayerModeChangedEvent` | `PlayerController` | nobody yet (see Traps) |
| `BuildingDamagedEvent`, `BuildingDiedEvent` | `BuildingLogic` | nobody yet |
| `EnemyOutpostClearedEvent` | `EnemyOutpost` | `TutorialManager` |
| `CollisionItemExchangeEvent` | `Item.CheckCollision` | every `InventorySlot` |
| `AudioClipEvent` | `UiManager` | `AudioManager` |

Source for the table: the `Subscribe`/`Publish` search over `Assets/Scripts`. "Nobody yet" means no
`Subscribe<T>` for that type exists in the code today.

### Two mechanisms side by side

The refactor to events is unfinished. These plain C# events still exist and are still used:
`InventoryManager.onInventoryChange`, `CurrencyManager.onCurrencyChange`, `UpgradeManager.onUpgradesChanged`,
`ZoneManager.onZoneUnlock`, `PlayerController.OnPlayerDodge`, `PlayerWeapon.OnPlayerShoot`,
`InterDimensionalTransporter.OnFuelAdded` / `OnItemSold`, `PlayerStateMachine.stateChanged`.
`TutorialManager` listens to most of them directly
([TutorialManager.cs:196-211](../../Assets/Scripts/Managers/TutorialManager.cs)).

## Invariants

1. **Events announce changes; they do not replace reads.** Decided 2026-07-25 (see
   [Decisions](../6-decisions/Decisions.md)). Do not wrap a per-frame poll in an event.
2. **A subscriber must unsubscribe when it is destroyed** (`OnDestroy` →
   `UnsubscribeFromAllEvents(this)` or `Unsubscribe<T>(this)`), guarded by `EventManager.HasInstance`
   when the subscriber can outlive the manager.
3. **Publish only through `EventManager.Instance?.`** so a missing manager is a no-op, not a crash.
4. **Subscribe in `Awake` if a publisher fires in `Start`.** `PlayerController.Start()` publishes the first
   health value; `UiManager` therefore subscribes in `Awake`
   ([PlayerController.cs:122](../../Assets/Scripts/Player/PlayerController.cs)).

## Traps

- **Destroyed Unity objects are not pruned.** The manager treats a subscriber as gone only when the
  `WeakReference.Target` is a C# null ([EventManager.cs:149-157](../../Assets/Scripts/Managers/EventManager.cs)).
  A destroyed `MonoBehaviour` is still a live C# object until it is garbage collected, so its callback
  keeps firing and can touch destroyed components. The explicit `OnDestroy` unsubscribe is the real
  protection, not the weak reference.
- **Two subscribers never unsubscribe**: `PlayerInputController` (lines 25-26) and `AudioManager` (line 35)
  have no `OnDestroy` cleanup. Today this is harmless because `EventManager` is rebuilt with the scene;
  it becomes a leak the moment either object outlives the manager. Not yet verified at runtime.
- **Nested publish resets the flag early.** `Publish` sets `_isPublishing = false` when it finishes, even
  if it was itself called from inside another publish ([EventManager.cs:170](../../Assets/Scripts/Managers/EventManager.cs)).
  An unsubscribe that happens in the outer publish after the inner one finishes then removes the entry
  immediately instead of deferring it. No known bug from this yet.
- **`UnsubscribeFromAllEvents` uses reflection** (`GetMethod("Unsubscribe").MakeGenericMethod`,
  [EventManager.cs:89](../../Assets/Scripts/Managers/EventManager.cs)). Do not call it per frame.
- **The `CollisionItemExchangeEvent` fan-out is deliberate and order-sensitive.** Every slot hears the
  drop and the first one to call `item.MarkClaimed()` wins; see [inventory-slots.md](inventory-slots.md).
- A stack overflow from event/UI feedback has happened before: inventory change → objective text update →
  UI change → inventory event. The fix was a dirty flag processed in `Update`
  ([TutorialManager.cs:107-112](../../Assets/Scripts/Managers/TutorialManager.cs)). Keep handlers that
  touch UI from publishing the event they are handling.
