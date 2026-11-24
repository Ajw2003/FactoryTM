# Session 3: Bespoke Building UIs, Manual Selling, & Tutorial Restructuring

This document records the design decisions and changes made during Session 3 (approx. 7 days ago in commit history).

---

## 1. Context & Motivation
Automation games require deep interface feedback. Previously, the interaction with Miners, Smelters, and the central IDT (seller) was generic. The player could not easily track how many resources were loaded or manually load fuel/ore. Additionally, the starting tutorial was prone to softlocks and did not introduce core mechanics smoothly.

---

## 2. Key Decisions & Implementation Details

### Bespoke Building UIs
* **Dedicated UI Sub-Panels:** Created distinct user interface modules for each functional building:
  - **Miner UI:** Displays current fuel capacity and extraction rates.
  - **Furnace UI:** Displays slot for fuel, slot for raw ore, and smelting progress.
  - **IDT / Seller UI:** Re-anchored to the right side of the screen, detailing exported items and profit logs.
* **Manual Interaction:** Allowed players to walk up to these buildings, press `E` to open their custom overlay, and manually deposit or withdraw items.

### Manual Ore Selling
* **Direct Selling Option:** Implemented direct seller interactions so that manually mined coal or iron could be inputted directly into the IDT from the player's inventory, ensuring the player can make starting capital before automating.

### Tutorial Stabilization
* **Starting Zone Safeguards:** Prevented enemy outposts and raids from generating/occurring during the starting tutorial.
* **Softlock Fixes:** Resolved issues where the player would get stuck if they refueled the IDT too early or spent resource items prematurely.

---

## 3. Associated Commits
* `299a992` - Massive changes and bespoke UI for each building which has an action (mining, selling, smelting).
* `3f25f2a` - Further tutorial logic, ensure enemy outposts don't spawn in starting zone, add manual selling.
* `5207f5c` - Anchor UI to right side for IDT and fix tutorial progression after refueling.
* `c801eb8` - Make selling ore and adding fuel work and look better.
* `1505c99` - Change healing key and fix states not properly preventing input.
