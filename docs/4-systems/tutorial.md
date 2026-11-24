# Tutorial

## What it owns

The scripted first-run: a state machine of eleven steps (plus start and completed states) that teaches mining, fuelling and selling, buying a
weapon, clearing an outpost, automating with a miner and belts, and surviving the first raid. It owns the
objective text, the progression flags the steps read, and the decision to hold the day clock still while it
runs. It does not own dialogue rendering (`DialogueManager`), the store (`StoreUiScript`) or zone unlocking
(`ZoneManager`), but it calls into all three.

## How it works

- `TutorialManager` is a `BaseStateMachine`, with one class per step under
  `Assets/Scripts/StateMachine/TutorialStates/`. The step ids are the `TutorialState` enum:
  `NotStarted → BriefIntro → MineCoalManually → FuelDCT → SellOtherOres → BuyFirstWeapon → BuyAmmoHealth →
  DestroyEnemyOutpost → BuyMinerConveyors → SetupAutomation → EarnAndExpand → DefendFirstRaid → Completed`
  ([TutorialManager.cs:20-35](../../Assets/Scripts/Managers/TutorialManager.cs)).
- **Start.** `Start` reads `PlayerPrefs "PlayTutorial"` (default 1). If it is 0 the machine jumps to
  `Completed` and releases the day clock. Otherwise it subscribes, waits 1.5 s, forces the day clock to wait
  (`SetTutorialActive(true)`), sets money to 0, and enters `BriefIntro`
  ([TutorialManager.cs:160-175, 253-270](../../Assets/Scripts/Managers/TutorialManager.cs)).
- **What it listens to**: `PlayerWeapon.OnPlayerShoot`, `PlayerController.OnPlayerDodge`,
  `EnemyOutpostClearedEvent`, `InventoryManager.onInventoryChange`, `CurrencyManager.onCurrencyChange`, and
  later the IDT's `OnItemSold` / `OnFuelAdded`
  ([TutorialManager.cs:191-213](../../Assets/Scripts/Managers/TutorialManager.cs)).
- **Objective text is dirty-flagged.** The inventory and currency handlers only set `_needsObjectiveUpdate`;
  `Update` rebuilds the text once per frame. Two later steps (`EarnAndExpand`, `DefendFirstRaid`) refresh
  every frame instead ([TutorialManager.cs:177-189](../../Assets/Scripts/Managers/TutorialManager.cs)).
- **Steps reach back into the manager** through read-only flag properties and three `Mark*` methods
  (`MarkOutpostCleared`, `MarkAutomaticSaleSubscribed`, `MarkFuelSubscribed`); they do not write the flags
  directly.
- **Side effects of steps.** `SetupOutpostPhase` force-unlocks three zones and spawns the tutorial outpost (patch size 4)
  at the centre cell plus (16,16) ([TutorialManager.cs:292-317](../../Assets/Scripts/Managers/TutorialManager.cs)).
  `UnlockWeaponsInShop` adds weapon upgrades to the shop list. `IsStoreLocked()` keeps the store closed for the
  first three steps.

## Invariants

1. **The tutorial must always release the day clock** (`SetTutorialActive(false)` or `CompleteTutorial`); the
   day manager defaults to "tutorial active" ([DayNightManager.cs:31](../../Assets/Scripts/Managers/DayNightManager.cs)).
2. **Subscribe and unsubscribe are paired.** `UnsubscribeEvents` mirrors `SubscribeEvents` and runs in
   `OnDestroy`. A new subscription added to one must be added to the other.
3. **Handlers must not change the thing they are listening to**, or the inventory/currency feedback loop
   returns (see Traps).
4. **Dodge roll is not the tutorial's to grant** (see [player-and-states.md](player-and-states.md)).

## Traps

- **The inventory ↔ objective loop.** Updating the objective text could trigger UI that changed the
  inventory, which re-fired the handler, until the stack overflowed. The dirty flag breaks it. The older
  `history/bug_*` notes describe this; the fix is the one above.
- **`TutorialManager` does not use `SingletonBase`.** It has its own static `Instance` and detaches from its
  parent in `Awake` ([TutorialManager.cs:129-141](../../Assets/Scripts/Managers/TutorialManager.cs)).
- **The "4 remaining" outpost objective was hard-coded**; the plan was a percentage of spawner health
  (`history/bug_implementation_plan.md` item 6). Whether it has been changed is not checked in this pass.
- **`OnItemSold` may be raised with `null`** when the IDT sells from its drag slot. `HandleAutomaticSale`
  today never reads its `item` argument ([TutorialManager.cs:385](../../Assets/Scripts/Managers/TutorialManager.cs)),
  so it is safe; keep it that way or add a null check.
- **Two bypassed dialogue assets** (`introDialogue`, `combatDialogue`) are still serialized but unused
  ("Dialogue Assets (Bypassed)" header, line 42).
- **`PlayTutorial` is written from four places** (`TutorialManager` lines 454, 505, 519 and the pause menu
  toggle in `PauseManager` line 129); a restart path that forgets to reset it launches straight into a
  completed tutorial.
- The planned rework (an AI-narrated story, itemised objectives that fire events) is in
  [Roadmap](../2-roadmap/Roadmap.md); none of it exists yet.
