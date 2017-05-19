using System;
using YarnInstaller;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Threading;

internal static class Logger
{
    private static IVsOutputWindowPane _pane;
    private static IVsOutputWindow _output = (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));

    public static void Log(object message)
    {
        try
        {
            ThreadHelper.Generic.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
            {
                if (EnsurePane())
                {
                    _pane.OutputString(message + Environment.NewLine);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.Write(ex);
        }
    }

    private static bool EnsurePane()
    {
        if (_pane == null)
        {
            var guid = Guid.NewGuid();
            _output.CreatePane(ref guid, Vsix.Name, 1, 1);
            _output.GetPane(ref guid, out _pane);
        }

        return _pane != null;
    }
}