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

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for DCSBiosOutputWindow.xaml
    /// </summary>
    public partial class DCSBiosOutputWindow : Window
    {
        private DCSBIOSOutput _dcsBiosOutput;
        private readonly string _description;
        private bool _formLoaded;
        private DCSBIOSControl _dcsbiosControl;
        private DCSFPProfile _dcsfpProfile;
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private bool _userEditsDescription = false;

        public DCSBiosOutputWindow(DCSFPProfile dcsfpProfile, string description, bool userEditsDescription = false)
        {
            InitializeComponent();
            _dcsfpProfile = dcsfpProfile;
            _description = description;
            _userEditsDescription = userEditsDescription;
            _dcsBiosOutput = new DCSBIOSOutput();
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
        }

        public DCSBiosOutputWindow(DCSFPProfile dcsfpProfile, string description, DCSBIOSOutput dcsBiosOutput, bool userEditsDescription = false)
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
                Common.ShowErrorMessageBox(ex);
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

            ButtonOk.IsEnabled = (_dcsbiosControl == null && _dcsBiosOutput == null) || _dcsbiosControl != null;
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

        private void ClearAll()
        {
            _dcsbiosControl = null;
            _dcsBiosOutput = null;
        }
        
        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyValues();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxSearchWord_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TextBoxSearchWord.Text == string.Empty)
                {
                    // Create an ImageBrush.
                    var textImageBrush = new ImageBrush();
                    textImageBrush.ImageSource =
                        new BitmapImage(new Uri("pack://application:,,,/dcsfp;component/Images/cue_banner_search_dcsbios.png", UriKind.RelativeOrAbsolute));
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
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
                _dcsbiosControl = null;
                _dcsBiosOutput = null;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DCSBiosOutputWindow_OnKeyDown(object sender, KeyEventArgs e)
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
