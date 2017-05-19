using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace YarnInstaller
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidPackageString)]
    [ProvideAutoLoad(PackageGuids.guidAutoloadString)]
    [ProvideUIContextRule(PackageGuids.guidAutoloadString,
        name: "package.json",
        expression: "config",
        termNames: new[] { "config" },
        termValues: new[] { "HierSingleSelectionName:package.json$" })]
    public sealed class YarnPackage : Package
    {
        protected override void Initialize()
        {
            InstallCommand.Initialize(this);
        }
    }
}
