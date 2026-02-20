using System;
using System.Collections.Generic;
using System.Reflection;

namespace DarkfallOnline.Events
{
    /// <summary>
    /// Scans Unity's predefined assemblies (Assembly-CSharp and variants) to
    /// collect all concrete types that implement a given interface.
    ///
    /// Used by <see cref="EventBusUtil"/> to discover every <see cref="IEvent"/>
    /// implementation at runtime without requiring manual registration.
    ///
    /// IL2CPP / code-stripping note:
    ///   IL2CPP may strip event structs that it considers unreachable.
    ///   If a struct-based event disappears from a release build, add a
    ///   link.xml entry to preserve it:
    ///   <code>
    ///     &lt;linker&gt;
    ///       &lt;assembly fullname="Assembly-CSharp" preserve="all"/&gt;
    ///     &lt;/linker&gt;
    ///   </code>
    /// </summary>
    public static class PredefinedAssemblyUtil
    {
        // Unity ships exactly these four assembly names for user scripts.
        static readonly HashSet<string> predefinedAssemblyNames = new HashSet<string>
        {
            "Assembly-CSharp",
            "Assembly-CSharp-Editor",
            "Assembly-CSharp-firstpass",
            "Assembly-CSharp-Editor-firstpass",
        };

        /// <summary>
        /// Returns all non-interface, non-abstract types in Unity's predefined
        /// assemblies that implement <paramref name="interfaceType"/>.
        /// </summary>
        public static List<Type> GetTypes(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            var results = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!predefinedAssemblyNames.Contains(assembly.GetName().Name))
                    continue;

                AddImplementations(assembly, interfaceType, results);
            }

            return results;
        }

        static void AddImplementations(Assembly assembly, Type interfaceType, List<Type> results)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    // Skip the interface itself and any abstract types
                    if (type == interfaceType) continue;
                    if (type.IsAbstract)       continue;

                    if (interfaceType.IsAssignableFrom(type))
                        results.Add(type);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Partial failure – log and continue with whatever loaded
                UnityEngine.Debug.LogWarning(
                    $"[PredefinedAssemblyUtil] Partial load in assembly " +
                    $"'{assembly.GetName().Name}': {ex.LoaderExceptions[0]?.Message}");

                foreach (var type in ex.Types)
                {
                    if (type == null)            continue;
                    if (type == interfaceType)   continue;
                    if (type.IsAbstract)         continue;

                    if (interfaceType.IsAssignableFrom(type))
                        results.Add(type);
                }
            }
        }
    }
}
