# Bug Research Log

## 1. Unicode Warnings
Review of `scratch\fix_spacing.py` reveals that while it DOES explicitly specify `encoding='utf-8'`, it fails to specify `newline=''` in the `open()` function. On Windows, Python's Universal Newlines mode automatically translates native LF (`\n`) line endings (which Unity prefabs use) into CRLF (`\r\n`) when writing. This corruption of the line endings is what triggers Unity's YAML parser to issue Unicode warnings.

## 2. Stack Overflow Error
Review of `TutorialManager.cs` and `DialogueManager.cs` reveals potential circular event triggers, specifically around the objective tracking and state transitions:
- `TutorialManager` subscribes `UpdateObjectiveText()` to both `InventoryManager.onInventoryChange` and `CurrencyManager.onCurrencyChange`.
- In several state transitions (like `BuyFirstWeaponTutorialState.CheckTransitions`), state changes invoke `Enter()`, which calls `manager.UpdateObjectiveText()`.
- If any UI update or dialogue activation triggered by `UpdateObjectiveText()` (e.g., through layout rebuilding, DOTween, or SetActive(true)) inadvertently triggers an inventory or currency change event, this will fire `onInventoryChange` again, causing an infinite recursive loop leading to a Stack Overflow.
- A thorough check should also be made on `UpdateObjectivesPanelText()` which gets called during `Update()` and inside `UpdateObjectiveText().
