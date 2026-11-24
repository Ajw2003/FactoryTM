# Session 5: Workspace Memory, Context Persistence, & Architecture Guidelines

This document records the design decisions and changes made during Session 5 (current conversation).

---

## 1. Context & Motivation
To improve the continuity and execution efficiency of AI agents (like Antigravity) across separate conversation boundaries, the developer requested a way to provide persistent context. The agent needs to quickly inherit guidelines, namespace structures, design patterns, and past architectural decisions without requiring manual copy-pasting of instructions at the start of every session.

---

## 2. Key Decisions & Implementation Details

### Setup of Project-Scoped Rules (`.agents/AGENTS.md`)
* **Auto-Discovery:** Leveraged Antigravity's auto-discovery directory (`.agents`) at the root of the workspace.
* **AGENTS.md Definition:** Created and structured `.agents/AGENTS.md` to feed guidelines directly into the system prompt of future agents.
* **Defined Patterns:**
  - **Namespaces:** Clear layout of EventSystems, StateMachine, and Singleton namespaces.
  - **Singletons:** Guidelines on inheriting from `SingletonBase<T>` and scene persistence.
  - **Event Decoupling:** Explicit structure of the Pub/Sub `EventManager` (publish, subscribe, unsubscribe syntax).
  - **State Machines:** Requirements for `BaseStateMachine` and `IState` implementations.
  - **ScriptableObjects:** Separation of data structures (`Assets/Scripts/Data/`) and instanced assets (`Assets/ScriptableObjects/`).

### Documentation and History Repository (`docs/decisions/`)
* **Chronological Knowledge Base:** Set up `docs/decisions/` inside the workspace to hold detailed session files summarizing past features and refactoring tasks.
* **Agent Context Loop:** Included rules in `.agents/AGENTS.md` to instruct future agents to review these markdown logs before starting any task, ensuring seamless continuity of context.

---

## 3. Associated Commits & Files
* `b912a43` - Add markdown and `.agents` folder for use with Antigravity.
* `.agents/AGENTS.md` - Workspace rules and project conventions.
* `docs/decisions/` - Chronological database of sessions and design choices.
