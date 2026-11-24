# Handoff — AjsAgent/project-docs

## What the work is for

Put FactoryTM's documentation into the six-tier structure (landing, roadmap, state, systems, today, decisions) so
anyone starting a session can find where the project stands. No game code was changed.

## Where it stands

Committed on branch `AjsAgent/project-docs`: seven system docs, roadmap, project state, decision log, today's
plan, `CHANGELOG.md`, and the old `history/` and `docs/decisions/` moved to `docs/archive/`. All 138 links in
the new docs resolve (checked with a script).

## The next step

Weights are approved (M8 visual overhaul now 10%). Define "similarly detailed" for sprites, or ask for a draft style sheet (page: `docs/generated/roadmap-review.html`). The IDT rule is decided:
destroying it ends the run, and it still has to be built. Then merge by pull request; do not commit to `main`.

## What is unverified

A Unity batch compile gave 0 `error CS`, and a scripted play-mode run showed 0 errors and 2 engine warnings.
No playthrough was done, so each roadmap acceptance check is unrun and the state page shows 0% verified.
The review page was not checked at 375px or in dark mode.

## Issue numbers

#1 UI and UX overhaul (diegetic); #2 beginner-friendly mobile edition with a tower-defense spin. Milestones M7
and M9 in the roadmap map to them; their acceptance criteria are not yet defined.
