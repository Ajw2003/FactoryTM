# Changelog

What changed in this repo, newest first. One line per change: plain words first, exact detail after.
No releases or tags exist yet; history before 2026-11-24 is summarised from `git log` (255 commits).

## Unreleased

- Adds the six-tier documentation structure and moves the old session logs to `docs/archive/` (`docs/1-landing` to `docs/6-decisions`, 7 system docs).
- Points `.agents/AGENTS.md` at `docs/1-landing/README.md` instead of the archived summary files.

## History

- 2026-07-29 — Fixes the player's hearts not showing after the event refactor (`2e87a28`).
- 2026-07-28 — More event and optimisation work (`bf74abc`).
- 2026-07-25 — Player and building health move to events; dead events and many public fields removed or made private (`3642fcb`, `ab88503`, `82921e4`); merges the grass-blending branch into `StagingMain`.
- 2026-07-23 — Grass and dirt blend tiles, with the ore tilemap using ore textures again (`b761ccd` to `d486cbf`); scenes are tracked.
- 2026-07-20 to 2026-07-22 — Whole inventory rewritten around `InventorySlot` and `ItemData`, with drag and drop; every building uses it (`aa674ab` to `4159da9`). Buildings gain input and output ports; Chest becomes real storage (`03463d1`, `0626359`). Fixes a tutorial stack overflow and unsupported font glyphs (`cc0d4a0`).
- 2026-07-02 — Project-wide namespace clean-up; the to-do list is itemised; `.agents/AGENTS.md` and the first `docs/decisions` records are added for assistant context.
- 2026-06-24 to 2026-06-26 — Zone unlocking with Control-scroll zoom, enemy buildings from ScriptableObjects, damage cracks, a bespoke panel per building, object pooling, tutorial as a state machine, floating text through a pooled singleton.
- 2026-06-16 to 2026-06-21 — Multi-tile buildings, dialogue begun, enemy target switching, currency as floats, hearts for health, stats tracking, the tutorial framework.
- 2026-06-08 to 2026-06-10 — Floating damage numbers; first roguelike days, upgrades and store; mass script import, then a move to the new Input System.
- 2026-05-26 to 2026-06-07 — Weapons, projectiles and enemies that shoot; health interface; rectangle-overlap hit detection.
- 2026-05-08 to 2026-05-24 — First belts, miners, seller and economy; resource nodes and furnaces; zones; grid manager; player movement, hotbar, animated tiles, CRT effect.
