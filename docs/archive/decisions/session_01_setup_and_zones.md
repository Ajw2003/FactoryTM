# Session 1: Map Expansion & Zone Configuration

This document records the design decisions and changes made during Session 1 (approx. 9 days ago in commit history).

---

## 1. Context & Motivation
Initially, the map structure of FactoryTM was static, which restricted player progression. To introduce a sense of scaling and exploration, the map was divided into unlockable "Zones". The player needed a mechanism to configure, purchase, and navigate these expanded grid zones.

---

## 2. Key Decisions & Implementation Details

### Map Expansion & UI Changes
* **Zone Unlocking:** Replaced the legacy map expansion menu with a dedicated Zone Unlocking UI overlay.
* **Scale Configurator:** Added system configurations to dynamically set the default number of zones owned at the start of a session.
* **Control-Scroll Zooming:** 
  - To prevent players from losing spatial awareness as the factory grows, camera controls were enhanced.
  - If the player owns 4 zones arranged in a square structure, holding the `Control` key and scrolling with the mouse wheel zooms the camera out to a bird's-eye view, providing a macro view of the automated layout.

---

## 3. Associated Commits
* `ac7b18a` - Some scene changes and preliminary story ideas.
* `40a65fb` - Change UI for map expansion / zone unlocking.
* `e4098b9` - Add system to configure default number of zones owned, and zoom out with Control+Scroll if 4 zones are owned forming a square.
* `60af8ff` - More fixes and begun balancing.
* `0219435` - Initial bug fixes.
