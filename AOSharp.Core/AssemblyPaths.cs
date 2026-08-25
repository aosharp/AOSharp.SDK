using System;
using System.Reflection;
using AOSharp.Common.Runtime;

namespace AOSharp.Core
{
    /// <summary>
    /// Resolves the deployment folder for a plugin or library assembly (sibling content files, config, etc.).
    /// </summary>
    public static class AssemblyPaths
    {
        public static string GetContentDirectory(Assembly assembly) =>
            AssemblyContentRoots.GetContentDirectory(assembly);

        public static string GetContentDirectory<T>() =>
            GetContentDirectory(typeof(T).Assembly);

        public static bool TryGetContentDirectory(Assembly assembly, out string directory) =>
            AssemblyContentRoots.TryGetContentDirectory(assembly, out directory);

        public static bool TryGetContentDirectory<T>(out string directory) =>
            TryGetContentDirectory(typeof(T).Assembly, out directory);
    }
}
