# Today — 2026-11-24

**Today is a documentation day.** It maps to no roadmap milestone; it exists so the six-tier structure is in
place before the next piece of work starts.

## What was done

- Read the code, git history (255 commits, 2026-05-08 to 2026-07-29), the old `history/` and `docs/decisions/`
  records, `Assets/Notes/ToDo.txt` and the two open issues.
- Wrote tier 4 first (seven system docs), then the roadmap, the state page, the landing page, the decision
  log, and `CHANGELOG.md`.
- Moved `history/` and `docs/decisions/` into `docs/archive/` with a note on why; pointed `.agents/AGENTS.md`
  at the new entry point.

## What was deliberately not done

- No source code was changed.
- Nothing was run in Unity; Unity MCP was not connected. All behaviour claims come from reading code, and
  anything not confirmed by reading is marked "not yet verified".
- Roadmap weights and the M7/M9 acceptance criteria were not invented; they are flagged for the owner.
- `docs/plain/` was not created (a later pass, `house-rules:plain-docs`).

## Surfaced, not today's job

- "Destroy the IDT = game over" is documented everywhere and built nowhere (see
  [ProjectState](../3-state/ProjectState.md)).
- Two `EventManager` subscribers never unsubscribe (`PlayerInputController`, `AudioManager`).
- `PlacementManager` and `HotbarManager` still poll `UnityEngine.Input`.
- `Assets/Scripts.zip` and `Assets/_Recovery` have not been compared with live source.

## What to do next, in order

1. **Confirm the roadmap** (weights, and the IDT game-over rule). Everything else is measured against it.
2. **Define the acceptance criteria for M7 and M9** (issues #1 and #2). Neither can be started honestly until
   a design and a device target exist.
3. **Run the M1 and M3 acceptance checks in the editor** and record a pass or fail in the state page. These
   unblock the headline number, which is 0% only because nothing has been checked.
4. **Finish M6**: unsubscribe the two leaking subscribers, move placement and hotbar to the Input System, and
   decide whether the remaining public fields are worth the scene churn.
5. **Fix the IDT game-over gap** or record the decision to drop it.
