using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Text;

namespace YarnInstaller
{
    public static class YarnHelper
    {
        private static bool _isInstalling;
        public static DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        public static void Install(string cwd)
        {
            if (_isInstalling)
            {
                WriteStatus("Install already in progress. Wait till it finishes");
                return;
            }

            _isInstalling = true;
            WriteStatus("Restoring packages...");

            var start = new ProcessStartInfo("cmd", $"/c yarn install")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = cwd,
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

                      WriteStatus("Packages restored");
                  }
                  catch (Exception ex)
                  {
                      Logger.Log(ex);
                      WriteStatus("Package restore failed. See Output Window for details");
                  }
                  finally
                  {
                      _isInstalling = false;
                  }
              });
        }

        private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Logger.Log(e.Data);
            }
        }

        private static void WriteStatus(string status)
        {
            ThreadHelper.Generic.BeginInvoke(() => {
                DTE.StatusBar.Text = status;
            });
        }
    }
}
