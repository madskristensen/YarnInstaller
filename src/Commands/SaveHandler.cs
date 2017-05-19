using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace YarnInstaller.Commands
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("JSON")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class SaveHandler : IWpfTextViewCreationListener
    {
        [Import]
        public ITextDocumentFactoryService DocumentService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (YarnHelper.DTE.Version == "14.0") // VS2015 not supported since it can't disable "npm install"
                return;

            if (DocumentService.TryGetTextDocument(textView.TextBuffer, out ITextDocument doc))
            {
                string fileName = Path.GetFileName(doc.FilePath);

                if (fileName.Equals(Constants.ConfigFileName, StringComparison.OrdinalIgnoreCase))
                {
                    doc.FileActionOccurred += OnSave;
                    textView.Closed += TextViewClosed;
                }
            }
        }

        private void OnSave(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType == FileActionTypes.ContentSavedToDisk)
            {
                if (YarnPackage.Options != null && YarnPackage.Options.InstallOnSave)
                {
                    string cwd = Path.GetDirectoryName(e.FilePath);
                    YarnHelper.Install(cwd);
                }
            }
        }

        private void TextViewClosed(object sender, EventArgs e)
        {
            var textView = (IWpfTextView)sender;

            if (DocumentService.TryGetTextDocument(textView.TextBuffer, out ITextDocument doc))
            {
                doc.FileActionOccurred -= OnSave;
            }
        }
    }
}
