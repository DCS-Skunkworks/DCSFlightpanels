using System;
using System.Diagnostics;
using System.IO;
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
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                //DCSFlightpanels.exe OpenProfile="C:\Users\User\Documents\Spitfire_Saitek_DCS_Profile.bindings"

                //1 Check for start arguments.
                //2 If argument and profile exists close running instance, start this with profile chosen
                var closeCurrentInstance = false;
                try
                {
                    if (e.Args.Length > 0)
                    {
                        var array = e.Args[0].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (array[0].Equals("OpenProfile") && File.Exists(array[1]))
                        {
                            Settings.Default.LastProfileFileUsed = array[1].Replace("\"", "");
                            closeCurrentInstance = true;
                        }
                        else
                        {
                            MessageBox.Show("Invalid startup arguments." + Environment.NewLine + array[0] + Environment.NewLine + array[1]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing startup arguments." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                }


                const string appName = "DCSFlightpanels.exe";
                if (!closeCurrentInstance)
                {
                    var mutex = new Mutex(true, appName, out var createdNew);
                    mutex.Close();
                    if (!createdNew)
                    {
                        //app is already running! Exiting the application  
                        Current.Shutdown();
                        MessageBox.Show("DCSFlightpanels is already running..");
                    }
                    else
                    {
                        base.OnStartup(e);
                    }
                }
                else
                {
                    var mutex = new Mutex(true, appName, out var createdNew);
                    try
                    {
                        var tryAgain = true;
                        while (tryAgain)
                        {
                            if (createdNew)
                            {
                                // Run the application
                                tryAgain = false;
                                base.OnStartup(e);
                            }
                            else
                            {
                                try
                                {
                                    createdNew = mutex.WaitOne(0, false);
                                }
                                catch (AbandonedMutexException)
                                {
                                    createdNew = true;
                                }
                            }

                            if (!createdNew)
                            {
                                foreach (var process in Process.GetProcesses())
                                {
                                    if (process.ProcessName.Equals(Process.GetCurrentProcess().ProcessName) && process.Id != Process.GetCurrentProcess().Id)
                                    {
                                        Debug.Print(process.ProcessName);
                                        process.Kill();
                                        break;
                                    }
                                }
                                // Wait for process to close
                                Thread.Sleep(2000);
                            }
                        }
                    }
                    finally
                    {
                        mutex.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting DCSFlightpanels." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
