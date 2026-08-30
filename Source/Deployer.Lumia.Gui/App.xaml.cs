using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Deployer.Lumia.Gui.Views;
using Deployer.NetFx;
using Deployer.UI;

namespace Deployer.Lumia.Gui
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MahApps.Metro.ThemeManager.IsAutomaticWindowsAppModeSettingSyncEnabled = true;
            MahApps.Metro.ThemeManager.SyncThemeWithWindowsAppModeSetting();

            if (!OS.IsCompatibleWindowsBuild)
            {
                MessageBox.Show(UI.Properties.Resources.IncompatibleWindows10Build, UI.Properties.Resources.IncompatibleWindows10BuildTitle);
                Current.Shutdown();
                return;
            }

            UpdateChecker.CheckForUpdates(AppProperties.GitHubBaseUrl);
            Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

            if (e.Args.Any())
            {
                LaunchConsole(e.Args);
            }
            else
            {
                LaunchGui();
            }
        }
        
        private void LaunchGui()
        {
            var window = new MainWindow();
            MainWindow = window;
            window.Show();            
        }

        private void LaunchConsole(string[] args)
        {
            var consoleExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Deployer.Lumia.Console.exe");
            if (File.Exists(consoleExe))
            {
                var quoted = string.Join(" ", args.Select(a => a.Contains(" ") ? "\"" + a + "\"" : a));
                Process.Start(new ProcessStartInfo(consoleExe, quoted) { UseShellExecute = false });
            }
            else
            {
                MessageBox.Show(
                    "Command-line mode needs Deployer.Lumia.Console.exe next to this app.\nBuild the Console project, or run WoaDeployer with no arguments for the GUI.",
                    "WOA Deployer");
            }

            Shutdown();
        }
    }
}
