# History Index

A lightweight, reverse-chronological running index of work sessions. Each entry summarises
what was **Investigated / Decided / Changed / Follow-ups**. Deep technical detail lives in the
dated per-topic files alongside this one (e.g. `2026-07-20_stack_overflow_unicode_and_building_ports.md`)
and in `docs/decisions/`; this file links out rather than duplicating them.

---

## 2026-07-25b — Remaining hard-reference clusters: manager encapsulation

Follow-on to the entry below, which deferred 11 clusters. Re-audited them from scratch rather than
trusting the earlier inventory.

### Investigated
- Enumerated every `public` field on `GameManager`, `GridManager`, `UiManager`, `UpgradeManager`,
  `DayNightManager`, `ZoneManager`, `BuildingUiManager`, `TutorialManager`, then counted external
  reference sites and, separately, external **write** sites.
- The "11 clusters" turned out to be far smaller than the original framing suggested:
  - **`BuildingUiManager` hover state was already done** — `HoveredBuilding`/`HoveredNode`/
    `HoveredItem`/`HoveredInteractable` are already `{ get; private set; }`. No work needed.
  - **Only 3 genuine cross-subsystem writes existed**, all *into* `DayNightManager`:
    `isTutorialActive` (5 sites), `timeRemaining` (2), `raidEnemyReduction` (1).
  - 3 further writes were same-subsystem (tutorial states writing their own `TutorialManager`
    flags), plus 1 from an Editor tool (`GrassDirtBlendEditor` → `GameManager.OreTileMap`).
  - The remaining ~23 fields were **read-only polling** — `MainTileMap` (12 reads),
    `ActiveEnemies` (15), `playerController` (11), `tileSize` (8), `resourceNodeDefinitions` (6)…

### Decided
- Same split as the previous pass: writes get methods, polling reads get tightened encapsulation.
- **`[SerializeField] private` keeping the exact field name, plus a PascalCase getter.** This keeps
  every scene/prefab YAML key valid, so no `[FormerlySerializedAs]` and no scene re-serialization.
  `Gamemanager.prefab` alone holds `minPatchSize: 6` / `maxPatchSize: 12` (code defaults 3/7), 8
  `resourceNodeDefinitions` and 4 `allBuildings` references — an auto-property conversion would
  have silently dropped all of it, exactly the trap avoided last pass.
- **Deliberately left alone**: the 9 already-PascalCase public fields (`MainTileMap`,
  `BuildingTileMap`, `OreTileMap`, `Plates`, `ActiveEnemies`, `Hearts`, `StorePanel`, `StatsPanel`,
  `GameOverPanel`). Encapsulating them requires renaming the backing field, which needs
  `[FormerlySerializedAs]` and rewrites the scene/prefab files. Deferred as a follow-up.
- `TutorialManager`'s two state-written flags got explicit `Mark*` methods rather than staying raw
  public fields — a raw cross-class field write is what this refactor targets regardless of
  subsystem, unlike `BuildingManager.NotifyBuildingDamaged()`, which was already a method call.

### Changed
- `Managers/DayNightManager.cs` — all 9 cycle/raid fields to `[SerializeField] private` + getters
  (`DayDuration`, `CurrentDay`, `CurrentPhase`, `IsTutorialActive`, `RaidEnemyReduction`,
  `TimeRemaining`). New `SetTutorialActive(bool)`, `SetTimeRemaining(float)`,
  `AddRaidEnemyReduction(int)`; 8 external call sites in `TutorialManager.cs`,
  `SetupAutomationTutorialState.cs` and `UpgradeManager.cs` migrated off direct writes.
- `Managers/GameManager.cs` — `mainCamera`, `allBuildings`, `sellerTile`, `finiteOres`,
  `resourceNodeDefinitions`, `minPatchSize`, `maxPatchSize`, `patchSpawnChance`, `playerController`
  encapsulated behind getters.
- `Managers/GridManager.cs` — `center`, `tileSize`, `gridSize` → `Center`, `TileSize`, `GridSize`.
- `Managers/ZoneManager.cs` — `zoneSizeInTiles`, `initialUnlockCost`, `costIncreasePerZone`,
  `mainCamera` encapsulated.
- `Managers/UpgradeManager.cs` — `allUpgrades`, `activeUpgradesInShop` → `AllUpgrades`,
  `ActiveUpgradesInShop`.
- `Managers/UiManager.cs` — `currentCurrency` → `CurrentCurrency`.
- `Managers/TutorialManager.cs` — all 14 progression flags plus `introDialogue`/`combatDialogue`
  encapsulated; added `MarkOutpostCleared()` and `MarkAutomaticSaleSubscribed()`. All 11 call sites
  across `StateMachine/TutorialStates/` migrated.

### Verification
- Full Roslyn compile: **0 errors**.
- **Serialization audit**: extracted every serialized YAML key from each manager's scene/prefab
  block and confirmed it still maps to a field of the same name in code. All accounted for
  (`detachFromParent` and `currentStateName` resolve to the `SingletonBase` / `BaseStateMachine`
  base classes).
- Editor scripts confirmed to reference none of the renamed members.
- Not playtested.

### Follow-ups
- The 9 PascalCase public fields above, if the scene/prefab churn from `[FormerlySerializedAs]` is
  acceptable.
- `GameManager.OreTileMap` is written by `Assets/Scripts/Editor/GrassDirtBlendEditor.cs`; it must
  stay settable (or gain an editor-only setter) if it is ever encapsulated.
- Exposing `AllUpgrades` / `ActiveUpgradesInShop` / `Plates` as `List<T>` getters still lets callers
  `.Add()` into them. `IReadOnlyList<T>` would close that if it ever matters.

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
- ~~11 remaining hard-reference clusters~~ — addressed in the 2026-07-25b entry above.
- **`PlacementManager` / `HotbarManager` poll raw `Input` directly** rather than using the Input
  System actions that are still bound in `PlayerInputController`. Migrating them is a separate
  change; until then those five input handlers are intentionally empty.
- **`PlayerController.OnPlayerDodge`** is still a plain C# event (one working subscriber in
  `TutorialManager`). Left untouched; convert for consistency later.
- `UiManager` has no ammo-counter UI (only a transient `ShowAmmoAlert`), so it does **not** subscribe
  to `PlayerAmmoChangedEvent` — `PlayerWeapon` owns `AmmoUI` and handles that event instead.
