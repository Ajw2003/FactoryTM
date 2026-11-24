# Handoff — AjsAgent/project-docs

## What the work is for

Put FactoryTM's documentation into the six-tier structure (landing, roadmap, state, systems, today, decisions) so
anyone starting a session can find where the project stands. No game code was changed.

## Where it stands

Committed on branch `AjsAgent/project-docs`: seven system docs, roadmap, project state, decision log, today's
plan, `CHANGELOG.md`, and the old `history/` and `docs/decisions/` moved to `docs/archive/`. All 138 links in
the new docs resolve (checked with a script).

## The next step

Review the roadmap: confirm or change the milestone weights, and decide whether "destroying the IDT ends the
run" is still a rule (the code does not do it). Then merge by pull request; do not commit to `main`.

## What is unverified

Nothing was run in Unity. The Editor was not open (only Unity Hub), the Unity MCP connection was refused,
and the `unity` CLI exists but a batch compile was not run. Behaviour claims come from reading code. Each
roadmap acceptance check is unrun, so the state page shows 0% verified.

## Issue numbers

#1 UI and UX overhaul (diegetic); #2 beginner-friendly mobile edition with a tower-defense spin. Milestones M7
and M9 in the roadmap map to them; their acceptance criteria are not yet defined.
