using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Deployer.Execution;
using Deployer.Services.Wim;
using Deployer.Tasks;
using Serilog;

namespace Deployer.Lumia
{
    public class WoaDeployer : IWoaDeployer
    {
        private readonly IScriptRunner scriptRunner;
        private readonly IScriptParser parser;
        private readonly ITooling tooling;
        private IDeploymentContext context;
        private readonly IFileSystemOperations fileSystemOperations;
        private readonly IOperationContext operationContext;
        private static readonly string BootstrapPath = Path.Combine("Core", "Bootstrap.txt");

        private static readonly string ScriptsDownloadPath = Path.Combine(AppPaths.ArtifactDownload, "Deployment-Scripts");
        private static readonly string VendoredScriptsPath = Path.Combine("Core", "Deployment-Scripts", "Lumia");

        public WoaDeployer(IScriptRunner scriptRunner, IScriptParser parser, ITooling tooling,
            IFileSystemOperations fileSystemOperations, IOperationContext operationContext)
        {
            this.scriptRunner = scriptRunner;
            this.parser = parser;
            this.tooling = tooling;
            this.fileSystemOperations = fileSystemOperations;
            this.operationContext = operationContext;
        }

        private IPhone Phone => (IPhone)context.Device;

        public async Task Deploy(IDeploymentContext deploymentContext)
        {
            context = deploymentContext;
            operationContext.Start();
            await EnsureFullyUnlocked();

            await DownloadDeploymentScripts();
            await RunDeploymentScript();
            await PatchBootManagerIfNeeded();
            await MoveMetadataToPhone();
            await PreparePhoneDiskForSafeRemoval();
        }

        private static string GetScriptsBasePath()
        {
            var vendoredTalkman = Path.Combine(VendoredScriptsPath, "Talkman", "SingleSim.txt");
            if (File.Exists(vendoredTalkman))
            {
                return VendoredScriptsPath;
            }

            return Path.Combine(ScriptsDownloadPath, "Lumia");
        }

        private async Task DownloadDeploymentScripts()
        {
            if (File.Exists(Path.Combine(GetScriptsBasePath(), "Talkman", "SingleSim.txt")))
            {
                Log.Information("Using local Deployment-Scripts (offline). Skipping GitHub fetch.");
                return;
            }

            Log.Information("Local Deployment-Scripts not found. Fetching from GitHub.");
            await RunScript(BootstrapPath);
        }

        private async Task RunDeploymentScript()
        {
            var scriptsBasePath = GetScriptsBasePath();
            var dict = new Dictionary<(PhoneModel, Variant), string>
            {
                {(PhoneModel.Talkman, Variant.SingleSim), Path.Combine(scriptsBasePath, "Talkman", "SingleSim.txt")},
                {(PhoneModel.Cityman, Variant.SingleSim), Path.Combine(scriptsBasePath, "Cityman", "SingleSim.txt")},
                {(PhoneModel.Talkman, Variant.DualSim), Path.Combine(scriptsBasePath, "Talkman", "DualSim.txt")},
                {(PhoneModel.Cityman, Variant.DualSim), Path.Combine(scriptsBasePath, "Cityman", "DualSim.txt")},
            };

            var phoneModel = await Phone.GetModel();
            Log.Verbose("{Model} detected", phoneModel);
            var path = dict[(phoneModel.Model, phoneModel.Variant)];
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Deployment script not found for this phone model. Expected " + path);
            }

            await RunScript(path);
        }

        private async Task RunScript(string path)
        {
            await scriptRunner.Run(parser.Parse(File.ReadAllText(path)));
        }

        private async Task EnsureFullyUnlocked()
        {
            var backUpEfiEsp = await context.Device.GetPartitionByName(PartitionName.BackupEfiesp);
            if (backUpEfiEsp != null)
            {
                throw new InvalidOperationException("Your phone isn't fully unlocked! Please, return to WPInternals and complete the unlock process.");
            }
        }

        private async Task MoveMetadataToPhone()
        {
            try
            {
                var windowsVolume = await context.Device.GetWindowsPartition();
                var destination = Path.Combine(windowsVolume.Root, "Windows", "Logs", "WOA-Deployer");
                await fileSystemOperations.CopyDirectory(AppPaths.Metadata, destination);
                await fileSystemOperations.DeleteDirectory(Path.Combine(AppPaths.Metadata, "Injected Drivers"));
            }
            catch (Exception e)
            {
                Log.Error(e,"Cannot write metadata");
            }
        }

        private async Task PatchBootManagerIfNeeded()
        {
            Log.Debug("Checking if we have to patch WOA's Boot Manager");
            var options = context.DeploymentOptions;
            using (var file = File.OpenRead(options.ImagePath))
            {
                var imageReader = new WindowsImageMetadataReader();
                var windowsImageInfo = imageReader.Load(file);

                var images = windowsImageInfo.Images;
                if (options.ImageIndex < 1 || options.ImageIndex > images.Count)
                {
                    Log.Warning("Image index {Index} is out of range for {Count} images; skipping boot manager patch", options.ImageIndex, images.Count);
                    return;
                }

                var selectedImage = options.ImageIndex - 1;
                if (int.TryParse(images[selectedImage].Build, out var buildNumber))
                {
                    if (buildNumber == 17763)
                    {
                        Log.Verbose("Build 17763 detected. Patching Boot Manager.");
                        var dest = Path.Combine((await Phone.GetSystemPartition()).Root, "EFI", "Boot") + Path.DirectorySeparatorChar;
                        await fileSystemOperations.Copy(@"Core\Boot\bootaa64.efi", dest);
                        Log.Verbose("Boot Manager Patched.");
                    }
                    else
                    {
                        Log.Information("Skipping legacy bootaa64.efi patch for build {Build} (only required for 17763)", buildNumber);
                    }
                }
            }
        }

        private async Task PreparePhoneDiskForSafeRemoval()
        {
            Log.Information("# Preparing phone for safe removal");
            Log.Information("Please wait...");
            var disk = await Phone.GetDeviceDisk();
            await disk.PrepareForRemoval();
        }

        public Task ToggleDualBoot(bool isEnabled)
        {
            return tooling.ToogleDualBoot(isEnabled);
        }
    }
}