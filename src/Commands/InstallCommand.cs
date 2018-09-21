using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;

namespace YarnInstaller
{
    internal sealed class InstallCommand
    {
        private readonly Package _package;

        private InstallCommand(Package package)
        {
            _package = package;

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var cmdId = new CommandID(PackageGuids.guidInstallCommandPackageCmdSet, PackageIds.InstallCommandId);
                var cmd = new OleMenuCommand(Execute, cmdId)
                {
                    Supported = false
                };

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

        private void Execute(object sender, EventArgs e)
        {
            EnvDTE.ProjectItem item = YarnHelper.DTE.SelectedItems.Item(1)?.ProjectItem;

            if (item == null)
                return;

            string fileName = Path.GetFileName(item.FileNames[1]);

            if (!fileName.Equals(Constants.ConfigFileName, StringComparison.OrdinalIgnoreCase))
                return;

            string cwd = Path.GetDirectoryName(item.FileNames[1]);

            YarnHelper.Install(cwd);
        }
    }
}
