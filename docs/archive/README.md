# Archive

Documents that were correct when written and are now inert. **Nothing here describes current behaviour.**
Moved on 2026-11-24 (with `git mv`, so history follows the files). Their real decisions were transcribed into
[Decisions](../6-decisions/Decisions.md).

## `decisions/` (was `docs/decisions/`)

| File | Why it is inert |
|---|---|
| `project_summary.md` | Mixes still-true architecture with claims that were never built (IDT destroyed = game over) and a "current backlog" from July. Replaced by [landing](../1-landing/README.md) and the [system docs](../4-systems/README.md). |
| `project_phases.md` | A task list from July with ticks that were never kept current. Replaced by the [Roadmap](../2-roadmap/Roadmap.md) and `Assets/Notes/ToDo.txt`. |
| `session_01` … `session_05` | Dated session logs ("approx. 9 days ago") from 2026-06-24 to 2026-06-26. Decisions copied to Decisions.md; relative dates replaced with git dates. |

## `history/` (was top-level `history/`)

| File | Why it is inert |
|---|---|
| `history.md` | Index of the 2026-07-25 refactor sessions. Decisions copied to Decisions.md; the field-by-field change lists were a record of one diff, not a description of the system. |
| `2026-07-20_stack_overflow_unicode_and_building_ports.md` | Describes the port rule and the stack overflow fix. Both now live in [buildings-and-conveyors.md](../4-systems/buildings-and-conveyors.md) and [player-and-states.md](../4-systems/player-and-states.md). |
| `2026-07-20b_chest_inventory_and_player_inventory_tab.md` | Describes the dictionary-based Chest and `playerResources`, replaced on 2026-07-22 by the slot model. |
| `bug_implementation_plan.md`, `bug_research_log.md`, `implementation_plan.md`, `research_log.md` | Plans and research for fixes dated 2026-07-19. Whether each item was done was not checked in this pass. Still useful as context; not a status. |

## Also left where it is

`scratch/fix_spacing.py` stays in `scratch/`; it is a script, not a document.
