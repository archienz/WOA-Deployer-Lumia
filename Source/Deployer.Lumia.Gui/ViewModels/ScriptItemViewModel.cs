using System.Diagnostics;
using System.Reactive;
using ReactiveUI;

namespace Deployer.Lumia.Gui.ViewModels
{
    public class ScriptItemViewModel : ReactiveObject
    {
        public string Name { get; }
        public string Path { get; }

        public ScriptItemViewModel(string name, string path)
        {
            Name = name;
            Path = path;
            OpenCommand = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrWhiteSpace(Path) || !System.IO.File.Exists(Path) && !System.IO.Directory.Exists(Path))
                {
                    throw new System.InvalidOperationException("Script path does not exist.");
                }

                return Process.Start(new ProcessStartInfo(Path) { UseShellExecute = true });
            });
        }

        public ReactiveCommand<Unit, Process> OpenCommand { get; }
    }
}