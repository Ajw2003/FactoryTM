# Bug Implementation Plan

## 1. Unicode Warnings Fix

### Layman Summary
When our Python script ran to fix code spacing, it inadvertently changed the hidden "line endings" of the files to Windows format. Unity expects a different format (Mac/Linux standard) for its prefabs and throws a warning when it sees the Windows format. We will update the Python script so it no longer alters these line endings when saving the files.

### Technical Specs
*   **Target Script:** `scratch\fix_spacing.py`
*   **Target Logic:** Any file reading or writing operations using the `open()` function.
*   **Implementation Steps:**
    *   Update all calls to `open()` (both `'r'` for reading and `'w'` for writing) to include the `newline=''` parameter.
    *   Example: `open(file, 'w', encoding='utf-8', newline='')`.
    *   This disables Python's Universal Newlines translation, ensuring that native LF (`\n`) characters aren't converted to CRLF (`\r\n`) on Windows, preserving the original line endings.

### Edge Cases
*   If any file was *already* converted to CRLF, this fix will just preserve it as CRLF. To fully fix already corrupted files, they may need their line endings manually normalized in an editor or via a separate script pass, but this change stops the Python script from causing the issue going forward.

---

## 2. Stack Overflow Circular Event Loop Fix

### Layman Summary
The game is crashing because of an infinite loop between the tutorial system and the inventory system. When the inventory changes, the tutorial updates the UI. However, updating the UI sometimes triggers another inventory update event, which tells the tutorial to update the UI again, repeating forever. We will break this loop by ensuring UI updates don't immediately trigger further changes in the same frame.

### Technical Specs
*   **Target Scripts:** `TutorialManager.cs` (and any related state scripts like `BuyFirstWeaponTutorialState.cs`).
*   **Target Methods:** `UpdateObjectiveText()`, `UpdateObjectivesPanelText()`, and event handlers for `InventoryManager.onInventoryChange` / `CurrencyManager.onCurrencyChange`.
*   **Implementation Steps:**
    *   **Approach (Dirty Flag):** Instead of calling `UpdateObjectiveText()` synchronously inside the event handlers for inventory and currency changes, we will introduce a dirty flag (e.g., `private bool _needsObjectiveUpdate = false;`).
    *   In the event listeners subscribed to `onInventoryChange` and `onCurrencyChange`, we simply set `_needsObjectiveUpdate = true;`.
    *   In the `Update()` method of `TutorialManager`, we check if `_needsObjectiveUpdate` is true. If so, we call `UpdateObjectiveText()` (or `UpdateObjectivesPanelText()`) and then reset `_needsObjectiveUpdate = false;`.
    *   This completely decouples the synchronous event chain and guarantees the UI only updates once per frame, breaking any potential circular call stack.

### Edge Cases
*   **Frame Delay:** Updating the UI in `Update()` introduces a maximum delay of one frame compared to updating it synchronously. This is visually imperceptible to the player and perfectly safe for UI updates.
*   **State Transitions:** If state transitions (like in `BuyFirstWeaponTutorialState.CheckTransitions`) rely on synchronous updates, they might need to be evaluated in `Update()` alongside the UI updates. The dirty flag approach naturally accommodates this by grouping the evaluation and update together once per frame.
