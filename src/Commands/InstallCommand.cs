using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace YarnInstaller
{
    internal sealed class InstallCommand
    {
        private readonly Package _package;
        private readonly DTE2 _dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
        private string _cwd;

        private InstallCommand(Package package)
        {
            _package = package;

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var cmdId = new CommandID(PackageGuids.guidInstallCommandPackageCmdSet, PackageIds.InstallCommandId);
                var cmd = new OleMenuCommand(Execute, cmdId);
                cmd.BeforeQueryStatus += BeforeQueryStatus;
                commandService.AddCommand(cmd);
            }
        }

        public static InstallCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new InstallCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            button.Visible = button.Enabled = false;

            var item = _dte.SelectedItems.Item(1)?.ProjectItem;

            if (item == null)
                return;

            string fileName = Path.GetFileName(item.FileNames[1]);

            if (!fileName.Equals("package.json", StringComparison.OrdinalIgnoreCase))
                return;

            _cwd = Path.GetDirectoryName(item.FileNames[1]);
            button.Visible = button.Enabled = true;
        }

        private void Execute(object sender, EventArgs e)
        {
            _dte.StatusBar.Text = "Restoring packages...";
            var start = new ProcessStartInfo("cmd", $"/c yarn install")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _cwd,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            };

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    using (var proc = System.Diagnostics.Process.Start(start))
                    {
                        proc.EnableRaisingEvents = true;
                        proc.OutputDataReceived += OutputDataReceived;
                        proc.ErrorDataReceived += OutputDataReceived;

                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();

                        proc.WaitForExit();
                    }

                    _dte.StatusBar.Text = "Packages restored";
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    _dte.StatusBar.Text = "Package restore failed. See Output Window for details";
                }
            });
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Logger.Log(e.Data);
            }
        }
    }
}
