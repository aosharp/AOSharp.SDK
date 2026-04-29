using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

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

            Log.Debug("[PluginLoadContext] Created {Name}; managed host dir (core dir) = {ManagedHostDir}; BaseDirectory = {BaseDir}",
                name, _pluginPath ?? "(null)", AppDomain.CurrentDomain.BaseDirectory);

            Resolving += (context, name) =>
            {
                try
                {
                    Log.Debug("[PluginLoadContext] Resolving event: {AssemblyName}", name.Name);
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
                        var sharedPath = TryFindAssemblyPathOnDisk(name, context, _pluginPath);
                        if (sharedPath != null)
                        {
                            Log.Information("Resolving {AssemblyName} into Default from {Path} (shared fallback)", name.Name, sharedPath);
                            return Default.LoadFromAssemblyPath(sharedPath);
                        }
                        Log.Warning("[PluginLoadContext] Could not resolve shared assembly {AssemblyName} from Default", name.Name);
                        return null;
                    }

                    var path = TryFindAssemblyPathOnDisk(name, context, _pluginPath);
                    if (path != null)
                    {
                        Log.Information("Resolving {AssemblyName} from {Path}", name.Name, path);
                        return context.LoadFromAssemblyPath(path);
                    }

                    Log.Warning("[PluginLoadContext] Resolving returned null for {AssemblyName} (not shared). {Diagnostics}",
                        name.Name, BuildAssemblyProbeDiagnostics(name, context, _pluginPath));
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
                {
                    Log.Debug("[PluginLoadContext] Load {Name}: dependency resolver -> {Path}", assemblyName.Name, assemblyPath);
                    return LoadFromAssemblyPath(assemblyPath);
                }

                string localPath = Path.Combine(_pluginPath, $"{assemblyName.Name}.dll");
                if (File.Exists(localPath))
                {
                    Log.Debug("[PluginLoadContext] Load {Name}: next to core dir -> {Path}", assemblyName.Name, localPath);
                    return LoadFromAssemblyPath(localPath);
                }

                Log.Debug("[PluginLoadContext] Load {Name}: resolver and {LocalPath} miss; probing disk", assemblyName.Name, localPath);
                var probe = TryFindAssemblyPathOnDisk(assemblyName, this, _pluginPath);
                if (probe != null)
                    return LoadFromAssemblyPath(probe);

                Log.Warning("[PluginLoadContext] Load returned null for {AssemblyName}. {Diagnostics}",
                    assemblyName.Name, BuildAssemblyProbeDiagnostics(assemblyName, this, _pluginPath));
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
        /// then under each <c>Plugins\*</c> folder next to the managed host (see <paramref name="managedHostDir"/>),
        /// then standard probing paths. Uses <paramref name="managedHostDir"/> instead of <see cref="AppDomain.CurrentDomain.BaseDirectory"/>
        /// because the bootstrap often runs with the game's base directory, not the AOSharp bin folder where <c>Plugins</c> lives.
        /// </summary>
        private static string TryFindAssemblyPathOnDisk(AssemblyName assemblyName, AssemblyLoadContext context, string managedHostDir)
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

            foreach (var pluginsRoot in EnumeratePluginsRoots(managedHostDir).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(pluginsRoot))
                    continue;
                if (!Directory.Exists(pluginsRoot))
                {
                    Log.Debug("[PluginLoadContext] Probe {Assembly}: Plugins root missing: {Root}", simple, pluginsRoot);
                    continue;
                }

                try
                {
                    var subs = Directory.EnumerateDirectories(pluginsRoot).Select(Path.GetFileName).ToList();
                    Log.Debug("[PluginLoadContext] Probe {Assembly}: under {Root} ({Count} plugin folders: {Folders})",
                        simple, pluginsRoot, subs.Count,
                        subs.Count == 0 ? "(none)" : string.Join(", ", subs.Take(25)) + (subs.Count > 25 ? ", …" : ""));

                    foreach (var sub in Directory.EnumerateDirectories(pluginsRoot))
                    {
                        foreach (var ext in new[] { ".dll", ".exe" })
                        {
                            var candidate = Path.Combine(sub, simple + ext);
                            if (File.Exists(candidate))
                            {
                                Log.Debug("[PluginLoadContext] Probe {Assembly}: hit {Candidate}", simple, candidate);
                                return candidate;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "[PluginLoadContext] Probe {Assembly}: error scanning {Root}", simple, pluginsRoot);
                }
            }

            foreach (var basePath in GetProbingPaths())
            {
                foreach (var ext in new[] { ".dll", ".exe" })
                {
                    var candidate = Path.Combine(basePath, simple + ext);
                    if (File.Exists(candidate))
                    {
                        Log.Debug("[PluginLoadContext] Probe {Assembly}: hit under general path {Candidate}", simple, candidate);
                        return candidate;
                    }
                }
            }

            Log.Debug("[PluginLoadContext] Probe {Assembly}: no match after full scan", simple);
            return null;
        }

        /// <summary>One-line summary for warnings when an assembly could not be resolved.</summary>
        private static string BuildAssemblyProbeDiagnostics(AssemblyName assemblyName, AssemblyLoadContext context, string managedHostDir)
        {
            var sb = new StringBuilder();
            sb.Append("Requested=").Append(assemblyName?.FullName ?? "?");
            sb.Append("; ManagedHostDir=").Append(managedHostDir ?? "(null)");
            sb.Append("; BaseDirectory=").Append(AppDomain.CurrentDomain.BaseDirectory);

            var roots = EnumeratePluginsRoots(managedHostDir).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            sb.Append("; Plugins roots: ");
            sb.Append(string.Join(" | ", roots.Select(r => $"{r} [{(Directory.Exists(r) ? "ok" : "missing")}]")));

            try
            {
                var loaded = context.Assemblies
                    .Select(a =>
                    {
                        try
                        {
                            return string.IsNullOrEmpty(a.Location)
                                ? $"{a.GetName().Name}@(no Location)"
                                : $"{a.GetName().Name}@…\\{Path.GetFileName(a.Location)}";
                        }
                        catch
                        {
                            return $"{a.GetName().Name}@(Location error)";
                        }
                    });
                sb.Append("; Loaded in ALC: ").Append(string.Join(", ", loaded));
            }
            catch (Exception ex)
            {
                sb.Append("; Loaded in ALC: (error listing: ").Append(ex.Message).Append(')');
            }

            return sb.ToString();
        }

        /// <summary>AOSharp plugin folders: <c>{managedHostDir}\Plugins</c>, then Bootstrap assembly dir, then BaseDirectory fallback.</summary>
        private static IEnumerable<string> EnumeratePluginsRoots(string managedHostDir)
        {
            if (!string.IsNullOrEmpty(managedHostDir))
                yield return Path.Combine(managedHostDir, "Plugins");

            var bootstrapDir = Path.GetDirectoryName(typeof(PluginLoadContext).Assembly.Location);
            if (!string.IsNullOrEmpty(bootstrapDir))
                yield return Path.Combine(bootstrapDir, "Plugins");

            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        }
    }
}
