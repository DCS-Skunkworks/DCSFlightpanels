using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        }

        private void LoadButtons()
        {
            DataGridStreamDeckButtons.DataContext = StreamDeckButton.GetButtons();
            DataGridStreamDeckButtons.ItemsSource = StreamDeckButton.GetButtons();
            DataGridStreamDeckButtons.Items.Refresh();
        }

        private void ButtonSelectAll_OnClick(object sender, RoutedEventArgs e)
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
                string fileName = "";
                var saveFileDialog = new SaveFileDialog();

                saveFileDialog.InitialDirectory = string.IsNullOrEmpty(Settings.Default.LastStreamDeckExportFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Settings.Default.LastStreamDeckExportFolder;
                saveFileDialog.Filter = @"Stream Deck Export|*.streamdeckexport";

                if (saveFileDialog.ShowDialog() == true)
                {
                    fileName = saveFileDialog.FileName;
                    Settings.Default.LastStreamDeckExportFolder = Path.GetDirectoryName(saveFileDialog.FileName);
                    Settings.Default.Save();

                    var streamDeckButtons = DataGridStreamDeckButtons.SelectedItems.Cast<StreamDeckButton>().ToList();

                    StreamDeckPanel.GetInstance(_panelHash).Export(fileName, streamDeckButtons);
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

        private void DataGridStreamDeckButtons_OnMouseDown(object sender, MouseButtonEventArgs e)
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
    }
}
