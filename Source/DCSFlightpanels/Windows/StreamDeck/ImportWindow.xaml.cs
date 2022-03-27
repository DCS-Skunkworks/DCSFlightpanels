using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels.Windows.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;

    using ClassLibraryCommon;

    using DCSFlightpanels.Properties;

    using MEF;

    using Newtonsoft.Json;

    using NonVisuals.StreamDeck;

    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window, IDisposable
    {
        private readonly string _bindingHash;
        private bool _formLoaded = false;
        private List<ButtonExport> _buttonExports = new List<ButtonExport>();

        private string _extractedFilesFolder = string.Empty;




        public ImportWindow(string bindingHash)
        {
            InitializeComponent();
            _bindingHash = bindingHash;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            ButtonImport.IsEnabled = !string.IsNullOrEmpty(TextBoxImageImportFolder.Text) && DataGridStreamDeckButtons.SelectedItems.Count > 0 && PreCheckBeforeImport();
            ComboBoxButtonName.IsEnabled = DataGridStreamDeckButtons.SelectedItems.Count == 1;

            ButtonImport.Content = "Import" + (DataGridStreamDeckButtons.SelectedItems.Count == 0 ? string.Empty : "(" + DataGridStreamDeckButtons.SelectedItems.Count + ")");
        }

        private void ShowButtons()
        {
            DataGridStreamDeckButtons.DataContext = _buttonExports;
            DataGridStreamDeckButtons.ItemsSource = _buttonExports;
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
                    System.Windows.MessageBox.Show(stringBuilder.ToString(), "Make backup", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CopyFilesToNewLocation();

                var selectedButtonExports = DataGridStreamDeckButtons.SelectedItems.Cast<ButtonExport>().ToList(); ;

                /*var duplicateList = selectedStreamDeckButtons.GroupBy(a => a.StreamDeckButtonName).Where(a => a.Count() > 1).Select(x => new { StreamDeckButtonName = x.Key }).ToList();

                if (duplicateList.Count > 0)
                {
                    var infoText = duplicateList[0].StreamDeckButtonName + " is repeated in the selection. Modify the selection.";
                    MessageBox.Show(infoText, "Duplicate buttons", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                */

                var importMode = EnumButtonImportMode.None;
                if (CheckBoxReplace.IsChecked == true)
                {
                    importMode = EnumButtonImportMode.Replace;
                }
                else if (CheckBoxOverwrite.IsChecked == true)
                {
                    importMode = EnumButtonImportMode.Overwrite;
                }

                StreamDeckPanel.GetInstance(_bindingHash).ImportButtons(importMode, selectedButtonExports);

                System.Windows.MessageBox.Show("Import was completed", "Import successful", MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (DataGridStreamDeckButtons.SelectedItems.Count != 1)
                {
                    SetComboBoxButtonNameValueNone();
                }
                else
                {
                    var buttonExport = (ButtonExport)DataGridStreamDeckButtons.SelectedItems[0];
                    SetComboBoxButtonNameValue(StreamDeckCommon.ButtonNumber(buttonExport.Button.StreamDeckButtonName));
                }

                if (DataGridStreamDeckButtons.SelectedItems.Cast<ButtonExport>().ToList().Select(m => m.LayerName).Distinct().ToList().Count > 1)
                {
                    ComboBoxLayers.Text = string.Empty;
                }
                else if (DataGridStreamDeckButtons.SelectedItems.Count == 1)
                {
                    ComboBoxLayers.Text = ((ButtonExport)DataGridStreamDeckButtons.SelectedItems[0]).LayerName;
                }
                else if (DataGridStreamDeckButtons.SelectedItems.Count == 0)
                {
                    ComboBoxLayers.Text = string.Empty;
                }

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void Clear()
        {
            _buttonExports.Clear();
            ShowButtons();
            SetFormState();
        }

        private void ButtonOpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var zipFileName = string.Empty;
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    InitialDirectory = string.IsNullOrEmpty(Settings.Default.LastStreamDeckImportFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Settings.Default.LastStreamDeckImportFolder,
                    Filter = @"Compressed File|*.zip"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
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
            try
            {

                if (!VerifyImportArchive(filename))
                {
                    System.Windows.MessageBox.Show("Archive does not contain button data file " + StreamDeckConstants.BUTTON_EXPORT_FILENAME + ". Choose an other file.", "Invalid export file", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                /*
                 * Copy zip to temp folder and work on it there
                 */
                StreamDeckCommon.CleanDCSFPTemporaryFolder();
                var tempFolder = StreamDeckCommon.GetDCSFPTemporaryFolder();
                _extractedFilesFolder = tempFolder + "\\extracted_files";

                if (!Directory.Exists(_extractedFilesFolder))
                {
                    Directory.CreateDirectory(_extractedFilesFolder);
                }

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
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
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

            _buttonExports = JsonConvert.DeserializeObject<List<ButtonExport>>(jsonText, settings);
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
            ComboBoxButtonName.Text = string.Empty;
            ComboBoxButtonName.DropDownClosed += ComboBoxButtonName_OnDropDownClosed;
        }

        private void LoadComboBoxButtonName()
        {

            ComboBoxButtonName.DropDownClosed -= ComboBoxButtonName_OnDropDownClosed;

            var buttonName = "BUTTON";
            for (var i = 0; i < StreamDeckPanel.GetInstance(_bindingHash).ButtonCount; i++)
            {
                ComboBoxButtonName.Items.Add(buttonName + (i + 1).ToString());
            }

            ComboBoxButtonName.DropDownClosed += ComboBoxButtonName_OnDropDownClosed;
        }

        private void ComboBoxButtonName_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ComboBoxButtonName.Text) || DataGridStreamDeckButtons.SelectedItems.Count != 1)
                {
                    return;
                }

                var streamDeckButton = ((ButtonExport)DataGridStreamDeckButtons.SelectedItems[0]).Button;
                streamDeckButton.StreamDeckButtonName = (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), ComboBoxButtonName.Text);
                DataGridStreamDeckButtons.Items.Refresh();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ComboBoxLayers_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ComboBoxLayers.Text) || DataGridStreamDeckButtons.SelectedItems.Count == 0)
                {
                    return;
                }

                foreach (var selectedItem in DataGridStreamDeckButtons.SelectedItems)
                {
                    var buttonExport = (ButtonExport)selectedItem;
                    buttonExport.LayerName = ComboBoxLayers.Text;
                }

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

            var layerList = StreamDeckPanel.GetInstance(_bindingHash).GetStreamDeckLayerNames();

            foreach (var layerName in layerList)
            {
                ComboBoxLayers.Items.Add(layerName);
            }

            ComboBoxLayers.DropDownClosed += ComboBoxLayers_OnDropDownClosed;
        }

        private void ImportWindow_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
                var folderBrowserDialog = new FolderBrowserDialog()
                {
                    Description = @"Select location to where files (images, sounds) will be saved.",
                    SelectedPath = string.IsNullOrEmpty(Settings.Default.ImageImportFolder)
                        ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                        : Settings.Default.ImageImportFolder,
                    ShowNewFolderButton = false
                };

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Settings.Default.ImageImportFolder = folderBrowserDialog.SelectedPath;
                    Settings.Default.Save();
                    TextBoxImageImportFolder.Text = Common.GetRelativePath(Common.GetApplicationPath(), folderBrowserDialog.SelectedPath);
                    SetNewFilePaths(TextBoxImageImportFolder.Text);
                    DataGridStreamDeckButtons.Items.Refresh();
                }

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetNewFilePaths(string filePath)
        {
            foreach (var buttonExport in _buttonExports)
            {
                var streamDeckButton = buttonExport.Button;

                if (streamDeckButton.ActionForPress != null && streamDeckButton.ActionForPress.HasSound)
                {
                    streamDeckButton.ActionForPress.SoundFile = Path.Combine(filePath, Path.GetFileName(streamDeckButton.ActionForPress.SoundFile));
                }

                if (streamDeckButton.ActionForRelease != null && streamDeckButton.ActionForRelease.HasSound)
                {
                    streamDeckButton.ActionForRelease.SoundFile = Path.Combine(filePath, Path.GetFileName(streamDeckButton.ActionForRelease.SoundFile));
                }

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
                        faceTypeImage.ImageFile = Path.Combine(filePath, Path.GetFileName(faceTypeImage.ImageFile));
                    }
                    else if (streamDeckButton.Face.GetType() == typeof(FaceTypeDCSBIOSOverlay))
                    {
                        var faceTypeDCSBIOSOverlay = ((FaceTypeDCSBIOSOverlay)streamDeckButton.Face);
                        faceTypeDCSBIOSOverlay.BackgroundBitmapPath = Path.Combine(filePath, Path.GetFileName(faceTypeDCSBIOSOverlay.BackgroundBitmapPath));
                    }
                }
            }
        }

        private void CopyFilesToNewLocation()
        {
            var extractedFolderDirectoryInfo = new DirectoryInfo(_extractedFilesFolder);
            var filesToCopy = extractedFolderDirectoryInfo.GetFiles();
            var show = false;
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Following files exists in target folder and were not copied :\n");

            foreach (var file in filesToCopy)
            {
                if (!file.Name.EndsWith(".txt"))
                {
                    if (File.Exists(Path.Combine(TextBoxImageImportFolder.Text, file.Name)))
                    {
                        stringBuilder.Append(file.Name).Append("\n");
                        show = true;
                    }
                    else
                    {
                        File.Copy(file.FullName, Path.Combine(TextBoxImageImportFolder.Text, file.Name));
                    }
                }
            }

            if (show)
            {
                System.Windows.MessageBox.Show(stringBuilder.ToString(), "File already exists", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
