using System;
using System.IO;
using System.Windows.Forms;

namespace OHTPmSimulatorV5;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            string errorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StartupError.txt");
            File.WriteAllText(errorPath, ex.ToString());

            MessageBox.Show(
                "The application failed to start.\n\nError saved to:\n" + errorPath,
                "Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
