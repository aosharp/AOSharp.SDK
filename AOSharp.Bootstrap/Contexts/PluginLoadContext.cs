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
        /// <summary>Physical folders for each loaded plugin DLL (collectible ALC often has empty <see cref="Assembly.Location"/>, so sibling probe needs this).</summary>
        private readonly List<string> _registeredPluginDirectories = new List<string>();

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

            RegisterPluginDirectory(_pluginPath);

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
                        return context.LoadFromStream(new MemoryStream(File.ReadAllBytes(path)));
                    }

                    Log.Error("[PluginLoadContext] Resolving returned null for {AssemblyName} (not shared). {Diagnostics}",
                        name.Name, BuildAssemblyProbeDiagnostics(name, context));
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
                    return LoadFromStream(new MemoryStream(File.ReadAllBytes(assemblyPath)));
                }

                string localPath = Path.Combine(_pluginPath, $"{assemblyName.Name}.dll");
                if (File.Exists(localPath))
                {
                    Log.Debug("[PluginLoadContext] Load {Name}: next to core dir -> {Path}", assemblyName.Name, localPath);
                    return LoadFromStream(new MemoryStream(File.ReadAllBytes(localPath)));
                }

                Log.Debug("[PluginLoadContext] Load {Name}: resolver and {LocalPath} miss; probing disk", assemblyName.Name, localPath);
                var probe = TryFindAssemblyPathOnDisk(assemblyName, this);
                if (probe != null)
                    return LoadFromStream(new MemoryStream(File.ReadAllBytes(probe)));

                Log.Error("[PluginLoadContext] Load returned null for {AssemblyName}. {Diagnostics}",
                    assemblyName.Name, BuildAssemblyProbeDiagnostics(assemblyName, this));
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

                libraryPath = TryFindUnmanagedDllPath(unmanagedDllName);
                if (libraryPath != null)
                {
                    Log.Debug("[PluginLoadContext] LoadUnmanagedDll {Name}: plugin probe -> {Path}", unmanagedDllName, libraryPath);
                    return LoadUnmanagedDllFromPath(libraryPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[PluginLoadContext] LoadUnmanagedDll {Name}: {Message}", unmanagedDllName ?? "?", ex.Message);
            }
            return IntPtr.Zero;
        }

        /// <summary>Find native SQLite / other deps next to registered plugin folders (e.g. runtimes\win-x64\native).</summary>
        private string TryFindUnmanagedDllPath(string unmanagedDllName)
        {
            if (string.IsNullOrWhiteSpace(unmanagedDllName))
                return null;

            var baseName = unmanagedDllName;
            if (baseName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                baseName.EndsWith(".so", StringComparison.OrdinalIgnoreCase) ||
                baseName.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase))
                baseName = Path.GetFileNameWithoutExtension(baseName);

            var fileNames = new[] { $"{baseName}.dll", $"lib{baseName}.so", $"lib{baseName}.dylib", $"{baseName}.dylib" };

            foreach (var dir in _registeredPluginDirectories)
            {
                var hit = ProbeUnmanagedInDirectory(dir, fileNames);
                if (hit != null)
                    return hit;
            }

            foreach (var pluginsRoot in EnumeratePluginsRoots(_pluginPath).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(pluginsRoot) || !Directory.Exists(pluginsRoot))
                    continue;

                try
                {
                    foreach (var sub in Directory.EnumerateDirectories(pluginsRoot))
                    {
                        var hit = ProbeUnmanagedInDirectory(sub, fileNames);
                        if (hit != null)
                            return hit;
                    }
                }
                catch
                {
                    // ignore scan errors
                }
            }

            return null;
        }

        private static string ProbeUnmanagedInDirectory(string directory, string[] fileNames)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            foreach (var fileName in fileNames)
            {
                var direct = Path.Combine(directory, fileName);
                if (File.Exists(direct))
                    return direct;
            }

            try
            {
                foreach (var candidate in Directory.EnumerateFiles(directory, fileNames[0], SearchOption.AllDirectories))
                    return candidate;
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private bool IsSharedAssembly(AssemblyName assemblyName)
        {
            if (assemblyName.Name == null)
                return false;

            var name = assemblyName.Name;

            // Assemblies that must be the same instance in Default and plugin ALCs
            if (name == "AOSharp.Common" ||
                name == "AOSharp.Bootstrap" ||
                name == "Newtonsoft.Json" ||
                name == "Serilog")
                return true;

            // Host / runtime facades (System.*) are already in Default
            if (name.StartsWith("System.", StringComparison.Ordinal))
                return true;

            // Only share Microsoft.* already loaded in Default (host/runtime).
            // Plugin-deployed Microsoft.* (e.g. Microsoft.Data.Sqlite) must load in the plugin ALC
            // so SQLitePCLRaw.* and native e_sqlite3 resolve from Plugins\AOItemQueryService (etc.).
            if (name.StartsWith("Microsoft.", StringComparison.Ordinal))
                return IsLoadedInDefault(name);

            return false;
        }

        private static bool IsLoadedInDefault(string simpleName) =>
            AssemblyLoadContext.Default.Assemblies.Any(a =>
                string.Equals(a.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase));

        /// <summary>Call after each <see cref="AssemblyLoadContext.LoadFromAssemblyPath"/> for a plugin entry assembly.</summary>
        public void RegisterPluginDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return;
            try
            {
                var full = Path.GetFullPath(directory.Trim());
                if (_registeredPluginDirectories.Contains(full, StringComparer.OrdinalIgnoreCase))
                    return;
                _registeredPluginDirectories.Add(full);
                Log.Information("[PluginLoadContext] Registered dependency search dir: {Dir}", full);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[PluginLoadContext] RegisterPluginDirectory failed for {Dir}", directory);
            }
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
        /// Finds a dependency on disk: registered plugin dirs (see <see cref="RegisterPluginDirectory"/>),
        /// then next to any assembly already loaded in <paramref name="context"/> (when <see cref="Assembly.Location"/> is set),
        /// then under each <c>Plugins\*</c> folder next to the managed host,
        /// then standard probing paths.
        /// </summary>
        private string TryFindAssemblyPathOnDisk(AssemblyName assemblyName, AssemblyLoadContext context)
        {
            if (assemblyName?.Name == null || context == null)
                return null;

            var simple = assemblyName.Name;

            foreach (var dir in _registeredPluginDirectories)
            {
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                    continue;
                foreach (var ext in new[] { ".dll", ".exe" })
                {
                    var candidate = Path.Combine(dir, simple + ext);
                    if (File.Exists(candidate))
                    {
                        Log.Information("[PluginLoadContext] Probe {Assembly}: found in registered dir -> {Candidate}", simple, candidate);
                        return candidate;
                    }
                }
            }

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

            foreach (var pluginsRoot in EnumeratePluginsRoots(_pluginPath).Distinct(StringComparer.OrdinalIgnoreCase))
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

        /// <summary>One-line summary when an assembly could not be resolved.</summary>
        private string BuildAssemblyProbeDiagnostics(AssemblyName assemblyName, AssemblyLoadContext context)
        {
            var sb = new StringBuilder();
            sb.Append("Requested=").Append(assemblyName?.FullName ?? "?");
            sb.Append("; ManagedHostDir=").Append(_pluginPath ?? "(null)");
            sb.Append("; BaseDirectory=").Append(AppDomain.CurrentDomain.BaseDirectory);
            sb.Append("; RegisteredDirs=").Append(_registeredPluginDirectories.Count == 0
                ? "(none)"
                : string.Join(", ", _registeredPluginDirectories));

            var roots = EnumeratePluginsRoots(_pluginPath).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
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

        /// <summary>
        /// AOSharp plugin folders: parent <c>Plugins</c> when the host lives under <c>Plugins\{name}</c>,
        /// then <c>{dir}\Plugins</c> for other layouts, then BaseDirectory fallback.
        /// </summary>
        private static IEnumerable<string> EnumeratePluginsRoots(string managedHostDir)
        {
            foreach (var root in GetPluginsRootIfUnderPluginsFolder(managedHostDir))
                yield return root;

            if (!string.IsNullOrEmpty(managedHostDir))
                yield return Path.Combine(managedHostDir, "Plugins");

            var bootstrapDir = Path.GetDirectoryName(typeof(PluginLoadContext).Assembly.Location);
            foreach (var root in GetPluginsRootIfUnderPluginsFolder(bootstrapDir))
                yield return root;

            if (!string.IsNullOrEmpty(bootstrapDir))
                yield return Path.Combine(bootstrapDir, "Plugins");

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir))
                yield return Path.Combine(baseDir, "Plugins");
        }

        /// <summary>If <paramref name="directory"/> is <c>...\Plugins\Something</c>, returns <c>...\Plugins</c>.</summary>
        private static IEnumerable<string> GetPluginsRootIfUnderPluginsFolder(string directory)
        {
            var root = TryGetPluginsRootIfUnderPluginsFolder(directory);
            if (root != null)
                yield return root;
        }

        private static string TryGetPluginsRootIfUnderPluginsFolder(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return null;

            try
            {
                var full = Path.GetFullPath(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                var parent = Directory.GetParent(full);
                if (parent != null && string.Equals(parent.Name, "Plugins", StringComparison.OrdinalIgnoreCase))
                    return parent.FullName;
            }
            catch
            {
                // ignore invalid paths
            }

            return null;
        }
    }
}
