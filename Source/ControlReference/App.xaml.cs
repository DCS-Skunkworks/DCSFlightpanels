using System.Diagnostics;
using System.Threading;
using System;
using System.Windows;
using ControlReference.Properties;

namespace ControlReference
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                /*
                 * Load previous application/user settings.
                 */
                if (Settings.Default.UpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeRequired = false;
                    Settings.Default.Save();
                }

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting CTRL-REF." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                Current.Shutdown(0);
                Environment.Exit(0);
            }
        }
    }


}
