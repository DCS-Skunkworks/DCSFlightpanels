using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using DCSFlightpanels.Properties;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;
        private bool _hasHandle;
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        private void InitNotificationIcon()
        {
            System.Windows.Forms.MenuItem notifyIconContextMenuShow = new System.Windows.Forms.MenuItem
            {
                Index = 0,
                Text = "Show"
            };
            notifyIconContextMenuShow.Click += new EventHandler(NotifyIcon_Show);

            System.Windows.Forms.MenuItem notifyIconContextMenuQuit = new System.Windows.Forms.MenuItem
            {
                Index = 1,
                Text = "Quit"
            };
            notifyIconContextMenuQuit.Click += new EventHandler(NotifyIcon_Quit);

            System.Windows.Forms.ContextMenu notifyIconContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIconContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { notifyIconContextMenuShow, notifyIconContextMenuQuit });

            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = DCSFlightpanels.Properties.Resources.flightpanels02_8Rc_icon,
                Visible = true,
                ContextMenu = notifyIconContextMenu
            };
            _notifyIcon.DoubleClick += new EventHandler(NotifyIcon_Show);

        }

        private void NotifyIcon_Show(object sender, EventArgs args)
        {
            MainWindow?.Show();
            if (MainWindow != null)
            {
                MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void NotifyIcon_Quit(object sender, EventArgs args)
        {
            MainWindow?.Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                InitNotificationIcon();

                Settings.Default.LoadStreamDeck = true; //Default is loading Stream Deck.
                Settings.Default.Save();

                //DCSFlightpanels.exe OpenProfile="C:\Users\User\Documents\Spitfire_Saitek_DCS_Profile.bindings"
                //DCSFlightpanels.exe OpenProfile='C:\Users\User\Documents\Spitfire_Saitek_DCS_Profile.bindings'

                //1 Check for start arguments.
                //2 If argument and profile exists close running instance, start this with profile chosen
                var closeCurrentInstance = false;

                if (e != null)
                {
                    try
                    {

                        for (int i = 0; i < e.Args.Length; i++)
                        {
                            var arg = e.Args[i];
                            if (arg.Contains("OpenProfile") && e.Args[0].Contains("="))
                            {
                                Settings.Default.LastProfileFileUsed = e.Args[i + 1].Replace("\"", "").Replace("'", "");
                                Settings.Default.RunMinimized = true;
                                Settings.Default.Save();
                                closeCurrentInstance = true;
                            }
                            else if (arg.ToLower().Contains("-nostreamdeck"))
                            {
                                Settings.Default.LoadStreamDeck = false;
                                Settings.Default.Save();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid startup arguments." + Environment.NewLine + ex.Message);
                        throw;
                    }
                }

                // get application GUID as defined in AssemblyInfo.cs
                var appGuid = "{23DB8D4F-D76E-4DF4-B04F-4F4EB0A8E992}";

                // unique id for global mutex - Global prefix means it is global to the machine
                string mutexId = "Global\\" + appGuid;

                // Need a place to store a return value in Mutex() constructor call
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);

                _mutex = new Mutex(false, mutexId, out var createdNew, securitySettings);

                _hasHandle = false;
                try
                {
                    _hasHandle = _mutex.WaitOne(2000, false);
                }
                catch (AbandonedMutexException)
                {
                    // Log the fact that the mutex was abandoned in another process,
                    // it will still get acquired
                    //_hasHandle = true;
                }

                if (!closeCurrentInstance && !_hasHandle)
                {
                    MessageBox.Show("DCSFlightpanels is already running..");
                    Current.Shutdown(0);
                    Environment.Exit(0);
                }
                if (closeCurrentInstance && !_hasHandle)
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        if (process.ProcessName.Equals(Process.GetCurrentProcess().ProcessName) && process.Id != Process.GetCurrentProcess().Id)
                        {
                            process.Kill();
                            break;
                        }
                    }
                    // Wait for process to close
                    Thread.Sleep(2000);
                }
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting DCSFlightpanels." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                Current.Shutdown(0);
                Environment.Exit(0);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Visible = false;
            if (_hasHandle)
            {
                _mutex?.ReleaseMutex();
            }
            base.OnExit(e);
        }
    }
}
