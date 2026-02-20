using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkfallOnline.Events
{
    /// <summary>
    /// Type-safe, allocation-light static event bus.
    ///
    /// Each event type T gets its own isolated static bus, so there is zero
    /// cross-contamination between unrelated events and no dictionary lookup
    /// per raise call.
    ///
    /// Thread safety: Unity is single-threaded (main thread only). This bus
    /// makes no guarantees for off-thread usage; raise events only from the
    /// main thread.
    ///
    /// ── Typical lifecycle ────────────────────────────────────────────────
    ///
    ///   // 1. Subscribe (Awake / OnEnable / Start)
    ///   playerDiedBinding = new EventBinding&lt;PlayerDiedEvent&gt;(OnPlayerDied);
    ///   EventBus&lt;PlayerDiedEvent&gt;.Register(playerDiedBinding);
    ///
    ///   // 2. Raise (from anywhere on the main thread)
    ///   EventBus&lt;PlayerDiedEvent&gt;.Raise(new PlayerDiedEvent { PlayerId = 0 });
    ///
    ///   // 3. Unsubscribe (OnDisable / OnDestroy) – MANDATORY to avoid leaks
    ///   EventBus&lt;PlayerDiedEvent&gt;.Deregister(playerDiedBinding);
    ///
    /// ── Safe to deregister inside a handler ──────────────────────────────
    ///
    ///   Raise() iterates over a snapshot of the binding set, so deregistering
    ///   a binding from within its own callback is safe.
    ///
    /// ── Exceptions are caught per-handler ────────────────────────────────
    ///
    ///   A throwing handler does not prevent subsequent handlers from running.
    ///   The exception is logged with full context and then swallowed.
    ///
    /// </summary>
    public static class EventBus<T> where T : IEvent
    {
        static readonly HashSet<IEventBinding<T>> bindings = new HashSet<IEventBinding<T>>();

        // ── Registration ─────────────────────────────────────────────────────

        /// <summary>Register a binding so it receives future Raise calls.</summary>
        public static void Register(EventBinding<T> binding)
        {
            if (binding == null)
            {
                Debug.LogWarning($"[EventBus<{typeof(T).Name}>] Register called with null binding.");
                return;
            }
            bindings.Add(binding);
        }

        /// <summary>Deregister a binding. Safe to call even if not registered.</summary>
        public static void Deregister(EventBinding<T> binding)
        {
            if (binding == null) return;
            bindings.Remove(binding);
        }

        // ── Dispatch ──────────────────────────────────────────────────────────

        /// <summary>
        /// Raises <paramref name="event"/> by invoking every registered handler.
        ///
        /// Iterates over a snapshot so that handlers may safely register or
        /// deregister bindings during the raise call.
        /// </summary>
        public static void Raise(T @event)
        {
            if (bindings.Count == 0) return;

            // Snapshot – prevents InvalidOperationException if a handler
            // modifies the binding set during iteration.
            var snapshot = new IEventBinding<T>[bindings.Count];
            bindings.CopyTo(snapshot);

            foreach (var binding in snapshot)
            {
                try
                {
                    binding.OnEvent?.Invoke(@event);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[EventBus<{typeof(T).Name}>] Exception in typed handler: {ex}");
                }

                try
                {
                    binding.OnEventNoArgs?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[EventBus<{typeof(T).Name}>] Exception in no-arg handler: {ex}");
                }
            }
        }

        // ── Cleanup ───────────────────────────────────────────────────────────

        /// <summary>
        /// Removes all registered bindings.
        /// Called by <see cref="EventBusUtil"/> on scene unload.
        /// </summary>
        public static void Clear()
        {
            int count = bindings.Count;
            bindings.Clear();

            if (count > 0)
                Debug.Log($"[EventBus<{typeof(T).Name}>] Cleared {count} binding(s).");
        }
    }
}
