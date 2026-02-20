using System;

namespace DarkfallOnline.Events
{
    /// <summary>
    /// Internal contract consumed by EventBus&lt;T&gt;.
    /// Separates the public API (EventBinding&lt;T&gt;) from the bus internals.
    /// </summary>
    public interface IEventBinding<T>
    {
        Action<T>  OnEvent       { get; set; }
        Action     OnEventNoArgs { get; set; }
    }

    /// <summary>
    /// Wraps one or more callbacks for a specific event type T.
    ///
    /// Supports two callback flavors:
    ///   • Action&lt;T&gt;  – receives the full event data struct.
    ///   • Action      – for listeners that only care that the event fired.
    ///
    /// Usage:
    /// <code>
    ///   // Create and register
    ///   var binding = new EventBinding&lt;PlayerDiedEvent&gt;(OnPlayerDied);
    ///   EventBus&lt;PlayerDiedEvent&gt;.Register(binding);
    ///
    ///   // Optionally stack extra callbacks
    ///   binding.Add(OnPlayerDiedNoArgs);
    ///
    ///   // Always deregister in OnDestroy / OnDisable to avoid leaks
    ///   EventBus&lt;PlayerDiedEvent&gt;.Deregister(binding);
    /// </code>
    /// </summary>
    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        // Default no-ops prevent null checks inside EventBus.Raise
        Action<T> onEvent       = _ => { };
        Action    onEventNoArgs = ()  => { };

        Action<T> IEventBinding<T>.OnEvent
        {
            get => onEvent;
            set => onEvent = value ?? (_ => { });
        }

        Action IEventBinding<T>.OnEventNoArgs
        {
            get => onEventNoArgs;
            set => onEventNoArgs = value ?? (() => { });
        }

        /// <summary>Create a binding with a typed callback.</summary>
        public EventBinding(Action<T> onEvent)       => this.onEvent       = onEvent ?? (_ => { });

        /// <summary>Create a binding with an untyped (no-arg) callback.</summary>
        public EventBinding(Action onEventNoArgs)    => this.onEventNoArgs = onEventNoArgs ?? (() => { });

        // ── Typed callbacks ──────────────────────────────────────────────────

        /// <summary>Stack an additional typed callback onto this binding.</summary>
        public void Add(Action<T> onEvent)    => this.onEvent       += onEvent;

        /// <summary>Remove a typed callback from this binding.</summary>
        public void Remove(Action<T> onEvent) => this.onEvent       -= onEvent;

        // ── Untyped callbacks ────────────────────────────────────────────────

        /// <summary>Stack an additional untyped callback onto this binding.</summary>
        public void Add(Action onEvent)       => onEventNoArgs += onEvent;

        /// <summary>Remove an untyped callback from this binding.</summary>
        public void Remove(Action onEvent)    => onEventNoArgs -= onEvent;
    }
}
