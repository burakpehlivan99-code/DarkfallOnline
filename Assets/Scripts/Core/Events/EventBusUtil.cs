using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkfallOnline.Events
{
    /// <summary>
    /// Bootstraps and maintains the event bus system for the lifetime of the
    /// application.
    ///
    /// Responsibilities:
    ///   1. Discovers all IEvent implementations via reflection at startup.
    ///   2. Builds a list of the corresponding EventBus&lt;T&gt; types.
    ///   3. Clears all buses on scene unload to prevent stale listener leaks.
    ///   4. In the Editor, additionally clears buses when exiting Play Mode
    ///      so that domain-reload-less iterations start with a clean slate.
    ///
    /// Clearing strategy:
    ///   When a scene unloads, every non-persistent MonoBehaviour is destroyed,
    ///   so their bindings become dangling. Clearing the buses at that moment
    ///   removes those dangling references automatically. Persistent singletons
    ///   (GameManager, SceneLoader, UIManager) do not subscribe to buses, so
    ///   they are unaffected.
    ///
    ///   Objects that survive across scenes (DontDestroyOnLoad) and DO subscribe
    ///   to events must re-register in OnEnable and deregister in OnDisable
    ///   rather than relying on the auto-clear.
    /// </summary>
    public static class EventBusUtil
    {
        // Populated once at startup; reused for every Clear call.
        static IReadOnlyList<Type> eventBusTypes;

#if UNITY_EDITOR
        // ── Editor-only: clear when exiting Play Mode ─────────────────────────

        [UnityEditor.InitializeOnLoadMethod]
        static void InitializeEditor()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
                ClearAllBuses();
        }
#endif

        // ── Runtime initializer ───────────────────────────────────────────────

        /// <summary>
        /// Called automatically before the first scene loads.
        /// Discovers all IEvent types and hooks into scene unload events.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            var eventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
            eventBusTypes  = BuildBusTypeList(eventTypes);

            Debug.Log($"[EventBusUtil] Initialized with {eventTypes.Count} event type(s).");

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        static void OnSceneUnloaded(Scene scene)
        {
            ClearAllBuses();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static List<Type> BuildBusTypeList(IReadOnlyList<Type> eventTypes)
        {
            var busList    = new List<Type>(eventTypes.Count);
            var busGeneric = typeof(EventBus<>);

            foreach (var evtType in eventTypes)
            {
                try
                {
                    busList.Add(busGeneric.MakeGenericType(evtType));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[EventBusUtil] Could not build EventBus<{evtType.Name}>: {ex.Message}");
                }
            }

            return busList;
        }

        static void ClearAllBuses()
        {
            if (eventBusTypes == null) return;

            foreach (var busType in eventBusTypes)
            {
                var clearMethod = busType.GetMethod(
                    "Clear",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                try
                {
                    clearMethod?.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[EventBusUtil] Failed to clear {busType.Name}: {ex.Message}");
                }
            }
        }
    }
}
