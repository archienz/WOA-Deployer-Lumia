using System;
using System.IO;
using Deployer.Utils;

namespace Deployer.Services
{
    public static class WindowsCommandLineUtils
    {
        public static string BcdEdit { get; } = Path.Combine(GetSystemFolder, "bcdedit.exe");
        public static string BcdBoot { get; } = Path.Combine(GetSystemFolder, "bcdboot.exe");
        public static string Dism { get; } = GetDismPath();

        private static string GetDismPath()
        {
            var hostDism = Path.Combine(GetSystemFolder, "dism.exe");
            var os = Environment.OSVersion.Version;
            if ((os.Major > 10 || (os.Major == 10 && os.Build >= 17763)) && File.Exists(hostDism))
            {
                return hostDism;
            }

            var bundledDism = Path.Combine("Core", "Tools", OsMetadata.Architecture.ToString(), "DISM", "DISM.exe");
            if (File.Exists(bundledDism))
            {
                return bundledDism;
            }

            return hostDism;
        }

        private static string GetSystemFolder
        {
            get
            {
                var sysNativeFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysNative");

                if (Is64OsRunning32Proc())
                {
                    return sysNativeFolder;
                }

                if (ArchitectureInfo.IsArm64())
                {
                    return sysNativeFolder;
                }

                return Path.Combine(Environment.SystemDirectory);
            }
        }

        private static bool Is64OsRunning32Proc()
        {
            return Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess;
        }
    }
}