using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.Shared;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;
using Color = System.Drawing.Color;
using Cursors = System.Windows.Input.Cursors;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// This StreamDeck implementation is a big clusterf*ck.
    /// </summary>
    public partial class StreamDeckDCSBIOSDecoderWindow : Window, IIsDirty
    {
        private string _streamDeckInstanceId;
        private bool _formLoaded;
        private readonly string _typeToSearch = "Type to search control";
        private Popup _popupSearch;
        private DataGrid _popupDataGrid;
        private IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private DCSBIOSControl _dcsbiosControl;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private Dictionary<string, double> _variables = new Dictionary<string, double>();
        private string _decoderResult;
        private bool _isDirty = false;
        private bool _populatingData = false;

        private DCSBIOSDecoder _dcsbiosDecoder = null;

        private bool _exitThread;

        private readonly string _formulaFile = AppDomain.CurrentDomain.BaseDirectory + "\\formulas.txt";





        public StreamDeckDCSBIOSDecoderWindow(DCSBIOSDecoder dcsbiosDecoder, string streamDeckInstanceId)
        {
            InitializeComponent();
            _dcsbiosDecoder = dcsbiosDecoder;
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
            _streamDeckInstanceId = streamDeckInstanceId;
            var thread = new Thread(ThreadLoop);
            thread.Start();
        }

        public StreamDeckDCSBIOSDecoderWindow(string streamDeckInstanceId)
        {
            InitializeComponent();
            _dcsbiosDecoder = new DCSBIOSDecoder();
            _dcsbiosDecoder.StreamDeckInstanceId = streamDeckInstanceId;
            LoadDefaults();
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
            _dcsbiosDecoder.StreamDeckButtonName = StreamDeckPanel.GetInstance(streamDeckInstanceId).SelectedButtonName;
            _streamDeckInstanceId = streamDeckInstanceId;
            var thread = new Thread(ThreadLoop);
            thread.Start();
        }

        private void StreamDeckDCSBIOSDecoderWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }

                ShowDecoder();
                _dcsbiosDecoder.IsVisible = true;
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _popupDataGrid = ((DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "PopupDataGrid"));
                _formLoaded = true;
                SetFormState();
                TextBoxSearch.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }

            StackPanelRawTextAndStyle.Visibility = RadioButtonOutputRaw.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            StackPanelConverters.Visibility = RadioButtonOutputConvert.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
            StackPanelConverters.IsEnabled = RadioButtonOutputConvert.IsChecked == true;
            ButtonAddConverter.IsEnabled = RadioButtonOutputConvert.IsChecked == true;

            ButtonEditConverter.IsEnabled = DataGridConverters.SelectedItems.Count == 1;
            ButtonDeleteConverter.IsEnabled = DataGridConverters.SelectedItems.Count == 1;


            GroupBoxFormula.IsEnabled = CheckBoxUseFormula.IsChecked == true;

            CheckBoxStringAsNumber.IsEnabled = RadioButtonStringSource.IsChecked == true;
            CheckBoxUseFormula.IsEnabled = (RadioButtonIntegerSource.IsChecked == true) ||
                                           (RadioButtonStringSource.IsChecked == true && CheckBoxStringAsNumber.IsChecked == true);

            if (RadioButtonIntegerSource.IsChecked == true)
            {
                CheckBoxStringAsNumber.IsChecked = false;
            }

            if (RadioButtonStringSource.IsChecked == true && CheckBoxStringAsNumber.IsChecked == false)
            {
                CheckBoxUseFormula.IsChecked = false;
            }

            ButtonSave.IsEnabled = _dcsbiosDecoder.DecoderConfigurationOK() && !string.IsNullOrEmpty(TextBoxDCSBIOSId.Text);
        }

        private void ShowDecoder()
        {
            _populatingData = true;

            DCSBIOSDecoder.ShowOnly(_dcsbiosDecoder, _streamDeckInstanceId);
            if (_dcsbiosDecoder.DCSBiosOutputType == DCSBiosOutputType.INTEGER_TYPE)
            {
                RadioButtonIntegerSource.IsChecked = true;
            }
            else if (_dcsbiosDecoder.DCSBiosOutputType == DCSBiosOutputType.STRING_TYPE)
            {
                RadioButtonStringSource.IsChecked = true;
            }

            switch (_dcsbiosDecoder.DecoderOutputType)
            {
                case EnumDCSBIOSDecoderOutputType.Raw:
                    {
                        TextBoxOutputTextRaw.Text = _dcsbiosDecoder.ButtonTextTemplate;
                        RadioButtonOutputRaw.IsChecked = true;
                        break;
                    }
                case EnumDCSBIOSDecoderOutputType.Converter:
                    {
                        RadioButtonOutputConvert.IsChecked = true;
                        break;
                    }
            }
            CheckBoxStringAsNumber.IsChecked = _dcsbiosDecoder.TreatStringAsNumber;

            CheckBoxUseFormula.IsChecked = _dcsbiosDecoder.UseFormula;
            if (_dcsbiosDecoder.UseFormula)
            {
                CheckBoxUseFormula.IsChecked = true;
                TextBoxFormula.Text = string.IsNullOrEmpty(_dcsbiosDecoder.Formula) ? "" : _dcsbiosDecoder.Formula;
            }

            if (_dcsbiosDecoder.DCSBIOSOutput != null)
            {
                TextBoxDCSBIOSId.Text = _dcsbiosDecoder.DCSBIOSOutput.ControlId;
            }
            ShowConverters();
            UpdateFontInfo();
            _populatingData = false;
        }

        private void ThreadLoop()
        {
            try
            {
                while (!_exitThread)
                {
                    if (_dcsbiosDecoder.ValueUpdated)
                    {
                        try
                        {
                            SetFormulaError(_dcsbiosDecoder.HasErrors ? _dcsbiosDecoder.LastFormulaError : "");
                            SetFormulaResult(_dcsbiosDecoder.FormulaResult);
                            SetRawDCSBIOSValue(_dcsbiosDecoder.UintDcsBiosValue);
                        }
                        catch (Exception e)
                        {
                            SetFormulaError(e.Message);
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                LabelErrors.Content = "Failed to start thread " + ex.Message;
            }
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_popupDataGrid.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_popupDataGrid.SelectedItem;
                    _dcsbiosDecoder.DCSBIOSOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(_dcsbiosControl.identifier);

                    TextBoxDCSBIOSId.Text = _dcsbiosControl.identifier;
                    TextBoxSearch.Text = _typeToSearch;
                    SetIsDirty();
                    SetFormState();
                }
                _popupSearch.IsOpen = false;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void AdjustShownPopupData(TextBox textBox)
        {
            _popupSearch.PlacementTarget = textBox;
            _popupSearch.Placement = PlacementMode.Bottom;
            _popupDataGrid.Tag = textBox;
            if (!_popupSearch.IsOpen)
            {
                _popupSearch.IsOpen = true;
            }
            if (_popupDataGrid != null)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    _popupDataGrid.DataContext = _dcsbiosControls;
                    _popupDataGrid.ItemsSource = _dcsbiosControls;
                    _popupDataGrid.Items.Refresh();
                    return;
                }
                var subList = _dcsbiosControls.Where(controlObject => (!string.IsNullOrWhiteSpace(controlObject.identifier) && controlObject.identifier.ToUpper().Contains(textBox.Text.ToUpper()))
                                                                      || (!string.IsNullOrWhiteSpace(controlObject.description) && controlObject.description.ToUpper().Contains(textBox.Text.ToUpper())));
                _popupDataGrid.DataContext = subList;
                _popupDataGrid.ItemsSource = subList;
                _popupDataGrid.Items.Refresh();
            }
        }

        private void TextBoxSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                AdjustShownPopupData((TextBox)sender);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonPressUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.OffsetY -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetY = _dcsbiosDecoder.OffsetY;
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonPressDown_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.OffsetY += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetY = _dcsbiosDecoder.OffsetY;
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public bool IsDirty => _isDirty;

        public void SetIsDirty()
        {
            if (_populatingData)
            {
                return;
            }
            _isDirty = true;
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        private void RepeatButtonPressLeft_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.OffsetX -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetX = _dcsbiosDecoder.OffsetX;
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonPressRight_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.OffsetX += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetX = _dcsbiosDecoder.OffsetX;
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxFormula_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetIsDirty();
            SetFormState();
        }

        private void ButtonFormulaHelp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var helpWindow = new JaceHelpWindow();
                helpWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxDCSBIOSId.Text = "";
                LabelSourceRawValue.Content = "";
                LabelResult.Content = "";
                LabelErrors.Content = "";
                TextBoxSearch.Text = _typeToSearch;
                TextBoxSearch.Foreground = new SolidColorBrush(Colors.Gainsboro);
                _dcsbiosDecoder.RemoveDCSBIOSOutput();
                _dcsbiosControl = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var font = SettingsManager.DefaultFont;

                if (StreamDeckUICommon.SetFontStyle(ref font) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosDecoder.TextFont = font;
                    SettingsManager.DefaultFont = font;
                    SetIsDirty();
                    UpdateFontInfo();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceFontColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var color = Color.Transparent;

                if (StreamDeckUICommon.SetFontColor(ref color) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosDecoder.FontColor = color;
                    SetIsDirty();
                    UpdateFontInfo();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceBackgroundColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var color = Color.Transparent;

                if (StreamDeckUICommon.SetBackgroundColor(ref color) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosDecoder.BackgroundColor = color;
                    SetIsDirty();
                    UpdateFontInfo();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateFontInfo()
        {
            TextBoxFontInfo.Text = "Font : " + _dcsbiosDecoder.TextFont.Name + " " +
                                   _dcsbiosDecoder.TextFont.Size + " " +
                                   (_dcsbiosDecoder.TextFont.Bold ? "Bold" : "Regular");
            TextBoxFontInfo.Text = TextBoxFontInfo.Text + "\n" + "Font Color : " + _dcsbiosDecoder.FontColor.ToString();
            TextBoxFontInfo.Text = TextBoxFontInfo.Text + "\n" + "Background Color : " + _dcsbiosDecoder.BackgroundColor.ToString();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBoxSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Text = "";
            textbox.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void TextBoxSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Text = _typeToSearch;
            textbox.Foreground = new SolidColorBrush(Colors.Gainsboro);
        }

        private void ShowConverters()
        {
            RadioButtonOutputConvert.IsChecked = DCSBIOSConverters.Count > 0;
            DataGridConverters.DataContext = DCSBIOSConverters;
            DataGridConverters.ItemsSource = DCSBIOSConverters;
            DataGridConverters.Items.Refresh();
        }

        private void ButtonAddConverter_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new StreamDeckDCSBIOSConverterWindow(_dcsbiosDecoder.StreamDeckButtonName, _streamDeckInstanceId);
                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    _dcsbiosDecoder.Add(window.DCSBIOSConverter.DeepClone());
                    window.DCSBIOSConverter = null;
                    ShowConverters();
                    SetFormState();
                }

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonEditConverter_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridConverters.SelectedItems.Count != 1)
                {
                    return;
                }
                var converter = (DCSBIOSConverter)DataGridConverters.SelectedItems[0];
                var window = new StreamDeckDCSBIOSConverterWindow(_dcsbiosDecoder.StreamDeckButtonName, _streamDeckInstanceId, converter);
                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    _dcsbiosDecoder.Replace((DCSBIOSConverter)DataGridConverters.SelectedItems[0], window.DCSBIOSConverter);
                    SetIsDirty();
                    ShowConverters();
                    SetFormState();
                }

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteConverter_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.Remove((DCSBIOSConverter)DataGridConverters.SelectedItems[0]);
                RadioButtonOutputRaw.IsChecked = DCSBIOSConverters.Count == 0;
                SetIsDirty();
                ShowConverters();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RadioButtonOutput_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RadioButtonOutputRaw.IsChecked == true)
                {
                    _dcsbiosDecoder.DecoderOutputType = EnumDCSBIOSDecoderOutputType.Raw;
                }
                if (RadioButtonOutputConvert.IsChecked == true)
                {
                    _dcsbiosDecoder.DecoderOutputType = EnumDCSBIOSDecoderOutputType.Converter;
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public List<DCSBIOSConverter> DCSBIOSConverters
        {
            get => _dcsbiosDecoder.DCSBIOSConverters;
        }

        private void DataGridConverters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void CheckBoxUseFormula_OnChange(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFormState();

                _dcsbiosDecoder.UseFormula = CheckBoxUseFormula.IsChecked == true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LoadDefaults()
        {
            _dcsbiosDecoder.OffsetX = SettingsManager.OffsetX;
            _dcsbiosDecoder.OffsetY = SettingsManager.OffsetY;
            _dcsbiosDecoder.TextFont = SettingsManager.DefaultFont;
            _dcsbiosDecoder.FontColor = SettingsManager.DefaultFontColor;
            _dcsbiosDecoder.BackgroundColor = SettingsManager.DefaultBackgroundColor;
        }

        /*
        private void DisplayButtonImage()
        {
            Dispatcher?.BeginInvoke(
                (Action)delegate
                {
                    ButtonImage.Source = CommonStreamDeck.ConvertBitMap(_dcsbiosDecoder.Bitmap);
                });
        }
        */
        private void SetFormulaError(string error)
        {
            Dispatcher?.BeginInvoke(
                (Action)delegate
                {
                    LabelErrors.Content = error;
                });
        }

        private void SetRawDCSBIOSValue(uint value)
        {
            Dispatcher?.BeginInvoke(
                (Action)delegate
                {
                    LabelSourceRawValue.Content = value;
                });
        }

        private void SetFormulaResult(double result)
        {
            Dispatcher?.BeginInvoke(
                (Action)delegate
                {
                    LabelResult.Content = result.ToString(CultureInfo.InvariantCulture);
                });
        }

        private void TextBoxFormula_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.Formula = TextBoxFormula.Text.Replace(Environment.NewLine, "");
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBlockFormulas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(_formulaFile);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void Control_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void Control_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelInsert_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TextBoxDCSBIOSId.Text))
                {
                    CheckBoxUseFormula.IsChecked = true;
                    TextBoxFormula.Text = TextBoxDCSBIOSId.Text + " " + TextBoxFormula.Text;
                    SystemSounds.Asterisk.Play();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void CloseWindow()
        {
            _dcsbiosDecoder.Clean();
            _exitThread = true;
            Close();
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
                CloseWindow();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CancelWindow();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public DCSBIOSDecoder DCSBIOSDecoder
        {
            get => _dcsbiosDecoder;
            set => _dcsbiosDecoder = value;
        }

        private void RadioButtonIntegerSource_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RadioButtonStringSource_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosControls = DCSBIOSControlLocator.GetStringOutputControls();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CheckBoxStringAsNumber_Changed(object sender, RoutedEventArgs e)
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

        private void CancelWindow()
        {
            if (CommonUI.DoDiscardAfterMessage(_isDirty))
            {
                DialogResult = false;
                CloseWindow();
            }
        }


        private void StreamDeckDCSBIOSDecoderWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                CancelWindow();
            }
        }

        private void LabelInsert_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelInsert_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelInsertRaw_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!TextBoxOutputTextRaw.Text.Contains(StreamDeckConstants.DCSBIOSValuePlaceHolder))
                {
                    TextBoxOutputTextRaw.Text = string.IsNullOrEmpty(TextBoxOutputTextRaw.Text) ? StreamDeckConstants.DCSBIOSValuePlaceHolder : StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + TextBoxOutputTextRaw.Text;
                    TextBoxOutputTextRaw.CaretIndex = TextBoxOutputTextRaw.Text.Length;
                    _dcsbiosDecoder.ButtonTextTemplate = TextBoxOutputTextRaw.Text;
                    SetIsDirty();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxOutputTextRaw_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.ButtonTextTemplate = ((TextBox)sender).Text;
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
