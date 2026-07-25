# History Index

A lightweight, reverse-chronological running index of work sessions. Each entry summarises
what was **Investigated / Decided / Changed / Follow-ups**. Deep technical detail lives in the
dated per-topic files alongside this one (e.g. `2026-07-20_stack_overflow_unicode_and_building_ports.md`)
and in `docs/decisions/`; this file links out rather than duplicating them.

---

## 2026-07-25 — Event-system refactor: PlayerController / BuildingLogic + dead-event cleanup

### Investigated
- Full audit of every `public` field / reference on a singleton or MonoBehaviour touched by 5+
  external call sites. Found ~13 hard-reference clusters; player health was the worst (30+ direct
  read/write sites on a `public int Health`).
- Mapped the custom event system end-to-end: `IEvent` marker + `EventManager`
  (`Assets/Scripts/EventSystems/`), a WeakReference-based pub/sub singleton.
- Inventoried every event class for live publishers/subscribers. Found four *subscribed but never
  published* events and a large block with zero references in either direction.

### Decided
- **Events are for "notify me when X changed"; they are not a replacement for polling reads.**
  A turret checking `Health > 0` while targeting, or `GridManager.tileSize` in per-frame math,
  gets tightened encapsulation (private setter + public getter), not an event wrapper.
- **Serialized state keeps a `[SerializeField] private` backing field.** The original plan called
  for converting `maxHealth`/`ammoReserve`/`canDodgeRoll`/etc. to `{ get; private set; }`
  auto-properties. Unity does not serialize auto-properties, and `Player.prefab` stores
  `canDodgeRoll: 1` and `ammoReserve: 120` — both differing from the code initialisers. That
  conversion would have silently shipped a locked dodge-roll and 90 starting ammo. Instead each
  keeps its **exact existing field name** as a `[SerializeField] private` field (so the prefab YAML
  still deserializes) and exposes a PascalCase read-only property.
- `BuildingLogic.Health` uses `{ get; protected set; }`, not `private set` — `TurretLogic`,
  `WallLogic` and `EnemySpawnerLogic` all assign it from their `Setup()` overrides.
- Deleted all four subscribed-but-never-published events rather than wiring them up.
- Scope limited to the two highest-blast-radius clusters; the other 11 are logged as follow-ups.

### Changed

**Shared interface**
- `Assets/Scripts/Utils/IHealth.cs` — `int Health { get; set; }` → `{ get; }`; removed
  `ChangeHealth(int, int)` entirely (all three implementers had a confirmed-dead empty body).

**New events** (constructor-payload style, matching `EnemyOutpostClearedEvent`)
- `Assets/Scripts/EventTypes/PlayerEvents/` — `PlayerHealthChangedEvent`, `PlayerAmmoChangedEvent`,
  `PlayerModeChangedEvent`
- `Assets/Scripts/EventTypes/BuildingEvents/` — `BuildingDamagedEvent`, `BuildingDiedEvent`

**Player**
- `Player/PlayerController.cs` — six stats moved to `[SerializeField] private` + read-only
  properties (`MaxHealth`, `DamageReductionFactor`, `HealthPacksCount`, `AmmoReserve`,
  `CanDodgeRoll`, `CurrentMode`); `Health` → `{ get; private set; }`; added `IsAlive`. New mutation
  API: `ApplyArmorUpgrade`, `GrantHealthPacks`, `AddAmmoReserve`, `ConsumeAmmoReserve`,
  `UnlockDodgeRoll`, `ApplyStaminaBoost`. `TakeDamage`/`UseHealthPack` publish
  `PlayerHealthChangedEvent` instead of calling `UiManager.UpdateHp` directly; `Start()` publishes
  initial health + ammo; `ToggleGameMode()` publishes `PlayerModeChangedEvent`. Deleted the
  zero-subscriber `ModeChangedAction`/`OnModeChanged` delegate. Added
  `UnsubscribeFromAllEvents(this)` to `OnDestroy()` (was missing entirely).
- `Managers/UiManager.cs` — subscribes to `PlayerHealthChangedEvent` in **`Awake()`** (not `Start()`
  — `PlayerController.Start()` publishes the seed value and Unity gives no Start-to-Start ordering
  guarantee); added `OnDestroy()` unsubscribe; removed the direct `UpdateHp` reach-in.
- `Managers/GameManager.cs` — removed its direct `UiManager.UpdateHp(...)` call.
- `Managers/UpgradeManager.cs`, `Ui/StoreUi/UpgradeShopItem.cs` — every direct stat mutation now
  goes through the new `PlayerController` methods; redundant `UpdateHp`/`UpdateStamina`/
  `UpdateAmmoUI` calls at those sites removed.
- `Weapons/PlayerWeapon.cs` — subscribes to `PlayerAmmoChangedEvent` → `UpdateAmmoUI()`; `Reload()`
  uses `ConsumeAmmoReserve(toLoad)`; added `OnDestroy()` unsubscribe.

**Buildings**
- `Placeables/BuildingLogic.cs` — `isEnemyOwned` → `[SerializeField] private` + `IsEnemyOwned`;
  added `IsAlive` and `virtual SetEnemyOwned(bool, EnemyOutpost = null)`; `TakeDamage()` publishes
  `BuildingDamagedEvent`, `Die()` publishes `BuildingDiedEvent`. The existing direct
  `BuildingManager.NotifyBuildingDamaged()` and `outpost.RemoveBuilding(this)` calls stay — those
  are same-subsystem, and the events are an additive channel, not a replacement.
- `Placeables/TurretLogic.cs` — overrides `SetEnemyOwned` to also set `turretWeapon.isEnemyFired`
  (null-guarded: the enemy-spawn path sets ownership *before* `Setup()` resolves the weapon).
- `Managers/EnemyOutpostManager.cs` — direct `isEnemyOwned` writes → `SetEnemyOwned(...)`; deleted
  the `is TurretLogic turret` special case, now handled by the override.
- `Placeables/{EnemySpawnerLogic,Furnace,Miner}.cs`, `Weapons/TurretWeapon.cs`,
  `Managers/PlacementManager.cs` — reads migrated to `IsEnemyOwned`.
- `Weapons/Projectiles/{BaseProjectile,EnemyExplosiveProjectile,EnemyProjectile,PlayerExplosiveProjectile}.cs`,
  `Npcs/Enemies/Cartel/CartelMember.cs` — `building.Health > 0` → `building.IsAlive`.

**Dead-event cleanup**
- Removed `IEvent<T>` (marker `IEvent` kept).
- Deleted: `FadeTransitionEvent`, `RespawnPlayerEvent`, `TransformPassThroughEvent`,
  `PlayerInteractEvent`/`PlayerInteractInputEvent`, root `InventoryItemEvent`, plus the four
  subscribed-but-never-published events (`GlobalVolumeEvent`, `StopAudioEvent`, `DialogueEvent`,
  `SceneChangeEvent`) and their subscription sites in `GlobalVolumeController.cs`,
  `AudioManager.cs`, `DialogueManager.cs` (handler removed; its direct-call API already covers it)
  and `PlayerInputController.cs`.
- Removed the dead `PlayerPlaceEvent`/`PlayerRemoveEvent`/`PlayerRotateEvent`/`PlayerNextItemEvent`/
  `PlayerPreviousItemEvent` classes from `PlayerInputEvents.cs` and their `Publish` calls, plus the
  orphaned `HandleInputToggleEvent` + `FindAction` helper. Input System **bindings were left intact**.
- Stripped the now-dangling `using EventTypes.InventoryEvents;` / `using EventTypes.InputEvents;`
  boilerplate from 114 files — those namespaces contained only deleted types, so leaving the
  directives would have broken every file.
- Removed the `EventTypes/InventoryEvents/`, `EventTypes/InputEvents/` and
  `EventTypes/PuzzleEvents/BlackJack/` folders outright (`AddItemEvent`, the duplicate
  `InventoryItemEvent`, `InputToggleEvent`, and the three BlackJack events — no BlackJack feature
  exists anywhere in the codebase), along with their `.meta` files.

### Verification
- Compiled the full `Assembly-CSharp` source set with Unity 6000.3.10f1's Roslyn
  (`Editor/Data/DotNetSdkRoslyn/csc.dll`) against the editor's engine assemblies: **0 errors**.
- Not yet playtested in the editor — see follow-ups.

### Follow-ups
- **Playtest**: damage/heal the player, buy armor/health/ammo/dodge/stamina upgrades, fire+reload
  (ammo UI), toggle build/combat mode, destroy a player building, capture/lose an outpost building
  (`isEnemyOwned` flip and turret `isEnemyFired` follow-through).
- **11 remaining hard-reference clusters, out of scope this pass**: TutorialManager flags;
  GameManager, GridManager, UiManager, UpgradeManager, DayNightManager and ZoneManager singleton
  public fields; BuildingUiManager hover state.
- **`PlacementManager` / `HotbarManager` poll raw `Input` directly** rather than using the Input
  System actions that are still bound in `PlayerInputController`. Migrating them is a separate
  change; until then those five input handlers are intentionally empty.
- **`PlayerController.OnPlayerDodge`** is still a plain C# event (one working subscriber in
  `TutorialManager`). Left untouched; convert for consistency later.
- `UiManager` has no ammo-counter UI (only a transient `ShowAmmoAlert`), so it does **not** subscribe
  to `PlayerAmmoChangedEvent` — `PlayerWeapon` owns `AmmoUI` and handles that event instead.
