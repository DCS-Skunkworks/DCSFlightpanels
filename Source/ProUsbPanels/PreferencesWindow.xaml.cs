using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProUsbPanels.Properties;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {

        private PupPreferences _pupPreferences = new PupPreferences();
        private bool _dataChanged = false;

        public PreferencesWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            SetWindowState();
        }
        
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if(!String.IsNullOrEmpty(Settings.Default.DefaultPreferencesFile))
                {
                    _pupPreferences.PreferencesFileAndPath = Settings.Default.DefaultPreferencesFile;
                    ReadFile();
                    checkBoxWarnDuplicates.Checked += CheckBoxWarnDuplicatesChecked;
                    checkBoxWarnDuplicates.Unchecked += CheckBoxWarnDuplicatesUnchecked;
                    textBoxDefaultDirectory.TextChanged += TextBoxDefaultDirectoryTextChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetWindowState()
        {
            try
            {
                buttonSavePreferences.IsEnabled = _pupPreferences != null && !String.IsNullOrEmpty(_pupPreferences.PreferencesFileAndPath) && _dataChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonChooseDirectoryClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.RestoreDirectory = true;


                string initialDirectory;
                if(!String.IsNullOrEmpty(textBoxDefaultDirectory.Text))
                {
                    initialDirectory = System.IO.Path.GetFullPath(textBoxDefaultDirectory.Text);
                }else
                {
                    initialDirectory = System.IO.Path.GetFullPath((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
                }
                saveFileDialog.InitialDirectory = initialDirectory;
                saveFileDialog.FileName = "default_settings";
                saveFileDialog.DefaultExt = ".pupsettings";
                saveFileDialog.Filter = "ProUsbPanelSettings (.pupsettings)|*.pupsettings";

                if (saveFileDialog.ShowDialog() == true)
                {
                    if (checkBoxWarnDuplicates.IsChecked == null)
                    {
                        checkBoxWarnDuplicates.IsChecked = false;
                    }
                    _pupPreferences.WarnForDuplicateConfigurations = (bool) checkBoxWarnDuplicates.IsChecked;
                    _pupPreferences.PreferencesFileAndPath = saveFileDialog.FileName;
                    textBoxDefaultDirectory.Text = _pupPreferences.PreferencesFileAndPath;
                    SaveFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonCloseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SaveFile()
        {
            try
            {
                Settings.Default.DefaultPreferencesFile = _pupPreferences.PreferencesFileAndPath;
                Settings.Default.Save();
                
                File.WriteAllText(_pupPreferences.PreferencesFileAndPath, _pupPreferences.ExportString());
                _dataChanged = false;
                SetWindowState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonSavePreferencesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFile();
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
                var fileContents = File.ReadAllText(_pupPreferences.PreferencesFileAndPath, Encoding.UTF8);
                _pupPreferences.ImportString(fileContents);
                textBoxDefaultDirectory.Text = _pupPreferences.PreferencesFileAndPath;
                checkBoxWarnDuplicates.IsChecked = _pupPreferences.WarnForDuplicateConfigurations;
                SetWindowState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckBoxWarnDuplicatesChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _pupPreferences.WarnForDuplicateConfigurations = true;
                _dataChanged = true;
                SetWindowState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckBoxWarnDuplicatesUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _pupPreferences.WarnForDuplicateConfigurations = false;
                _dataChanged = true;
                SetWindowState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBoxDefaultDirectoryTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _dataChanged = true;
                SetWindowState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
