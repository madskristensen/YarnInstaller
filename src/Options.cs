using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace YarnInstaller
{
    public class Options : DialogPage
    {
        [Category("General")]
        [DisplayName("Install on save")]
        [Description("Automatically run \"yarn install\" when package.json is saved")]
        [DefaultValue(false)]
        public bool InstallOnSave { get; set; }
    }
}
