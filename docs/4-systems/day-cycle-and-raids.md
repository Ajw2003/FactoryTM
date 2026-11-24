# Day cycle and raids

## What it owns

The roguelike loop: a timed Day, an Evening raid, an Upgrade pause, then the next Day. It owns the
phase enum, the countdown, raid difficulty scaling, and the moment the game freezes for upgrade
selection. It does not own enemy behaviour (`CartelMember`), enemy outposts (`EnemyOutpostManager`), or
what the upgrades do (`UpgradeManager`).

## How it works

Three phases ([DayNightManager.cs:18-23](../../Assets/Scripts/Managers/DayNightManager.cs)):

1. **Day** — `timeRemaining` counts down from `dayDuration` (default 90 s). At zero, `StartEveningRaid()`.
2. **Evening** — raises the night-vision light, asks `RaidManager` to start a raid. Difficulty scales with
   the day number: `enemies = max(1, baseEnemiesPerWave + (day-1)*2 - raidEnemyReduction)` and
   `waves = wavesPerRaid + day/3` ([DayNightManager.cs:157-158](../../Assets/Scripts/Managers/DayNightManager.cs)).
3. **UpgradePhase** — when the raid is over and no raid enemies are alive, set `Time.timeScale = 0` and
   open the upgrade picker ([DayNightManager.cs:165-176](../../Assets/Scripts/Managers/DayNightManager.cs)).
   `StartNextDay()` is called by the picker, resets the phase and restores `timeScale = 1`.

`RaidManager` runs the raid as a coroutine, either wave based or continuous, spawning prefabs on a circle
around the player ([RaidManager.cs:105-192](../../Assets/Scripts/Managers/RaidManager.cs)). Spawned
enemies are tagged `isRaidEnemy = true`; that tag is how the day manager knows the raid is over.

"Raid finished" is the conjunction of four things
([DayNightManager.cs:130-136](../../Assets/Scripts/Managers/DayNightManager.cs)): enemies have appeared at
least once (`raidHasBegun`), `RaidManager.isRaidActive` is false, zero raid enemies remain, and at least
3 s of unscaled time have passed since the raid began.

The player dying ends the run through `UiManager.ShowGameOver()`
([PlayerController.cs:549](../../Assets/Scripts/Player/PlayerController.cs)). That is the only caller
of `ShowGameOver` in the code. The older notes claim that losing the IDT also ends the run; no code
path for that was found, so treat it as not built.

## Invariants

1. **While `isTutorialActive` is true the Day countdown does not run**
   ([DayNightManager.cs:97-100](../../Assets/Scripts/Managers/DayNightManager.cs)). It defaults to `true`
   (line 31). Only the tutorial finishing, or `PlayTutorial = 0` in `PlayerPrefs`, releases it.
2. **Raid waits use real time** (`WaitForSecondsRealtime`) so the upgrade pause cannot freeze a raid
   mid-spawn.
3. **Only `DayNightManager` writes the cycle fields.** Other systems go through `SetTutorialActive`,
   `SetTimeRemaining`, `AddRaidEnemyReduction` (decided 2026-07-25).
4. Anything that sets `Time.timeScale` must restore it. Four places write it: `DayNightManager` (169, 192),
   `PauseManager` (71, 92, which remembers the original) and `UiManager` game over (172, 179, 185).

## Traps

- **A raid that never "begins" never ends.** `raidHasBegun` flips only when a raid enemy exists in
  `GameManager.ActiveEnemies`. If enemy prefabs fail to spawn or are not added to that list, the Evening
  phase never reaches the upgrade pause and the game sits at night forever. `RaidManager.StartRaid` logs an
  error if no prefabs are assigned, and returns without setting `isRaidActive`, which the day manager reads
  as "finished" but it still waits for `raidHasBegun`.
- **Pause and upgrade pause both use `timeScale = 0`.** Pausing during the upgrade phase and resuming
  restores whatever `originalTimeScale` the pause saw, which is 0. Not yet verified in play.
- `nightVis.intensity` is set directly and `sun` is dereferenced in `StartNextDay`; both are scene
  references. A scene without them throws on the first night.
- A tutorial stack overflow once came from `ChangeState` assigning `CurrentState` *after* calling
  `Exit()`. Do not reorder `BaseStateMachine.ChangeState` ([BaseStateMachine.cs:22-27](../../Assets/Scripts/StateMachine/BaseStateMachine.cs)).
