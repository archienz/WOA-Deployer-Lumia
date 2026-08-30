using System.Reflection;

namespace Deployer
{
    public class AppVersionMixin
    {
        public static string VersionString
        {
            get
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                {
                    return "{None}";
                }

                var version = entryAssembly.GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        private static string Format(int versionBuild)
        {
            if (versionBuild == 0)
            {
                return "";
            }

            return "." + versionBuild;
        }
    }
}