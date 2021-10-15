namespace DCSFlightpanels.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using ClassLibraryCommon;

    using DCS_BIOS;

    /// <summary>
    /// Interaction logic for DCSBiosOutputFormulaWindow.xaml
    /// </summary>
    public partial class DCSBiosOutputFormulaWindow : Window
    {
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private readonly string _description;
        private readonly bool _userEditsDescription;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private DCSBIOSOutput _dcsBiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula;
        private bool _formLoaded;
        private DCSBIOSControl _dcsbiosControl;
        private DCSFPProfile _dcsfpProfile;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;

        public DCSBiosOutputFormulaWindow(DCSFPProfile dcsfpProfile, string description, bool userEditsDescription = false)
        {
            InitializeComponent();
            _dcsfpProfile = dcsfpProfile;
            _description = description;
            _userEditsDescription = userEditsDescription;
            _dcsBiosOutput = new DCSBIOSOutput();
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
        }

        public DCSBiosOutputFormulaWindow(DCSFPProfile dcsfpProfile, string description, DCSBIOSOutput dcsBiosOutput, bool userEditsDescription = false)
        {
            InitializeComponent();
            _dcsfpProfile = dcsfpProfile;
            _description = description;
            _userEditsDescription = userEditsDescription;
            _dcsBiosOutput = dcsBiosOutput;
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControl = DCSBIOSControlLocator.GetControl(_dcsBiosOutput.ControlId);
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
        }

        public DCSBiosOutputFormulaWindow(DCSFPProfile dcsfpProfile, string description, DCSBIOSOutputFormula dcsBiosOutputFormula, bool userEditsDescription = false)
        {
            InitializeComponent();
            _dcsfpProfile = dcsfpProfile;
            _description = description;
            _userEditsDescription = userEditsDescription;
            _dcsbiosOutputFormula = dcsBiosOutputFormula;
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _dataGridValues = ((DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "DataGridValues"));
                LabelDescription.Content = _description;
                ShowValues2();
                _formLoaded = true;
                SetFormState();
                TextBoxSearchWord.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ShowValues2()
        {
            if (_dcsbiosControl != null)
            {
                TextBoxControlId.Text = _dcsbiosControl.identifier;
                TextBoxControlDescription.Text = _dcsbiosControl.description;
                TextBoxMaxValue.Text = _dcsbiosControl.outputs[0].max_value.ToString();
                TextBoxOutputType.Text = _dcsbiosControl.outputs[0].type;
            }
            if (_dcsbiosOutputFormula != null)
            {
                CheckBoxUseFormula.IsChecked = true;
                TextBoxFormula.Text = _dcsbiosOutputFormula.Formula;
            }
        }

        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }
            
            LabelDescription.Visibility = !_userEditsDescription ? Visibility.Visible : Visibility.Collapsed;
            LabelUserDescription.Visibility = _userEditsDescription ? Visibility.Visible : Visibility.Collapsed;
            TextBoxUserDescription.Visibility = _userEditsDescription ? Visibility.Visible : Visibility.Collapsed;

            GroupBoxFormula.Visibility = Visibility.Visible;

            LabelFormula.IsEnabled = (CheckBoxUseFormula.IsChecked.HasValue && CheckBoxUseFormula.IsChecked.Value);
            TextBoxFormula.IsEnabled = LabelFormula.IsEnabled;
            LabelResult.IsEnabled = LabelFormula.IsEnabled;
            ButtonTestFormula.IsEnabled = LabelFormula.IsEnabled;
            ButtonOk.IsEnabled = (_dcsbiosControl == null && _dcsBiosOutput == null) || (_dcsbiosControl != null || (!string.IsNullOrWhiteSpace(TextBoxFormula.Text) && CheckBoxUseFormula.IsChecked == true));
            if (_userEditsDescription && string.IsNullOrEmpty(TextBoxUserDescription.Text))
            {
                ButtonOk.IsEnabled = false;
            }

            if (_dcsbiosControl == null && _dcsBiosOutput == null)
            {
                TextBoxControlId.Text = string.Empty;
                TextBoxMaxValue.Text = string.Empty;
                TextBoxOutputType.Text = string.Empty;
                TextBoxControlDescription.Text = string.Empty;
            }
        }

        private void CopyValues()
        {
            if (CheckBoxUseFormula.IsChecked.HasValue && CheckBoxUseFormula.IsChecked.Value)
            {
                //Use formula
                try
                {
                    _dcsbiosOutputFormula = new DCSBIOSOutputFormula(TextBoxFormula.Text);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while creating formula object : {ex.Message}");
                }
            }
            else
            {
                //Use single DCSBIOSOutput
                //This is were DCSBiosOutput (subset of DCSBIOSControl) get populated from DCSBIOSControl
                try
                {
                    if (_dcsbiosControl == null && !string.IsNullOrWhiteSpace(TextBoxControlId.Text))
                    {
                        _dcsbiosControl = DCSBIOSControlLocator.GetControl(TextBoxControlId.Text);
                        _dcsBiosOutput.Consume(_dcsbiosControl);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while creating DCSBIOSOutput object : {ex.Message}");
                }
            }
        }

        private void ClearAll()
        {
            _dcsbiosControl = null;
            _dcsBiosOutput = null;
            _dcsbiosOutputFormula = null;
        }

        private void CheckFormula()
        {
            if (CheckBoxUseFormula.IsChecked.HasValue && CheckBoxUseFormula.IsChecked.Value && string.IsNullOrWhiteSpace(TextBoxFormula.Text))
            {
                throw new Exception("Formula field can not be empty.");
            }
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckFormula();
                CopyValues();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAll();
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public DCSBIOSOutput DCSBiosOutput
        {
            get { return _dcsBiosOutput; }
            set { _dcsBiosOutput = value; }
        }

        private void AdjustShownPopupData()
        {
            _popupSearch.PlacementTarget = TextBoxSearchWord;
            _popupSearch.Placement = PlacementMode.Bottom;
            if (!_popupSearch.IsOpen)
            {
                _popupSearch.IsOpen = true;
            }
            if (_dataGridValues != null)
            {
                if (string.IsNullOrEmpty(TextBoxSearchWord.Text))
                {
                    _dataGridValues.DataContext = _dcsbiosControls;
                    _dataGridValues.ItemsSource = _dcsbiosControls;
                    _dataGridValues.Items.Refresh();
                    return;
                }
                var subList = _dcsbiosControls.Where(controlObject => (!string.IsNullOrWhiteSpace(controlObject.identifier) && controlObject.identifier.ToUpper().Contains(TextBoxSearchWord.Text.ToUpper()))
                    || (!string.IsNullOrWhiteSpace(controlObject.description) && controlObject.description.ToUpper().Contains(TextBoxSearchWord.Text.ToUpper())));
                _dataGridValues.DataContext = subList;
                _dataGridValues.ItemsSource = subList;
                _dataGridValues.Items.Refresh();
            }
        }

        private void TextBoxSearchWord_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                AdjustShownPopupData();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }
        
        private void TextBoxSearchWord_OnTextChanged(object sender, TextChangedEventArgs e)
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

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosOutput = new DCSBIOSOutput();
                    _dcsBiosOutput.Consume(_dcsbiosControl);
                    ShowValues2();
                    SetFormState();
                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosOutput = new DCSBIOSOutput();
                    _dcsBiosOutput.Consume(_dcsbiosControl);
                    ShowValues2();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosOutput = new DCSBIOSOutput();
                    _dcsBiosOutput.Consume(_dcsbiosControl);
                    ShowValues2();

                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonTestFormula_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBlockFormulaErrors.Text = string.Empty;
                LabelResult.Content = "Result : " + _jaceExtended.Evaluate(TextBoxFormula.Text);
                SetFormState();
            }
            catch (Exception ex)
            {
                TextBlockFormulaErrors.Text = ex.Message;
            }
        }

        private void CheckBoxUseFormula_OnChecked(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        private void CheckBoxUseFormula_OnUnchecked(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        private void ButtonFormulaHelp_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new TextBlockDialogWindow("Formula help");
            dialog.AddBold("Using a single DCS-BIOS Control as is:");
            dialog.AddLineBreak();
            dialog.AddLineBreak();
            dialog.Add("Type search words for the DCS-BIOS Control and double click it once it is visible and then click Ok. Thats all that is required.");
            dialog.AddLineBreak();
            dialog.AddLineBreak();
            dialog.AddBold("Using a formula:");
            dialog.AddLineBreak();
            dialog.AddLineBreak();
            dialog.Add("DCS-BIOS outputs data 'as is' from DCS. Most of the time these values are not for human consumption. ");
            dialog.Add("You can use a formula to modify these values to something understandable or for example change units feet -> meter. ");
            dialog.AddLineBreak();
            dialog.AddLineBreak();
            dialog.Add("Build the formula first with numbers and make sure it works by using the Test button. Once it works use the search box to search for the DCS-BIOS Controls you want in your formula. ");
            dialog.Add("Copy the control's name as is into the formula replacing the test values you used. As always use DCS-BIOS Control Reference Page (Chrome) to get a better understanding of the values for the DCS-BIOS Control. ");
            dialog.AddLineBreak();
            dialog.AddLineBreak();
            dialog.AddBold("Example");
            dialog.AddLineBreak();
            dialog.Add("USE COMMA, NOT POINT FOR DECIMALS");
            //dialog.Add("This formula sums the altitude for the A-10C and converts it to meter format. (USE COMMA, NOT POINT FOR DECIMALS) ");
            dialog.AddLineBreak();
            //dialog.AddItalic("((ALT_10000FT_CNT/65535)*100*10000 + (ALT_1000FT_CNT/65535)*100*1000 + (ALT_100FT_CNT/65535)*100*100) / 3,28", true);
            dialog.ShowDialog();
        }

        public DCSBIOSOutputFormula DCSBIOSOutputFormula
        {
            get { return _dcsbiosOutputFormula; }
        }

        public bool UseFormula()
        {
            if (CheckBoxUseFormula.IsChecked.HasValue && CheckBoxUseFormula.IsChecked.Value)
            {
                return true;
            }
            return false;
        }

        public bool UseSingleDCSBiosControl()
        {
            if (_dcsbiosControl != null && _dcsBiosOutput != null)
            {
                return true;
            }
            return false;
        }

        private void ButtonClearAll_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBoxUseFormula.IsChecked = false;
                _dcsbiosControl = null;
                _dcsBiosOutput = null;
                TextBoxFormula.Text = string.Empty;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void TextBoxFormula_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void DCSBiosOutputFormulaWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        public string UserDescription
        {
            get { return TextBoxUserDescription.Text; }
        }

    }
}
