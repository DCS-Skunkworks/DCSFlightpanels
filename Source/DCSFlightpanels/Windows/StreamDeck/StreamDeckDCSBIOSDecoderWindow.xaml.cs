using System.ComponentModel;
using System.Windows.Threading;
using DCS_BIOS.EventArgs;
using DCS_BIOS.Interfaces;
using DCS_BIOS.Json;
using NLog;
using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels.Windows.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Media;
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

    /// <summary>
    /// This StreamDeck implementation is a big clusterf*ck.
    /// </summary>
    public partial class StreamDeckDCSBIOSDecoderWindow : Window, IIsDirty, IDisposable, IDcsBiosDataListener
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string _formulaFile = AppDomain.CurrentDomain.BaseDirectory + "\\formulas.txt";
        private readonly StreamDeckPanel _streamDeckPanel;
        private bool _formLoaded;
        private Popup _popupSearch;
        private DataGrid _popupDataGrid;
        private IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private DCSBIOSControl _dcsbiosControl;
        private bool _isDirty = false;
        private bool _populatingData = false;
        private DCSBIOSDecoder _dcsbiosDecoder = null;
        private bool _closing = false;
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer;

        public StreamDeckDCSBIOSDecoderWindow(DCSBIOSDecoder dcsbiosDecoder, StreamDeckPanel streamDeckPanel)
        {
            InitializeComponent();
            _dcsbiosDecoder = dcsbiosDecoder;
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
            _streamDeckPanel = streamDeckPanel;

            BIOSEventHandler.AttachDataListener(this);
        }

        public StreamDeckDCSBIOSDecoderWindow(StreamDeckPanel streamDeckPanel)
        {
            InitializeComponent();
            _streamDeckPanel = streamDeckPanel;
            _dcsbiosDecoder = new DCSBIOSDecoder
            {
                DecoderSourceType = DCSBiosOutputType.IntegerType,
                StreamDeckPanelInstance = streamDeckPanel,
                StreamDeckButtonName = streamDeckPanel.SelectedButtonName
            };
            LoadDefaults();
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
            BIOSEventHandler.AttachDataListener(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dcsbiosDecoder?.Dispose();
                BIOSEventHandler.DetachDataListener(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                _popupDataGrid = (DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "PopupDataGrid");
                _formLoaded = true;
                SetFormState();
                TextBoxSearchWord.Focus();

                _dispatcherTimer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Send);
                _dispatcherTimer.Tick += new EventHandler(DispatcherTimerTick);
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
                _dispatcherTimer.Start();
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

            if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.StringType && CheckBoxTreatStringAsNumber.IsChecked == false)
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

            ButtonOK.IsEnabled = _dcsbiosDecoder.DecoderConfigurationOK() && (!string.IsNullOrEmpty(TextBoxDCSBIOSId.Text) || !string.IsNullOrEmpty(TextBoxFormula.Text));

            ComboBoxDecimals.IsEnabled = CheckBoxLimitDecimals.IsChecked == true;
        }

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            try
            {
                LabelErrors.Content = _dcsbiosDecoder.HasErrors ? _dcsbiosDecoder.LastFormulaError : string.Empty;
                LabelFormulaResult.Content = _dcsbiosDecoder.HasErrors ? "-" : _dcsbiosDecoder.GetResultString();

                if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.IntegerType)
                {
                    if (_dcsbiosDecoder.UseFormula)
                    {
                        LabelSourceRawDCSBIOSValue.Content = "-";
                    }
                    else
                    {
                        LabelSourceRawDCSBIOSValue.Content = _dcsbiosDecoder.UintDcsBiosValue;
                    }
                }
                else
                {
                    LabelSourceRawDCSBIOSValue.Content = _dcsbiosDecoder.StringDcsBiosValue;
                }
            }
            catch (Exception ex)
            {
                LabelErrors.Content = ex.Message;
            }

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                if (_dcsbiosDecoder.FormulaInstance == null || _closing)
                {
                    return;
                }

                _dcsbiosDecoder.FormulaInstance.CheckForMatchAndNewValue(e.Address, e.Data, 20);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DcsBiosDataReceived()");
            }
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

            DCSBIOSDecoder.ShowOnly(_dcsbiosDecoder, _streamDeckPanel);
            if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.IntegerType)
            {
                RadioButtonIntegerSource.IsChecked = true;
            }
            else if (_dcsbiosDecoder.DecoderSourceType == DCSBiosOutputType.StringType)
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
            if (_dcsbiosDecoder.UseFormula && _dcsbiosDecoder.FormulaInstance != null)
            {
                CheckBoxUseFormula.IsChecked = true;

                TextBoxFormula.Text = string.IsNullOrEmpty(_dcsbiosDecoder.FormulaInstance.Formula) ? string.Empty : _dcsbiosDecoder.FormulaInstance.Formula;

                if (_dcsbiosDecoder.LimitDecimalPlaces && _dcsbiosDecoder.NumberFormatInfoFormula != null)
                {
                    CheckBoxLimitDecimals.Checked -= CheckBoxLimitDecimals_CheckedChanged;
                    CheckBoxLimitDecimals.Unchecked -= CheckBoxLimitDecimals_CheckedChanged;
                    CheckBoxLimitDecimals.IsChecked = _dcsbiosDecoder.LimitDecimalPlaces;
                    CheckBoxLimitDecimals.Checked += CheckBoxLimitDecimals_CheckedChanged;
                    CheckBoxLimitDecimals.Unchecked += CheckBoxLimitDecimals_CheckedChanged;

                    ComboBoxDecimals.SelectionChanged -= ComboBoxDecimals_OnSelectionChanged;
                    ComboBoxDecimals.SelectedIndex = _dcsbiosDecoder.NumberFormatInfoFormula.NumberDecimalDigits;
                    ComboBoxDecimals.SelectionChanged += ComboBoxDecimals_OnSelectionChanged;
                }
            }

            if (_dcsbiosDecoder.DCSBIOSOutput != null)
            {
                TextBoxDCSBIOSId.Text = _dcsbiosDecoder.DCSBIOSOutput.ControlId;
            }

            ShowConverters();
            TextBoxFontInfo.UpdateFontInfo();
            _populatingData = false;
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_popupDataGrid.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_popupDataGrid.SelectedItem;
                    _dcsbiosDecoder.DCSBIOSOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(_dcsbiosControl.Identifier);

                    TextBoxDCSBIOSId.Text = _dcsbiosControl.Identifier;
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
                var subList = _dcsbiosControls.Where(controlObject => (!string.IsNullOrWhiteSpace(controlObject.Identifier) && controlObject.Identifier.ToUpper().Contains(textBox.Text.ToUpper()))
                                                                      || (!string.IsNullOrWhiteSpace(controlObject.Description) && controlObject.Description.ToUpper().Contains(textBox.Text.ToUpper())));
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
                TextBoxDCSBIOSId.Text = string.Empty;
                LabelSourceRawDCSBIOSValue.Content = string.Empty;
                LabelFormulaResult.Content = string.Empty;
                LabelErrors.Content = string.Empty;
                TextBoxSearchWord.Text = string.Empty;
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
                var color = System.Drawing.Color.Transparent;

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
                var color = System.Drawing.Color.Transparent;

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
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                
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
            textbox.Text = string.Empty;
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
                if (TextBoxSearchWord.Text == string.Empty)
                {
                    // Create an ImageBrush.
                    var textImageBrush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(
                            new Uri("pack://application:,,,/dcsfp;component/Images/cue_banner_search_dcsbios.png", UriKind.RelativeOrAbsolute)
                        ),
                        AlignmentX = AlignmentX.Left,
                        Stretch = Stretch.Uniform
                    };
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
                var window = new StreamDeckDCSBIOSConverterWindow(_dcsbiosDecoder.StreamDeckButtonName, _streamDeckPanel);
                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    DCSBIOSConverter dcsbiosConverter = window.DCSBIOSConverter.CloneJson();
                    dcsbiosConverter.StreamDeckPanelInstance = _streamDeckPanel;
                    _dcsbiosDecoder.Add(dcsbiosConverter);
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
                var window = new StreamDeckDCSBIOSConverterWindow(_dcsbiosDecoder.StreamDeckButtonName, converter, _streamDeckPanel);
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


        private void TextBoxFormula_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                _dcsbiosDecoder.SetFormula(TextBoxFormula.Text.Replace(Environment.NewLine, string.Empty));
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
                Process.Start(new ProcessStartInfo
                {
                    FileName = _formulaFile,
                    UseShellExecute = true
                });
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
                    _dcsbiosDecoder.DecoderSourceType = DCSBiosOutputType.IntegerType;
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
                    _dcsbiosDecoder.DecoderSourceType = DCSBiosOutputType.StringType;
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

        private void StreamDeckDCSBIOSDecoderWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _closing = true;
        }

        private void CheckBoxLimitDecimals_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dcsbiosDecoder == null)
                {
                    return;
                }

                _dcsbiosDecoder.SetNumberOfDecimals(CheckBoxLimitDecimals.IsChecked == true, Convert.ToInt32(ComboBoxDecimals.SelectedValue.ToString()));
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ComboBoxDecimals_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_dcsbiosDecoder == null)
                {
                    return;
                }

                _dcsbiosDecoder.SetNumberOfDecimals(true, Convert.ToInt32(ComboBoxDecimals.SelectedValue.ToString()));

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}