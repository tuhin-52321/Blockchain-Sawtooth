using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace CSInteroperabilityConsole
{
    public static class TWNativeMethods
    {
        private const string libtallyworld = TWNativeSymbols.Source;

        // An object tied to the lifecycle of the TWNativeMethods static class.
        // This will handle initialization and shutdown of the underlying
        // native library.
#pragma warning disable 0414
        private static TWNativeShutdownObject shutdownObject;
#pragma warning restore 0414

        static TWNativeMethods()
        {
            if (Platform.IsRunningOnNetFramework() || Platform.IsRunningOnNetCore())
            {
                // Use .NET Core 3.0+ NativeLibrary when available.
                if (!TryUseNativeLibrary())
                {
                    // NativeLibrary is not available, fall back.

                    // Use GlobalSettings.NativeLibraryPath when set.
                    // Try to load the .dll from the path explicitly.
                    // If this call succeeds further DllImports will find the library loaded and not attempt to load it again.
                    // If it fails the next DllImport will load the library from safe directories.
                    string nativeLibraryPath = GetGlobalSettingsNativeLibraryPath();
                    if (nativeLibraryPath != null)
                    {
#if NETFRAMEWORK
                        if (Platform.OperatingSystem == OperatingSystemType.Windows)
#else
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#endif
                        {
                            LoadWindowsLibrary(nativeLibraryPath);
                        }
                        else
                        {
                            LoadUnixLibrary(nativeLibraryPath, RTLD_NOW);
                        }
                    }
                }
            }

            InitializeNativeLibrary();
        }

        private static string GetGlobalSettingsNativeLibraryPath()
        {
            string nativeLibraryDir = GlobalSettings.GetAndLockNativeLibraryPath();
            if (nativeLibraryDir == null)
            {
                return null;
            }
            return Path.Combine(nativeLibraryDir, libtallyworld + Platform.GetNativeLibraryExtension());
        }

        private delegate bool TryLoadLibraryByNameDelegate(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, out IntPtr handle);
        private delegate bool TryLoadLibraryByPathDelegate(string libraryPath, out IntPtr handle);

        static TryLoadLibraryByNameDelegate _tryLoadLibraryByName;
        static TryLoadLibraryByPathDelegate _tryLoadLibraryByPath;

        static bool TryLoadLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, out IntPtr handle)
        {
            if (_tryLoadLibraryByName == null)
            {
                throw new NotSupportedException();
            }
            return _tryLoadLibraryByName(libraryName, assembly, searchPath, out handle);
        }

        static bool TryLoadLibrary(string libraryPath, out IntPtr handle)
        {
            if (_tryLoadLibraryByPath == null)
            {
                throw new NotSupportedException();
            }
            return _tryLoadLibraryByPath(libraryPath, out handle);
        }

        private static bool TryUseNativeLibrary()
        {
            // NativeLibrary is available in .NET Core 3.0+.
            // We use reflection to use NativeLibrary so this library can target 'netstandard2.0'.

            Type dllImportResolverType = Type.GetType("System.Runtime.InteropServices.DllImportResolver, System.Runtime.InteropServices", throwOnError: false);
            Type nativeLibraryType = Type.GetType("System.Runtime.InteropServices.NativeLibrary, System.Runtime.InteropServices", throwOnError: false);
            var tryLoadLibraryByName = (TryLoadLibraryByNameDelegate)nativeLibraryType?.GetMethod("TryLoad",
                    new Type[] { typeof(string), typeof(Assembly), typeof(DllImportSearchPath?), typeof(IntPtr).MakeByRefType() })?.CreateDelegate(typeof(TryLoadLibraryByNameDelegate));
            var tryLoadLibraryByPath = (TryLoadLibraryByPathDelegate)nativeLibraryType?.GetMethod("TryLoad",
                    new Type[] { typeof(string), typeof(IntPtr).MakeByRefType() })?.CreateDelegate(typeof(TryLoadLibraryByPathDelegate));
            MethodInfo setDllImportResolver = nativeLibraryType?.GetMethod("SetDllImportResolver", new Type[] { typeof(Assembly), dllImportResolverType });

            if (dllImportResolverType == null ||
                nativeLibraryType == null ||
                tryLoadLibraryByName == null ||
                tryLoadLibraryByPath == null ||
                setDllImportResolver == null)
            {
                return false;
            }

            _tryLoadLibraryByPath = tryLoadLibraryByPath;
            _tryLoadLibraryByName = tryLoadLibraryByName;

            // NativeMethods.SetDllImportResolver(typeof(NativeMethods).Assembly, ResolveDll);
            object resolveDelegate = typeof(TWNativeMethods).GetMethod(nameof(ResolveDll), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate(dllImportResolverType);
            setDllImportResolver.Invoke(null, new object[] { typeof(TWNativeMethods).Assembly, resolveDelegate });

            return true;
        }

        private static IntPtr ResolveDll(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr handle = IntPtr.Zero;
            if (libraryName == libtallyworld)
            {
                // Use GlobalSettings.NativeLibraryPath when set.
                string nativeLibraryPath = GetGlobalSettingsNativeLibraryPath();
                if (nativeLibraryPath != null &&
                    TryLoadLibrary(nativeLibraryPath, out handle))
                {
                    return handle;
                }

                // Use Default DllImport resolution.
                if (TryLoadLibrary(libraryName, assembly, searchPath, out handle))
                {
                    return handle;
                }

#if NETFRAMEWORK
#else
                // We cary a number of .so files for Linux which are linked against various
                // libc/OpenSSL libraries. Try them out.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // The libraries are located at 'runtimes/<rid>/native/lib{libraryName}.so'
                    // The <rid> ends with the processor architecture. e.g. fedora-x64.
                    string assemblyDirectory = Path.GetDirectoryName(typeof(TWNativeMethods).Assembly.Location);
                    string processorArchitecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
                    string runtimesDirectory = Path.Combine(assemblyDirectory, "runtimes");

                    if (Directory.Exists(runtimesDirectory))
                    {
                        foreach (var runtimeFolder in Directory.GetDirectories(runtimesDirectory, $"*-{processorArchitecture}"))
                        {
                            string libPath = Path.Combine(runtimeFolder, "native", $"lib{libraryName}.so");
                            if (TryLoadLibrary(libPath, out handle))
                            {
                                return handle;
                            }
                        }
                    }
                }
#endif
            }
            return handle;
        }

        public const int RTLD_NOW = 0x002;

        [DllImport("libdl", EntryPoint = "dlopen")]
        private static extern IntPtr LoadUnixLibrary(string path, int flags);

        [DllImport("kernel32", EntryPoint = "LoadLibrary")]
        private static extern IntPtr LoadWindowsLibrary(string path);

        // Avoid inlining this method because otherwise mono's JITter may try
        // to load the library _before_ we've configured the path.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeNativeLibrary()
        {
            int initCounter;
            try
            {
            }
            finally // avoid thread aborts
            {
                // Initialization can be called multiple times as long as there is a corresponding shutdown to each initialization.
                initCounter = tw_init();
                shutdownObject = new TWNativeShutdownObject();
            }

        }

        // Shutdown the native library in a finalizer.
        private sealed class TWNativeShutdownObject : CriticalFinalizerObject
        {
            ~TWNativeShutdownObject()
            {
                tw_finalize();
            }
        }

        [DllImport(libtallyworld, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe TWError* tw_last_error();

        [DllImport(libtallyworld, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe int tw_init();

        [DllImport(libtallyworld, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe void tw_finalize();

        [DllImport(libtallyworld, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe int tw_post_request();

    }
}