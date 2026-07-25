using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using System;
using System.Collections.Generic;
using EventSystems;
using Singleton;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Scripts.EventSystems
{

    public class EventManager : SingletonBase<EventManager>
    {
        public static bool DebugLoggingEnabled = false; // Set to false to disable all debug logs from EventManager and related classes

        private readonly Dictionary<Type, List<EventSubscription>> _subscriptions = new();
        private readonly List<EventSubscription> _pendingRemovals = new();
        private bool _isPublishing;

        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }

        private class EventSubscription // base class for event subscriptions
        {
            public WeakReference TargetReference { get; set; }
            public Delegate Callback { get; set; }
            public bool MarkedForRemoval { get; set; }
        }


        // Run this function to listen to a specific event with the syntax of
        // EventManager.Instance?.Subscribe(this, ("EventName" e) => "FunctionToCall"(e."value to pass through")); 
        public void Subscribe<T>(object target, Action<T> callback) where T : IEvent
        {
            var eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out var subscriptionList))
            {
                subscriptionList = new List<EventSubscription>();
                _subscriptions[eventType] = subscriptionList;
            }

            subscriptionList.Add(new EventSubscription
            {
                TargetReference = new WeakReference(target),
                Callback = callback
            });
        }

        public void Unsubscribe<T>(object target) where T : IEvent
        {
            var eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out var subscriptionList))
                return;

            foreach (var subscription in subscriptionList)
            {
                if (subscription.TargetReference.Target == target)
                {
                    if (_isPublishing)
                    {
                        subscription.MarkedForRemoval = true;
                        _pendingRemovals.Add(subscription);
                    }
                    else
                    {
                        subscriptionList.Remove(subscription);
                        break;
                    }
                }
            }
        }

        public void UnsubscribeFromAllEvents(object target)
        {
            foreach (var kvp in _subscriptions)
            {
                var eventType = kvp.Key;

                var unsubscribeMethod = typeof(EventManager)
                    .GetMethod("Unsubscribe")
                    ?.MakeGenericMethod(eventType);

                if (unsubscribeMethod != null) unsubscribeMethod.Invoke(this, new object[] { target });
            }
        }

        public void UnsubscribeAllTargets()
        {
            var allTargets = GetAllSubscribedTargets();

            foreach (var target in allTargets)
            {
                UnsubscribeFromAllEvents(target);
            }
        }

        public IEnumerable<object> GetAllSubscribedTargets()
        {
            var targets = new HashSet<object>();

            foreach (var subscriptionList in _subscriptions.Values)
            {
                foreach (var subscription in subscriptionList)
                {
                    var target = subscription.TargetReference.Target;
                    if (target != null)
                        targets.Add(target); // Avoid duplicates
                }
            }

            return targets;
        }

        public void Publish<T>(T eventData)
            where T : IEvent
        {
            var eventType = typeof(T);
            if (DebugLoggingEnabled) Debug.Log($"EventManager: Attempting to publish event of type {eventType.Name}");

            if (!_subscriptions.TryGetValue(eventType, out var subscriptionList))
            {
                if (DebugLoggingEnabled) Debug.Log($"EventManager: No subscriptions found for event type {eventType.Name}");
                return;
            }

            if (DebugLoggingEnabled) Debug.Log($"EventManager: Found {subscriptionList.Count} subscriptions for {eventType.Name}");
            _isPublishing = true;

            for (int i = subscriptionList.Count - 1; i >= 0; i--)
            {
                var subscription = subscriptionList[i];

                if (subscription.MarkedForRemoval)
                {
                    if (DebugLoggingEnabled) Debug.Log($"EventManager: Subscription for {eventType.Name} marked for removal, skipping.");
                    continue;
                }

                var target = subscription.TargetReference.Target;

                if (target == null)
                {
                    if (DebugLoggingEnabled) Debug.LogWarning($"EventManager: Target for subscription of {eventType.Name} is null (likely destroyed), marking for removal.");
                    subscription.MarkedForRemoval = true;
                    _pendingRemovals.Add(subscription);
                    continue;
                }

                try
                {
                    if (DebugLoggingEnabled) Debug.Log($"EventManager: Invoking callback for {eventType.Name} on target {target.GetType().Name}.");
                    ((Action<T>)subscription.Callback).Invoke(eventData);
                }
                catch (Exception e)
                {
                     Debug.LogError($"Error publishing event {typeof(T).Name} on target {target.GetType().Name}: {e.Message}");
                }
            }

            _isPublishing = false;
            CleanupPendingRemovals();
        }

        private void CleanupPendingRemovals() // clean up unused events
        {
            if (_pendingRemovals.Count == 0)
                return;

            foreach (var subscription in _pendingRemovals)
            {
                foreach (var kvp in _subscriptions)
                {
                    kvp.Value.Remove(subscription);
                }
            }

            _pendingRemovals.Clear();
        }

        public void Clear()
        {
            _subscriptions.Clear();
            _pendingRemovals.Clear();
            _isPublishing = false;
        }

        protected override void OnDestroy()
        {
            Clear();
        }
    }
}
