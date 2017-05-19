using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;

namespace YarnInstaller
{
    internal sealed class InstallCommand
    {
        private readonly Package _package;
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

            var item = YarnHelper.DTE.SelectedItems.Item(1)?.ProjectItem;

            if (item == null)
                return;

            string fileName = Path.GetFileName(item.FileNames[1]);

            if (!fileName.Equals(Constants.ConfigFileName, StringComparison.OrdinalIgnoreCase))
                return;

            _cwd = Path.GetDirectoryName(item.FileNames[1]);
            button.Visible = button.Enabled = true;
        }

        private void Execute(object sender, EventArgs e)
        {
            YarnHelper.Install(_cwd);
        }
    }
}
