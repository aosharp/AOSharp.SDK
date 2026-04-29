using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace AOSharp.Bootstrap.Contexts
{
    /// <summary>
    /// Custom AssemblyLoadContext for loading plugins in isolation
    /// </summary>
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly string _pluginPath;
        private readonly Dictionary<string, Assembly> _sharedAssemblies;

        public PluginLoadContext(string pluginPath, string name, IEnumerable<Assembly> sharedAssemblies = null) 
            : base(name, isCollectible: true)
        {
            _pluginPath = Path.GetDirectoryName(pluginPath);
            _resolver = new AssemblyDependencyResolver(pluginPath);
            _sharedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

            // Index the explicitly-shared assemblies by simple name so Load() and Resolving can return them directly
            if (sharedAssemblies != null)
            {
                foreach (var asm in sharedAssemblies)
                {
                    var n = asm.GetName().Name;
                    if (n != null)
                        _sharedAssemblies[n] = asm;
                }
            }


            Resolving += (context, name) =>
            {
                try
                {
                    Log.Debug("Resolving: {AssemblyName}", name.Name);
                    // For explicitly-shared assemblies return the pinned instance directly
                    if (name.Name != null && _sharedAssemblies.TryGetValue(name.Name, out var pinned))
                    {
                        Log.Information("Resolving {AssemblyName} from shared (pinned)", name.Name);
                        return pinned;
                    }

                    if (IsSharedAssembly(name))
                    {
                        var existing = Default.Assemblies.FirstOrDefault(a => a.GetName().Name == name.Name);
                        if (existing != null)
                        {
                            Log.Information("Resolving {AssemblyName} from Default.Assemblies (shared)", name.Name);
                            return existing;
                        }
                        // Not pinned and not in Default — try to find it on disk and load into Default
                        // so both contexts share the same instance
                        var sharedPath = TryFindAssemblyPathOnDisk(name, context);
                        if (sharedPath != null)
                        {
                            Log.Information("Resolving {AssemblyName} into Default from {Path} (shared fallback)", name.Name, sharedPath);
                            return Default.LoadFromAssemblyPath(sharedPath);
                        }
                        Log.Warning("[PluginLoadContext] Could not resolve shared assembly {AssemblyName} from Default", name.Name);
                        return null;
                    }

                    var path = TryFindAssemblyPathOnDisk(name, context);
                    if (path != null)
                    {
                        Log.Information("Resolving {AssemblyName} from {Path}", name.Name, path);
                        return context.LoadFromAssemblyPath(path);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[PluginLoadContext] Resolving {AssemblyName}: {Message}", name?.Name ?? "?", ex.Message);
                }
                return null;
            };
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            try
            {
                // Return pinned shared assembly directly — no search, guaranteed same instance
                if (assemblyName.Name != null && _sharedAssemblies.TryGetValue(assemblyName.Name, out var pinned))
                    return pinned;

                if (IsSharedAssembly(assemblyName))
                {
                    try
                    {
                        var assembly = Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
                        if (assembly != null)
                            return assembly;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "[PluginLoadContext] Load (shared {Name}): {Message}", assemblyName?.Name ?? "?", ex.Message);
                    }
                    return null;
                }

                string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                    return LoadFromAssemblyPath(assemblyPath);

                string localPath = Path.Combine(_pluginPath, $"{assemblyName.Name}.dll");
                if (File.Exists(localPath))
                    return LoadFromAssemblyPath(localPath);

                var probe = TryFindAssemblyPathOnDisk(assemblyName, this);
                if (probe != null)
                    return LoadFromAssemblyPath(probe);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[PluginLoadContext] Load {Name}: {Message}", assemblyName?.Name ?? "?", ex.Message);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            try
            {
                string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                if (libraryPath != null)
                    return LoadUnmanagedDllFromPath(libraryPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[PluginLoadContext] LoadUnmanagedDll {Name}: {Message}", unmanagedDllName ?? "?", ex.Message);
            }
            return IntPtr.Zero;
        }

        private bool IsSharedAssembly(AssemblyName assemblyName)
        {
            // Define which assemblies should be shared between contexts
            return assemblyName.Name == "AOSharp.Common" ||
                   assemblyName.Name == "AOSharp.Bootstrap" ||
                   assemblyName.Name.StartsWith("System.") ||
                   assemblyName.Name.StartsWith("Microsoft.") ||
                   assemblyName.Name == "Newtonsoft.Json" ||
                   assemblyName.Name == "Serilog";
        }

        private static IEnumerable<string> GetProbingPaths()
        {
            var paths = new List<string>
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory,
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? "") ?? ""
            };

            var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (!string.IsNullOrEmpty(runtimeDir))
                paths.Add(runtimeDir);

            return paths.Where(p => !string.IsNullOrEmpty(p)).Distinct();
        }

        /// <summary>
        /// Finds a dependency on disk: next to any assembly already loaded in <paramref name="context"/>,
        /// then under each <c>Plugins\*</c> folder (repo compiler output), then standard probing paths.
        /// </summary>
        private static string TryFindAssemblyPathOnDisk(AssemblyName assemblyName, AssemblyLoadContext context)
        {
            if (assemblyName?.Name == null || context == null)
                return null;

            var simple = assemblyName.Name;

            foreach (var asm in context.Assemblies)
            {
                try
                {
                    var loc = asm.Location;
                    if (string.IsNullOrEmpty(loc))
                        continue;
                    var dir = Path.GetDirectoryName(loc);
                    if (string.IsNullOrEmpty(dir))
                        continue;
                    foreach (var ext in new[] { ".dll", ".exe" })
                    {
                        var candidate = Path.Combine(dir, simple + ext);
                        if (File.Exists(candidate))
                            return candidate;
                    }
                }
                catch
                {
                    // Location can throw for dynamic assemblies
                }
            }

            var pluginsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (Directory.Exists(pluginsRoot))
            {
                try
                {
                    foreach (var sub in Directory.EnumerateDirectories(pluginsRoot))
                    {
                        foreach (var ext in new[] { ".dll", ".exe" })
                        {
                            var candidate = Path.Combine(sub, simple + ext);
                            if (File.Exists(candidate))
                                return candidate;
                        }
                    }
                }
                catch
                {
                    // ignore IO errors
                }
            }

            foreach (var basePath in GetProbingPaths())
            {
                foreach (var ext in new[] { ".dll", ".exe" })
                {
                    var candidate = Path.Combine(basePath, simple + ext);
                    if (File.Exists(candidate))
                        return candidate;
                }
            }

            return null;
        }
    }
}
