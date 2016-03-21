using System;
using System.IO;
using System.Text;
using System.Windows;
using ProUsbPanels.Properties;

namespace ProUsbPanels
{
    public class PupPreferences
    {
        private String _preferencesFileAndPath;
        private bool _warnForDuplicateConfigurations;

        public string DefaultDirectory()
        {
            return Path.GetDirectoryName(_preferencesFileAndPath);
        }

        public string PreferencesFileAndPath
        {
            get { return _preferencesFileAndPath; }
            set { _preferencesFileAndPath = value; }
        }

        public bool WarnForDuplicateConfigurations
        {
            get { return _warnForDuplicateConfigurations; }
            set { _warnForDuplicateConfigurations = value; }
        }

        public String ExportString()
        {
            try
            {
                return "PupPreferences{" + _preferencesFileAndPath + "," + (_warnForDuplicateConfigurations ? Boolean.TrueString : Boolean.FalseString) + "}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

        public void ImportString(String str)
        {
            try
            {
                if (String.IsNullOrEmpty(str))
                {
                    throw new ArgumentException("Import string empty. (PupPreferences)");
                }
                if (!str.StartsWith("PupPreferences{") || !str.EndsWith("}"))
                {
                    throw new ArgumentException("Import string format exception. (PupPreferences) >" + str + "<");
                }
                //PupPreferences{C:\Users\John\Documents\default_preferences.pupsettings,true}
                var dataString = str.Remove(0, 15);
                //C:\Users\John\Documents\default_preferences.pupsettings,true}
                dataString = dataString.Remove(dataString.Length - 1, 1);
                //C:\Users\John\Documents\default_preferences.pupsettings,true
                _preferencesFileAndPath = dataString.Substring(0, dataString.IndexOf(",", StringComparison.Ordinal));
                _warnForDuplicateConfigurations = Boolean.Parse(dataString.Substring(dataString.IndexOf(",", StringComparison.Ordinal) + 1));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        public void ReadFile()
        {
            try
            {
                if(!String.IsNullOrEmpty(Settings.Default.DefaultPreferencesFile))
                {
                    var fileContents = File.ReadAllText(Settings.Default.DefaultPreferencesFile, Encoding.UTF8);
                    ImportString(fileContents);
                }else
                {
                    var messageBoxText = "Default preferences not found in {My documents}. Browse for preferences file?";
                    var caption = "Preferences not found";

                    var messageBox = MessageBoxButton.YesNo;
                    var messageBoxIcon = MessageBoxImage.Warning;

                    var messageBoxResult = MessageBox.Show(messageBoxText, caption, messageBox, messageBoxIcon);

                    switch (messageBoxResult)
                    {
                        case MessageBoxResult.Yes:
                            OpenFile();
                            break;

                        case MessageBoxResult.No:
                            MessageBox.Show("Please go to \"Edit\" -> \"Preferences\" and specify the default directory for the preference file");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenFile()
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.RestoreDirectory = true;
                openFileDialog.FileName = "default_settings";
                openFileDialog.DefaultExt = ".pupsettings";
                openFileDialog.Filter = "ProUsbPanelSettings (.pupsettings)|*.pupsettings";

                if (openFileDialog.ShowDialog() == true)
                {
                    var fileContents = File.ReadAllText(Settings.Default.DefaultPreferencesFile, Encoding.UTF8);
                    ImportString(fileContents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
