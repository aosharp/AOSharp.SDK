using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace AOSharp.Common.Runtime
{
    /// <summary>
    /// Maps assembly simple names to the deployment directory that contains the DLL and sibling content files.
    /// Populated by <c>AOSharp.Bootstrap</c> when assemblies are loaded from disk or from a known source path.
    /// </summary>
    public static class AssemblyContentRoots
    {
        private static readonly ConcurrentDictionary<string, string> Roots =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Registers a content directory for an assembly. First registration wins.</summary>
        public static void Register(string assemblySimpleName, string directoryOrDllPath)
        {
            if (string.IsNullOrWhiteSpace(assemblySimpleName) || string.IsNullOrWhiteSpace(directoryOrDllPath))
                return;

            var directory = NormalizeToDirectory(directoryOrDllPath);
            if (string.IsNullOrEmpty(directory))
                return;

            Roots.TryAdd(assemblySimpleName.Trim(), directory);
        }

        /// <summary>Registers using the DLL file name (without extension) as the assembly simple name.</summary>
        public static void RegisterFromAssemblyPath(string assemblyDllPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyDllPath))
                return;

            var name = Path.GetFileNameWithoutExtension(assemblyDllPath);
            if (string.IsNullOrEmpty(name))
                return;

            Register(name, assemblyDllPath);
        }

        public static bool TryGetContentDirectory(string assemblySimpleName, out string directory)
        {
            directory = null;
            if (string.IsNullOrWhiteSpace(assemblySimpleName))
                return false;

            return Roots.TryGetValue(assemblySimpleName.Trim(), out directory);
        }

        public static bool TryGetContentDirectory(Assembly assembly, out string directory)
        {
            directory = null;
            if (assembly == null)
                return false;

            var name = assembly.GetName().Name;
            if (!string.IsNullOrEmpty(name) && TryGetContentDirectory(name, out directory))
                return true;

            try
            {
                var loc = assembly.Location;
                if (!string.IsNullOrEmpty(loc))
                {
                    directory = Path.GetDirectoryName(loc);
                    return !string.IsNullOrEmpty(directory);
                }
            }
            catch
            {
                // Location can throw for dynamic assemblies
            }

            return false;
        }

        public static string GetContentDirectory(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (TryGetContentDirectory(assembly, out var directory))
                return directory;

            var name = assembly.GetName().Name ?? assembly.FullName ?? "?";
            throw new InvalidOperationException(
                $"No content directory is registered for assembly '{name}'. " +
                "Ensure the assembly was loaded by AOSharp Bootstrap, or call after the host has loaded it.");
        }

        /// <summary>Clears all registrations (e.g. when the plugin load context is unloaded).</summary>
        public static void Clear() => Roots.Clear();

        private static string NormalizeToDirectory(string directoryOrDllPath)
        {
            try
            {
                var full = Path.GetFullPath(directoryOrDllPath.Trim());
                if (File.Exists(full))
                    return Path.GetDirectoryName(full);

                if (Directory.Exists(full))
                    return full;
            }
            catch
            {
                // ignore invalid paths
            }

            return null;
        }
    }
}
