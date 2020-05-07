using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.Properties;
using Microsoft.Win32;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.Windows.StreamDeck
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window, IDisposable
    {
        private bool _formLoaded = false;
        private string _panelHash;
        private string _exportFileName = "";



        public ExportWindow(string panelHash)
        {
            InitializeComponent();
            _panelHash = panelHash;
        }

        public void Dispose()
        {
        }

        private void ExportWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }

                LoadButtons();
                SetFormState();
                _formLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

        }

        private void SetFormState()
        {
            ButtonExport.IsEnabled = DataGridStreamDeckButtons.SelectedItems.Count > 0;
            ButtonOpenFile.IsEnabled = !string.IsNullOrEmpty(_exportFileName);
        }

        private void LoadButtons()
        {
            var buttonList = StreamDeckPanel.GetInstance(_panelHash).GetButtonExports();
            DataGridStreamDeckButtons.DataContext = buttonList;
            DataGridStreamDeckButtons.ItemsSource = buttonList;
            DataGridStreamDeckButtons.Items.Refresh();
        }

        private void ButtonSelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridStreamDeckButtons.SelectAll();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonSelectNone_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridStreamDeckButtons.SelectedItems.Clear();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();

                saveFileDialog.InitialDirectory = string.IsNullOrEmpty(Settings.Default.LastStreamDeckExportFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Settings.Default.LastStreamDeckExportFolder;
                saveFileDialog.Filter = @"Stream Deck Export|*.txt";
                saveFileDialog.FileName = "streamdeck_export";

                if (saveFileDialog.ShowDialog() == true)
                {
                    _exportFileName = saveFileDialog.FileName;
                    Settings.Default.LastStreamDeckExportFolder = Path.GetDirectoryName(saveFileDialog.FileName);
                    Settings.Default.Save();

                    var buttonExports = DataGridStreamDeckButtons.SelectedItems.Cast<ButtonExport>().ToList();

                    StreamDeckPanel.GetInstance(_panelHash).Export(_exportFileName, buttonExports);
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void DataGridStreamDeckButtons_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_exportFileName) && File.Exists(_exportFileName))
                {
                    Process.Start(_exportFileName);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ExportWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
