using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.Properties;
using Newtonsoft.Json;
using NonVisuals.StreamDeck;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace DCSFlightpanels.Windows.StreamDeck
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window, IDisposable
    {
        private bool _formLoaded = false;
        private readonly string _panelHash;
        private List<StreamDeckButton> _streamDeckButtons = new List<StreamDeckButton>();

        private string _extractedFilesFolder = "";




        public ImportWindow(string panelHash)
        {
            InitializeComponent();
            _panelHash = panelHash;
        }

        public void Dispose()
        {
        }

        private void ImportWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }

                LoadComboBoxButtonName();
                LoadComboBoxLayers();
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
            ButtonImport.IsEnabled = !string.IsNullOrEmpty(TextBoxImageImportFolder.Text) && DataGridStreamDeckButtons.SelectedItems.Count > 0 && PreCheckBeforeImport() && !string.IsNullOrEmpty(ComboBoxLayers.Text);
            ComboBoxButtonName.IsEnabled = DataGridStreamDeckButtons.SelectedItems.Count == 1;
        }

        private void ShowButtons()
        {
            DataGridStreamDeckButtons.DataContext = _streamDeckButtons;
            DataGridStreamDeckButtons.ItemsSource = _streamDeckButtons;
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

        private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var stringBuilder = new StringBuilder(100);

                if (Settings.Default.ShowImportWarning)
                {
                    stringBuilder.Append("WARNING !\n\n");
                    stringBuilder.Append("A failed import can corrupt a bindings file.\n");
                    stringBuilder.Append("Before you import, make a copy of your bindings file");
                    MessageBox.Show(stringBuilder.ToString(), "Make backup", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CopyImagesToNewLocation();

                var selectedStreamDeckButtons = DataGridStreamDeckButtons.SelectedItems.Cast<StreamDeckButton>().ToList(); ;
                var duplicateList = selectedStreamDeckButtons.GroupBy(a => a.StreamDeckButtonName).Where(a => a.Count() > 1).Select(x => new { StreamDeckButtonName = x.Key }).ToList();

                if (duplicateList.Count > 0)
                {
                    var infoText = duplicateList[0].StreamDeckButtonName + " is repeated in the selection. Modify the selection.";
                    MessageBox.Show(infoText, "Duplicate buttons", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                var importMode = EnumButtonImportMode.None;
                if (CheckBoxReplace.IsChecked == true)
                {
                    importMode = EnumButtonImportMode.Replace;
                }
                else if (CheckBoxOverwrite.IsChecked == true)
                {
                    importMode = EnumButtonImportMode.Overwrite;
                }

                StreamDeckPanel.GetInstance(_panelHash).ImportButtons(importMode, ComboBoxLayers.Text, selectedStreamDeckButtons);
                
                MessageBox.Show("Import was completed", "Import successful", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void DataGridStreamDeckButtons_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DataGridStreamDeckButtons.SelectedItems.Count == 0 || DataGridStreamDeckButtons.SelectedItems.Count > 1)
                {
                    SetComboBoxButtonNameValueNone();
                    return;
                }

                var button = (StreamDeckButton)DataGridStreamDeckButtons.SelectedItems[0];
                SetComboBoxButtonNameValue(StreamDeckCommon.ButtonNumber(button.StreamDeckButtonName));
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void Clear()
        {
            _streamDeckButtons.Clear();
            DataGridStreamDeckButtons.Items.Clear();
            SetFormState();
        }

        private void ButtonOpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var zipFileName = "";
                var openFileDialog = new OpenFileDialog();

                openFileDialog.InitialDirectory = string.IsNullOrEmpty(Settings.Default.LastStreamDeckImportFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Settings.Default.LastStreamDeckImportFolder;
                openFileDialog.Filter = @"Compressed File|*.zip";

                if (openFileDialog.ShowDialog() == true)
                {
                    zipFileName = openFileDialog.FileName;
                    Settings.Default.LastStreamDeckImportFolder = Path.GetDirectoryName(openFileDialog.FileName);
                    Settings.Default.Save();
                    ReadFile(zipFileName);
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ReadFile(string filename)
        {
            if (!VerifyImportArchive(filename))
            {
                MessageBox.Show("Archive does not contain button data file " + StreamDeckConstants.BUTTON_EXPORT_FILENAME + ". Choose an other file.", "Invalid export file", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            /*
             * Copy zip to temp folder and work on it there
             */
            StreamDeckCommon.CleanDCSFPTemporaryFolder();
            var tempFolder = StreamDeckCommon.GetDCSFPTemporaryFolder();
            _extractedFilesFolder = tempFolder + "\\extracted_files";
            
            
            File.Copy(filename, tempFolder + "\\" + Path.GetFileName(filename));
            filename = tempFolder + "\\" + Path.GetFileName(filename);

            /*
             * Extract files to folder extracted_files
             */
            ZipArchiver.ExtractZipFile(filename, _extractedFilesFolder);
            
            Clear();

            var fileContents = File.ReadAllText(_extractedFilesFolder + "\\" + StreamDeckConstants.BUTTON_EXPORT_FILENAME);

            TranslateJSON(fileContents);

            ShowButtons();
        }

        private bool VerifyImportArchive(string filename)
        {
            return ZipArchiver.ZipFileContainsFile(filename, StreamDeckConstants.BUTTON_EXPORT_FILENAME);
        }

        private void TranslateJSON(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return;
            }
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            _streamDeckButtons = JsonConvert.DeserializeObject<List<StreamDeckButton>>(jsonText, settings);


        }

        private void SetComboBoxButtonNameValue(int index)
        {
            ComboBoxButtonName.DropDownClosed -= ComboBoxButtonName_OnDropDownClosed;
            ComboBoxButtonName.SelectedItem = ComboBoxButtonName.Items[index - 1];
            ComboBoxButtonName.DropDownClosed += ComboBoxButtonName_OnDropDownClosed;
        }

        private void SetComboBoxButtonNameValueNone()
        {
            ComboBoxButtonName.DropDownClosed -= ComboBoxButtonName_OnDropDownClosed;
            ComboBoxButtonName.Text = "";
            ComboBoxButtonName.DropDownClosed += ComboBoxButtonName_OnDropDownClosed;
        }

        private void LoadComboBoxButtonName()
        {

            ComboBoxButtonName.DropDownClosed -= ComboBoxButtonName_OnDropDownClosed;

            var buttonName = "BUTTON";
            for (var i = 0; i < StreamDeckPanel.GetInstance(_panelHash).ButtonCount; i++)
            {
                ComboBoxButtonName.Items.Add(buttonName + (i + 1).ToString());
            }

            ComboBoxButtonName.DropDownClosed += ComboBoxButtonName_OnDropDownClosed;
        }

        private void ComboBoxButtonName_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                ((StreamDeckButton)DataGridStreamDeckButtons.SelectedItems[0]).StreamDeckButtonName = (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), ComboBoxButtonName.Text);
                DataGridStreamDeckButtons.Items.Refresh();
                DataGridStreamDeckButtons.Items.Refresh();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LoadComboBoxLayers()
        {

            ComboBoxLayers.DropDownClosed -= ComboBoxLayers_OnDropDownClosed;

            var layerList = StreamDeckPanel.GetInstance(_panelHash).GetStreamDeckLayerNames();

            foreach (var layerName in layerList)
            {
                ComboBoxLayers.Items.Add(layerName);
            }

            ComboBoxLayers.DropDownClosed += ComboBoxLayers_OnDropDownClosed;
        }

        private void ComboBoxLayers_OnDropDownClosed(object sender, EventArgs e)
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

        private void ImportWindow_OnKeyDown(object sender, KeyEventArgs e)
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

        private bool PreCheckBeforeImport()
        {
            return true;
        }

        private void CheckBoxOverwrite_CheckedChanged(object sender, RoutedEventArgs e)
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

        private void CheckBoxReplace_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckBoxOverwrite.IsChecked == true)
                {
                    CheckBoxOverwrite.IsChecked = false;
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CheckBoxReplace_OnUnchecked(object sender, RoutedEventArgs e)
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

        private void CheckBoxOverwrite_OnUnchecked(object sender, RoutedEventArgs e)
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

        private void CheckBoxOverwrite_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckBoxReplace.IsChecked == true)
                {
                    CheckBoxReplace.IsChecked = false;
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderBrowserDialog = new FolderBrowserDialog();

                
                folderBrowserDialog.SelectedPath = string.IsNullOrEmpty(Settings.Default.ImageImportFolder)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Settings.Default.ImageImportFolder;
                folderBrowserDialog.ShowNewFolderButton = true;

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Settings.Default.ImageImportFolder = folderBrowserDialog.SelectedPath;
                    Settings.Default.Save();
                    TextBoxImageImportFolder.Text = Common.GetRelativePath(Common.GetApplicationPath(), folderBrowserDialog.SelectedPath);
                    SetNewImageFilePaths(TextBoxImageImportFolder.Text);
                    DataGridStreamDeckButtons.Items.Refresh();
                }

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetNewImageFilePaths(string filePath)
        {
            foreach (var streamDeckButton in _streamDeckButtons)
            {
                if (streamDeckButton.Face != null)
                {
                    if (streamDeckButton.Face.GetType() == typeof(DCSBIOSDecoder))
                    {
                        var decoder = ((DCSBIOSDecoder)streamDeckButton.Face);
                        decoder.SetImageFilePaths(filePath);
                    }
                    else if (streamDeckButton.Face.GetType() == typeof(FaceTypeImage))
                    {
                        var faceTypeImage = ((FaceTypeImage)streamDeckButton.Face);
                        faceTypeImage.ImageFile = Path.Combine(filePath, faceTypeImage.ImageFile);
                    }
                    else if (streamDeckButton.Face.GetType() == typeof(FaceTypeDCSBIOSOverlay))
                    {
                        var faceTypeDCSBIOSOverlay = ((FaceTypeDCSBIOSOverlay)streamDeckButton.Face);
                        faceTypeDCSBIOSOverlay.BackgroundBitmapPath = Path.Combine(filePath, faceTypeDCSBIOSOverlay.BackgroundBitmapPath);
                    }
                }
            }
        }

        private void CopyImagesToNewLocation()
        {
            var extractedFolderDirectoryInfo = new DirectoryInfo(_extractedFilesFolder);
            var filesToCopy = extractedFolderDirectoryInfo.GetFiles();

            foreach (var file in filesToCopy)
            {
                if (!file.Name.EndsWith(".txt"))
                {
                    if (File.Exists(Path.Combine(TextBoxImageImportFolder.Text, file.Name)))
                    {
                        if (MessageBox.Show("Overwrite file " + Path.Combine(TextBoxImageImportFolder.Text, file.Name) + "?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                            MessageBoxResult.Yes)
                        {
                            File.Copy(file.FullName, Path.Combine(TextBoxImageImportFolder.Text, file.Name), true);
                        }
                    }
                    else
                    {
                        File.Copy(file.FullName, Path.Combine(TextBoxImageImportFolder.Text, file.Name));
                    }
                }
            }
        }
    }
}
