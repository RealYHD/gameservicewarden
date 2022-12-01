using System;
using System.Reflection;
using System.Runtime.Loader;

namespace GameServiceWarden.Core.Module
{
    class ModuleLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver dependencyResolver;

        public ModuleLoadContext(string path) {
            dependencyResolver = new AssemblyDependencyResolver(path);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = dependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null) {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            String libraryPath = dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null) {
                return LoadUnmanagedDllFromPath(libraryPath);
            }
            return IntPtr.Zero;
        }
    }
}