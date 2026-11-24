# Roadmap — what 0–100% means

FactoryTM is "done" when a new player can finish a run: mine and automate resources, defend the IDT through
raids, pick upgrades, and lose or win cleanly, on the platform issue #2 targets. This page defines the
milestones and, for each, the test that proves it is finished. [ProjectState](../3-state/ProjectState.md)
measures against it.

> **Status of this page: drafted 2026-11-24 from the repo, not yet confirmed by the project owner.** The
> milestone list is taken from `Assets/Notes/ToDo.txt`, the archived phase plan and open issues #1 and #2.
> The **weights are proposals**. Confirming or changing them is a Tier 2 change and should be recorded in
> [Decisions](../6-decisions/Decisions.md).

**Rule:** a milestone is done when its acceptance criterion has actually been run and passed, not when the
code exists. Written but never run is not done. The headline percentage is the sum of the weights of
finished milestones.

| # | Milestone | Weight |
|---|---|---|
| M1 | Core factory loop | 15% |
| M2 | Slot-based inventory and building I/O | 10% |
| M3 | Combat, raids and day cycle | 15% |
| M4 | Upgrades, store and balance | 15% |
| M5 | Tutorial and story | 15% |
| M6 | Code architecture | 10% |
| M7 | UI / UX overhaul (issue #1) | 10% |
| M8 | World visuals and feedback | 5% |
| M9 | Beginner-friendly mobile edition (issue #2) | 5% |
| | **Total** | **100%** |

---

## M1 — Core factory loop (15%)

Mine ore, smelt it, move it on belts and sell it through the IDT for money.

**Contains:** resource nodes and miners; furnace; conveyors with rotation; chest; the IDT as the only sink;
fuel for miners, furnaces and the IDT; currency; finite ore (flag exists).

**Acceptance:** from a fresh scene, a player places a miner on coal, feeds it by belt, routes iron through a
furnace and a belt into the IDT, and money rises with no manual steps after placement, for five minutes,
with no belt blocked and no console error. Placing a building on a cell that has a loose belt item is either
allowed or refused with a message, never silently wrong.

## M2 — Slot-based inventory and building I/O (10%)

Every item the player or a building holds lives in an `InventorySlot`, and moves by drag and drop.

**Contains:** `ItemData` for every item and building; hotbar slots; building panels with input/output slots;
left-click takes one, right-click takes the stack; building purchase puts the building in the hotbar.

**Acceptance:** drag one and a stack of each resource between the player, a chest, a furnace, a miner and the
IDT in both directions; counts match on both sides every time; a drop outside any slot returns the stack;
buying a building from the store places it in the hotbar and placing it uses it up.

## M3 — Combat, raids and day cycle (15%)

The roguelike loop: Day, Evening raid, upgrade pause, next Day, with a real chance of losing.

**Contains:** `DayNightManager`, `RaidManager`, enemy outposts, cartel enemies, player weapons and projectiles,
building damage, game over.

**Acceptance:** play five consecutive days. Each raid scales up, ends when its enemies are dead, opens the
upgrade picker, and the next day starts with time running. The player dying shows game over and a restart
works. Destroying the IDT also ends the run.

*Criterion note (2026-11-24):* the last sentence comes from the old project summary and the to-do list, which
both state it as design. The code has no path for it (`ShowGameOver` has one caller, the player's death), so
it is part of this milestone's open work, not something already built. If the owner has dropped that rule,
remove the sentence and record the decision in [Decisions](../6-decisions/Decisions.md).

## M4 — Upgrades, store and balance (15%)

Spending money has meaningful choices, and the curve from day 1 to day 10 is playable.

**Contains:** `UpgradeManager` and the upgrade assets; the store; three-way upgrade choice after raids;
starting store limited to weapons and consumables; essential upgrades offered first; configurable day length;
price, yield and damage tuning; difficulty settings.

**Acceptance:** a first-time player reaches day 5 without softlock; every upgrade type can be bought and has a
visible effect; no upgrade is purchasable before its prerequisites; a written balance sheet of prices, yields,
enemy counts and day lengths exists and was playtested against.

## M5 — Tutorial and story (15%)

A new player is taught the loop without help and has a reason to keep going.

**Contains:** the eleven-step `TutorialManager`; objectives as an itemised list that fires events; the AI
narrator; story text; sections for land expansion, ammo and health, and shooting.

**Acceptance:** three people who have never seen the game finish the tutorial unaided and none hits a
softlock; every step has a short objective; restarting the tutorial from the pause menu works.

## M6 — Code architecture (10%)

Systems talk through events and narrow methods, not by reaching into each other's fields.

**Contains:** the event refactor on branch `EventRefactor`; encapsulation of manager state; removal of dead
events and code; Input System used everywhere; consistent namespaces.

**Acceptance:** no public mutable field is written from another subsystem (a search shows none); every
subscriber unsubscribes on destroy; no use of `UnityEngine.Input`; the project compiles with zero warnings
about missing references; a full playthrough of M1–M3 shows no new console error compared with before the
refactor.

## M7 — UI / UX overhaul, diegetic (10%) — issue #1

Information lives in the world (on machines and the character) instead of floating panels.

**Contains:** a redesign of building panels, store, objectives, dialogue and HUD; spacing standard; larger
text outside post-processing; contrast pass; hotbar names on hover.

**Acceptance:** *not yet defined by the owner.* Placeholder until the design is approved: every screen passes
a 375px-wide and a 1920px-wide check, text is legible with the CRT filter on, and no panel overlaps another.

## M8 — World visuals and feedback (5%)

**Contains:** grass/dirt blend tiles; vegetation disappearing as the planet is stripped; particles and
animation for buildings and processed ore; damage flash; sound for every action.

**Acceptance:** the blend overlay is correct on a regenerated map (no unrotated or missing edge pieces); each
building action has one effect and one sound; the world visibly changes as ore is removed.

## M9 — Beginner-friendly mobile edition (5%) — issue #2

Reframe as a first factory game with a tower-defense spin that runs on mobile.

**Contains:** touch controls; simplified onboarding; mobile build; performance budget.

**Acceptance:** *not yet defined by the owner.* A device and frame-rate target are needed before this can be
checked; see [ProjectState](../3-state/ProjectState.md).
