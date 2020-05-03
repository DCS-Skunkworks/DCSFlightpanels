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
using System.Windows.Media.Imaging;
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
        private Popup _popupSearch;
        private DataGrid _popupDataGrid;
        private IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private DCSBIOSControl _dcsbiosControl;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private Dictionary<string, double> _variables = new Dictionary<string, double>();
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
            _dcsbiosDecoder.DecoderSourceType = DCSBiosOutputType.INTEGER_TYPE;
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
                TextBoxSearchWord.Focus();
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

            StackPanelRawTextAndStyle.Visibility = RadioButtonProcessToRaw.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            StackPanelConverters.Visibility = RadioButtonProcessToConverter.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
            StackPanelConverters.IsEnabled = RadioButtonProcessToConverter.IsChecked == true;
            ButtonAddConverter.IsEnabled = RadioButtonProcessToConverter.IsChecked == true;

            ButtonEditConverter.IsEnabled = DataGridConverters.SelectedItems.Count == 1;
            ButtonDeleteConverter.IsEnabled = DataGridConverters.SelectedItems.Count == 1;

            if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.STRING_TYPE && CheckBoxTreatStringAsNumber.IsChecked == false)
            {
                RadioButtonProcessToRaw.IsChecked = true;
                RadioButtonProcessToConverter.IsEnabled = false;
            }
            else
            {
                RadioButtonProcessToConverter.IsEnabled = true;
            }

            GroupBoxFormula.IsEnabled = CheckBoxUseFormula.IsChecked == true;

            CheckBoxTreatStringAsNumber.IsEnabled = RadioButtonStringSource.IsChecked == true;

            CheckBoxUseFormula.IsEnabled = (RadioButtonIntegerSource.IsChecked == true) ||
                                           (RadioButtonStringSource.IsChecked == true && CheckBoxTreatStringAsNumber.IsChecked == true);

            if (RadioButtonIntegerSource.IsChecked == true)
            {
                CheckBoxTreatStringAsNumber.IsChecked = false;
            }

            if (RadioButtonStringSource.IsChecked == true && CheckBoxTreatStringAsNumber.IsChecked == false)
            {
                CheckBoxUseFormula.IsChecked = false;
            }

            ButtonOK.IsEnabled = _dcsbiosDecoder.DecoderConfigurationOK() && !string.IsNullOrEmpty(TextBoxDCSBIOSId.Text);
        }

        private void SetInfoTextBoxes()
        {
            TextBoxFontInfo.TargetFont = _dcsbiosDecoder.RawTextFont;
            TextBoxFontInfo.TargetFontColor = _dcsbiosDecoder.RawFontColor;
            TextBoxFontInfo.TargetBackgroundColor = _dcsbiosDecoder.RawBackgroundColor;

            TextBoxOffsetInfo.OffSetX = _dcsbiosDecoder.OffsetX;
            TextBoxOffsetInfo.OffSetY = _dcsbiosDecoder.OffsetY;
        }

        private void ShowDecoder()
        {
            _populatingData = true;

            SetInfoTextBoxes();

            DCSBIOSDecoder.ShowOnly(_dcsbiosDecoder, _streamDeckInstanceId);
            if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.INTEGER_TYPE)
            {
                RadioButtonIntegerSource.IsChecked = true;
            }
            else if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.STRING_TYPE)
            {
                RadioButtonStringSource.IsChecked = true;
            }

            switch (_dcsbiosDecoder.DecoderOutputType)
            {
                case EnumDCSBIOSDecoderOutputType.Raw:
                    {
                        TextBoxOutputTextRaw.Text = _dcsbiosDecoder.ButtonTextTemplate;
                        RadioButtonProcessToRaw.IsChecked = true;
                        break;
                    }
                case EnumDCSBIOSDecoderOutputType.Converter:
                    {
                        RadioButtonProcessToConverter.IsChecked = true;
                        break;
                    }
            }
            CheckBoxTreatStringAsNumber.IsChecked = _dcsbiosDecoder.TreatStringAsNumber;

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
            TextBoxFontInfo.UpdateFontInfo();
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
                            if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.INTEGER_TYPE)
                            {
                                SetRawDCSBIOSValue(_dcsbiosDecoder.UintDcsBiosValue);
                            }
                            else
                            {
                                SetRawDCSBIOSValue(_dcsbiosDecoder.StringDcsBiosValue);
                            }
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
                TextBoxOffsetInfo.OffSetY = _dcsbiosDecoder.OffsetY;
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
                TextBoxOffsetInfo.OffSetY = _dcsbiosDecoder.OffsetY;
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
                TextBoxOffsetInfo.OffSetX = _dcsbiosDecoder.OffsetX;
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
                TextBoxOffsetInfo.OffSetX = _dcsbiosDecoder.OffsetX;
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
                TextBoxSearchWord.Text = "";
                TextBoxSearchWord.Foreground = new SolidColorBrush(Colors.Gainsboro);
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
                    TextBoxFontInfo.TargetFont = font;
                    SetIsDirty();
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
                    TextBoxFontInfo.TargetFontColor = color;
                    SetIsDirty();
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
                    TextBoxFontInfo.TargetBackgroundColor = color;
                    SetIsDirty();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
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
            textbox.Foreground = new SolidColorBrush(Colors.Gainsboro);
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TextBoxSearchWord.Text == "")
                {
                    // Create an ImageBrush.
                    var textImageBrush = new ImageBrush();
                    textImageBrush.ImageSource =
                        new BitmapImage(
                            new Uri("pack://application:,,,/dcsfp;component/Images/cue_banner_search_dcsbios.png", UriKind.RelativeOrAbsolute)
                        );
                    textImageBrush.AlignmentX = AlignmentX.Left;
                    textImageBrush.Stretch = Stretch.Uniform;
                    // Use the brush to paint the button's background.
                    TextBoxSearchWord.Background = textImageBrush;
                }
                else
                {
                    TextBoxSearchWord.Background = null;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ShowConverters()
        {
            RadioButtonProcessToConverter.IsChecked = DCSBIOSConverters.Count > 0;
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
                RadioButtonProcessToRaw.IsChecked = DCSBIOSConverters.Count == 0;
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
                if (RadioButtonProcessToRaw.IsChecked == true)
                {
                    _dcsbiosDecoder.DecoderOutputType = EnumDCSBIOSDecoderOutputType.Raw;
                }
                if (RadioButtonProcessToConverter.IsChecked == true)
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

        private void SetRawDCSBIOSValue(string value)
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

        private void ButtonOK_OnClick(object sender, RoutedEventArgs e)
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
                if (_dcsbiosDecoder != null)
                {
                    _dcsbiosDecoder.DecoderSourceType = DCSBiosOutputType.INTEGER_TYPE;
                }
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
                if (_dcsbiosDecoder != null)
                {
                    _dcsbiosDecoder.DecoderSourceType = DCSBiosOutputType.STRING_TYPE;
                }
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
                _dcsbiosDecoder.TreatStringAsNumber = CheckBoxTreatStringAsNumber.IsChecked == true;
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
        
        private void TextBoxOutputTextRaw_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SetInfoTextBoxes();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
