using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSInteroperabilityConsole
{
    internal static class GlobalSettings
    {
        private static readonly bool nativeLibraryPathAllowed;

        private static string nativeLibraryPath;
        private static bool nativeLibraryPathLocked;
        private static readonly string nativeLibraryDefaultPath;

        static GlobalSettings()
        {
            bool netFX = Platform.IsRunningOnNetFramework();
            bool netCore = Platform.IsRunningOnNetCore();

            nativeLibraryPathAllowed = netFX || netCore;

            if (netFX)
            {
                // For .NET Framework apps the dependencies are deployed to lib/win32/{architecture} directory
                nativeLibraryDefaultPath = Path.Combine(GetExecutingAssemblyDirectory(), "lib", "win32");
            }
            else
            {
                nativeLibraryDefaultPath = null;
            }

        }

        private static string GetExecutingAssemblyDirectory()
        {
            // Assembly.CodeBase is not actually a correctly formatted
            // URI.  It's merely prefixed with `file:///` and has its
            // backslashes flipped.  This is superior to EscapedCodeBase,
            // which does not correctly escape things, and ambiguates a
            // space (%20) with a literal `%20` in the path.  Sigh.
            var managedPath = Assembly.GetExecutingAssembly().CodeBase;
            if (managedPath == null)
            {
                managedPath = Assembly.GetExecutingAssembly().Location;
            }
            else if (managedPath.StartsWith("file:///"))
            {
                managedPath = managedPath.Substring(8).Replace('/', '\\');
            }
            else if (managedPath.StartsWith("file://"))
            {
                managedPath = @"\\" + managedPath.Substring(7).Replace('/', '\\');
            }

            managedPath = Path.GetDirectoryName(managedPath);
            return managedPath;
        }


        /// <summary>
        /// Sets a path for loading native binaries on .NET Framework or .NET Core.
        /// When specified, native library will first be searched under the given path.
        /// On .NET Framework a subdirectory corresponding to the architecture  ("x86" or "x64") is appended,
        /// otherwise the native library is expected to be found in the directory as specified.
        ///
        /// If the library is not found it will be searched in standard search paths:
        /// <see cref="DllImportSearchPath.AssemblyDirectory"/>,
        /// <see cref="DllImportSearchPath.ApplicationDirectory"/> and
        /// <see cref="DllImportSearchPath.SafeDirectories"/>.
        /// <para>
        /// This must be set before any other calls to the library,
        /// and is not available on other platforms than .NET Framework and .NET Core.
        /// </para>
        /// <para>
        /// If not specified on .NET Framework it defaults to lib/win32 subdirectory
        /// of the directory where this assembly is loaded from.
        /// </para>
        /// </summary>
        public static string NativeLibraryPath
        {
            get
            {
                if (!nativeLibraryPathAllowed)
                {
                    throw new TallyWorldException("Querying the native hint path is only supported on .NET Framework and .NET Core platforms");
                }

                return nativeLibraryPath ?? nativeLibraryDefaultPath;
            }

            set
            {
                if (!nativeLibraryPathAllowed)
                {
                    throw new TallyWorldException("Setting the native hint path is only supported on .NET Framework and .NET Core platforms");
                }

                if (nativeLibraryPathLocked)
                {
                    throw new TallyWorldException("You cannot set the native library path after it has been loaded");
                }

                try
                {
                    nativeLibraryPath = Path.GetFullPath(value);
                }
                catch (Exception e)
                {
                    throw new TallyWorldException(e.Message);
                }
            }
        }

        internal static string GetAndLockNativeLibraryPath()
        {
            nativeLibraryPathLocked = true;
            string result = nativeLibraryPath ?? nativeLibraryDefaultPath;
            return Platform.IsRunningOnNetFramework() ? Path.Combine(result, Platform.ProcessorArchitecture) : result;
        }

    }


}