# FactoryTM Project Rules, Context, Requirements

## Requirments

- Always follow the bellow guidelines 
- Ensure solution compiles and project compiles before finishing a task.

## Global Pipeline Respect
- **Global Agentic Pipeline:** You MUST read and respect the rules in the `global-agentic-pipeline` skill (located in the global skills directory) for every action. Do not ignore its constraints and guidelines.



## Project Overview
- FactoryTM is a Unity-based factory automation game.
- Scripting language: C# (Unity 6+ style).

## Architectural Guidelines

### Namespaces & Structure
- Code should reside in appropriate namespaces to maintain clean organization:
  - Event interface: `EventSystems`
  - Event implementations: `EventTypes.*` (e.g., `EventTypes.InventoryEvents` or `Code.Scripts.Interfaces.EventTypes`)
  - State machine base classes: `StateMachine`
  - Singleton base: `Singleton`
  - Event Manager: `Code.Scripts.EventSystems`

### Singletons
- Prefer inheriting from `SingletonBase<T>` (defined in the `Singleton` namespace) rather than implementing ad-hoc singleton patterns, unless a special constructor is required (like in `PlayerController`).
- Set `persistBetweenScenes = true` or `false` in `Awake()` depending on whether the manager should survive scene transitions.

### Event-Driven Architecture
- Code should be decoupled using the custom Pub/Sub event system.
- To define a new event, create a class implementing `IEvent` (from the `EventSystems` namespace).
- To publish an event, use: `EventManager.Instance?.Publish(new MyEvent(parameters));`
- To subscribe to an event, use: `EventManager.Instance?.Subscribe(this, (MyEvent e) => MyCallback(e.Value));`
- To unsubscribe, use: `EventManager.Instance?.Unsubscribe<MyEvent>(this);` or `EventManager.Instance?.UnsubscribeFromAllEvents(this);` (usually in `OnDestroy()`).

### State Machines
- State Machine implementations must inherit from `BaseStateMachine` (from `StateMachine` namespace).
- Individual states must implement `IState` (from `StateMachine` namespace) with `Enter()`, `Update()`, `Exit()`, and `FixedUpdate()`.

### ScriptableObjects & Configuration
- Configuration and state-less data must be defined as `ScriptableObject`s.
- Save data definitions inside `Assets/Scripts/Data/` and create instances inside `Assets/ScriptableObjects/`.

## Key Learnings & Conventions
- **De-coupling:** Do not directly link gameplay scripts to UI or other unrelated systems. Use the `EventManager` to broadcast changes and allow listeners to react.
- **Unity 6+ features:** Keep code modern, clean, and optimized, utilizing performance-efficient patterns (e.g., object pooling for conveyors, items, and projectiles via `ObjectPoolManager`).
- **Clean Code:** Retain file headers, keep methods small, and write explanatory comments for complex game logic.


