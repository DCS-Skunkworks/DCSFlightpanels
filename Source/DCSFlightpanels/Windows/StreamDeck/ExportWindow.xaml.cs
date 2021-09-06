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
        private StreamDeckPanel _streamDeckPanel;
        private string _zipFileName = string.Empty;



        public ExportWindow(StreamDeckPanel streamDeckPanel)
        {
            InitializeComponent();
            _streamDeckPanel = streamDeckPanel;
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
            ButtonOpenFile.IsEnabled = !string.IsNullOrEmpty(_zipFileName);
        }

        private void LoadButtons()
        {
            var buttonList = _streamDeckPanel.GetButtonExports();
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
                saveFileDialog.Filter = @"Compressed File|*.zip";
                saveFileDialog.FileName = "streamdeck_export";

                if (saveFileDialog.ShowDialog() == true)
                {
                    _zipFileName = saveFileDialog.FileName;
                    Settings.Default.LastStreamDeckExportFolder = Path.GetDirectoryName(saveFileDialog.FileName);
                    Settings.Default.Save();

                    var buttonExports = DataGridStreamDeckButtons.SelectedItems.Cast<ButtonExport>().ToList();

                    _streamDeckPanel.Export(_zipFileName, buttonExports);
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
                if (DataGridStreamDeckButtons.SelectedItems.Count == 1)
                {
                    var buttonExport = (ButtonExport)DataGridStreamDeckButtons.SelectedItems[0];
                    if (buttonExport != null)
                    {
                        TextBoxLayerName.Text = buttonExport.LayerName;
                        TextBoxButtonName.Text = buttonExport.ButtonName.ToString();
                        TextBoxDescription.Text = buttonExport.ButtonDescription;
                    }
                }
                else
                {
                    TextBoxLayerName.Text = string.Empty;
                    TextBoxButtonName.Text = string.Empty;
                    TextBoxDescription.Text = string.Empty;
                }
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
                if (!string.IsNullOrEmpty(_zipFileName) && File.Exists(_zipFileName))
                {
                    Process.Start(_zipFileName);
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
