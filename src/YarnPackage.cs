using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace YarnInstaller
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidPackageString)]
    [ProvideOptionPage(typeof(Options), "Web", Vsix.Name, 101, 102, true, new string[0], ProvidesLocalizedCategoryName = false)]
    [ProvideAutoLoad(PackageGuids.guidAutoloadString)]
    [ProvideUIContextRule(PackageGuids.guidAutoloadString,
        name: "package.json",
        expression: "config",
        termNames: new[] { "config" },
        termValues: new[] { "HierSingleSelectionName:package.json$" })]
    public sealed class YarnPackage : Package
    {
        private static Options _options;
        private static object _syncRoot = new object();

        public static Options Options
        {
            get
            {
                if (_options == null)
                {
                    lock (_syncRoot)
                    {
                        if (_options == null)
                        {
                            EnsurePackageLoaded();
                        }
                    }
                }

                return _options;
            }
        }

        protected override void Initialize()
        {
            _options = (Options)GetDialogPage(typeof(Options));
            InstallCommand.Initialize(this);
        }

        private static void EnsurePackageLoaded()
        {
            var shell = (IVsShell)GetGlobalService(typeof(SVsShell));

            if (shell.IsPackageLoaded(ref PackageGuids.guidPackage, out IVsPackage package) != VSConstants.S_OK)
            {
                ErrorHandler.Succeeded(shell.LoadPackage(ref PackageGuids.guidPackage, out package));
            }
        }
    }
}
