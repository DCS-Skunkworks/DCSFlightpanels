using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Properties;
using NonVisuals;
using NonVisuals.StreamDeck;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for JaceSandbox.xaml
    /// </summary>
    public partial class StreamDeckDCSBIOSDecoderWindow : Window, IDcsBiosDataListener
    {
        private EnumStreamDeckButtonNames _streamDeckButton;
        private StreamDeckPanel _streamDeck;
        private DCSBIOSOutput _dcsbiosOutput1 = null;
        private volatile uint _value1 = 0;
        private bool _dataChanged;
        private bool _formLoaded;
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly string _typeToSearch = "Type to search control";
        private readonly DCSBIOS _dcsbios;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private DCSBIOSControl _dcsbiosControl1;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private Dictionary<string, double> _variables = new Dictionary<string, double>();
        private bool _formulaHadErrors = false;
        private bool _changesMade = false;
        private string _decoderResult;
        private bool _isDirty = false;






        public StreamDeckDCSBIOSDecoderWindow(StreamDeckPanel streamDeck, EnumStreamDeckButtonNames streamDeckButton, DCSBIOS dcsbios)
        {
            InitializeComponent();
            _dcsbios = dcsbios;
            _dcsbios.AttachDataReceivedListener(this);
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
            _streamDeckButton = streamDeckButton;
            _streamDeck = streamDeck;
        }


        private void StreamDeckDCSBIOSDecoderWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }
                //LoadFontSettings();
                SetBills();
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _dataGridValues = ((DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "DataGridValues"));
                _formLoaded = true;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1001, ex);
            }
        }


        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    var dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    var dcsBiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControl.identifier);
                    var textBox = (TextBox)_dataGridValues.Tag;
                    if (Equals(textBox, TextBoxSearch1))
                    {
                        _dcsbiosOutput1 = dcsBiosOutput;
                        _dcsbiosControl1 = dcsbiosControl;
                        TextBoxId1.Text = _dcsbiosControl1.identifier;
                        TextBoxSearch1.Text = _typeToSearch;
                        if (string.IsNullOrEmpty(TextBoxFormula.Text))
                        {
                            TextBoxFormula.Text = _dcsbiosOutput1.ControlId + " + 1";
                        }
                    }
                    SetFormState();
                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1006, ex);
            }
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (_dcsbiosOutput1?.Address == e.Address)
            {
                if (!Equals(_value1, e.Data))
                {
                    _value1 = e.Data;
                    _dataChanged = true;
                    Dispatcher?.BeginInvoke(
                        (Action)delegate
                        {
                            LabelSourceRawValue1.Content = "Value : " + _value1;
                        });
                }
            }
        }

        private void AdjustShownPopupData(TextBox textBox)
        {
            _popupSearch.PlacementTarget = textBox;
            _popupSearch.Placement = PlacementMode.Bottom;
            _dataGridValues.Tag = textBox;
            if (!_popupSearch.IsOpen)
            {
                _popupSearch.IsOpen = true;
            }
            if (_dataGridValues != null)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    _dataGridValues.DataContext = _dcsbiosControls;
                    _dataGridValues.ItemsSource = _dcsbiosControls;
                    _dataGridValues.Items.Refresh();
                    return;
                }
                var subList = _dcsbiosControls.Where(controlObject => (!string.IsNullOrWhiteSpace(controlObject.identifier) && controlObject.identifier.ToUpper().Contains(textBox.Text.ToUpper()))
                                                                      || (!string.IsNullOrWhiteSpace(controlObject.description) && controlObject.description.ToUpper().Contains(textBox.Text.ToUpper())));
                _dataGridValues.DataContext = subList;
                _dataGridValues.ItemsSource = subList;
                _dataGridValues.Items.Refresh();
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
                Common.ShowErrorMessageBox(1005, ex);
            }
        }

        private void ButtonTestFormula_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var formula = TextBoxFormula.Text;
                var variables = new Dictionary<string, double>();

                if (_dcsbiosOutput1 != null)
                {
                    variables.Add(_dcsbiosOutput1.ControlId, 0);
                    variables[_dcsbiosOutput1.ControlId] = GetVariableValues(_dcsbiosOutput1.ControlId);
                }
                _decoderResult = _jaceExtended.CalculationEngine.Calculate(formula, variables).ToString(CultureInfo.InvariantCulture);

                LabelErrors.Content = "";
                LabelResult.Content = "Result : " + _decoderResult;
                SetFormState();
            }
            catch (Exception ex)
            {
                LabelErrors.Content = ex.Message;
                _formulaHadErrors = true;
            }
        }

        private double GetVariableValues(string controlId)
        {
            if (Equals(controlId, _dcsbiosOutput1.ControlId))
            {
                return _value1;
            }
            throw new Exception("Failed to pair DCSBIOSOutput " + controlId);
        }

        private void RepeatButtonActionPressUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxButtonTextFace.Bill.OffsetY -= Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressDown_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxButtonTextFace.Bill.OffsetY += Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressLeft_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxButtonTextFace.Bill.OffsetX -= Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void TestImage()
        {
            try
            {
                if (string.IsNullOrEmpty(_decoderResult))
                {
                    return;
                }
                var bitmap = BitMapCreator.CreateStreamDeckBitmap(TextBoxButtonTextFace.Text, TextBoxButtonTextFace.Bill.TextFont, TextBoxButtonTextFace.Bill.FontColor, TextBoxButtonTextFace.Bill.BackgroundColor, TextBoxButtonTextFace.Bill.OffsetX, TextBoxButtonTextFace.Bill.OffsetY);
                _streamDeck.SetImage(_streamDeckButton, bitmap);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressRight_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxButtonTextFace.Bill.OffsetX += Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void TextBoxFormula_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _changesMade = true;
            SetFormState();
        }

        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }

            ButtonTestFormula.IsEnabled = !string.IsNullOrEmpty(TextBoxId1.Text) && !string.IsNullOrEmpty(TextBoxFormula.Text);
            ButtonSave.IsEnabled = !string.IsNullOrEmpty(TextBoxId1.Text) && !string.IsNullOrEmpty(TextBoxFormula.Text) && !_formulaHadErrors;
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

        private void StreamDeckDCSBIOSDecoderWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_changesMade)
            {
                if (MessageBox.Show("Discard changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void ButtonClear1_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxId1.Text = "";
                LabelSourceRawValue1.Content = "Value : ";
                LabelResult.Content = "Result :";
                TextBoxSearch1.Text = _typeToSearch;
                TextBoxSearch1.Foreground = new SolidColorBrush(Colors.Gainsboro);
                _dcsbiosOutput1 = null;
                _dcsbiosControl1 = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void SetBills()
        {
            TextBoxButtonTextFace.Bill = new BillStreamDeckFace();
            TextBoxButtonTextFace.Bill.ParentTextBox = TextBoxButtonTextFace;
        }

        private void ButtonTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFontStyle(TextBoxButtonTextFace);
                TestImage();
                SetFormState();
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
                SetFontColor(TextBoxButtonTextFace);
                TestImage();
                SetFormState();
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
                SetBackgroundColor(TextBoxButtonTextFace);
                TestImage();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void SetFontStyle(StreamDeckFaceTextBox textBox)
        {
            var fontDialog = new FontDialog();

            fontDialog.FixedPitchOnly = true;
            fontDialog.FontMustExist = true;
            fontDialog.MinSize = 6;

            if (Settings.Default.ButtonTextFaceFont != null)
            {
                fontDialog.Font = Settings.Default.ButtonTextFaceFont;
            }
            
            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox.Bill.TextFont = fontDialog.Font;

                Settings.Default.ButtonTextFaceFont = fontDialog.Font;
                Settings.Default.Save();

                SetIsDirty();
            }
        }

        private void SetFontColor(StreamDeckFaceTextBox textBox)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextFaceFontColor;
            colorDialog.CustomColors = Constants.GetOLEColors();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox.Bill.FontColor = colorDialog.Color;

                Settings.Default.ButtonTextFaceFontColor = colorDialog.Color;
                Settings.Default.Save();

                SetIsDirty();
            }
        }

        private void SetBackgroundColor(StreamDeckFaceTextBox textBox)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextFaceBackgroundColor;
            colorDialog.CustomColors = Constants.GetOLEColors();
            
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox.Bill.BackgroundColor = colorDialog.Color;

                Settings.Default.ButtonTextFaceBackgroundColor = colorDialog.Color;
                Settings.Default.Save();

                SetIsDirty();
            }
        }

        private void SetIsDirty()
        {
            _isDirty = true;
        }


        private void LoadFontSettings()
        {
            if (Settings.Default.ButtonTextFaceFont != null)
            {
                TextBoxButtonTextFace.Bill.TextFont = Settings.Default.ButtonTextFaceFont;
            }
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void StreamDeckDCSBIOSDecoderWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
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
    }
}
