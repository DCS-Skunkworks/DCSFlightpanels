using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.TagDataClasses;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonAction.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonAction : UserControlBase
    {
        private List<TextBox> _textBoxes = new List<TextBox>();

        private IStreamDeckUIParent _streamDeckUIParent;
        private IGlobalHandler _globalHandler;
        private bool _isLoaded = false;
        private bool _isDirty = false;


        public UserControlStreamDeckButtonAction()
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckButtonAction_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }

            FillTextBoxList();
            SetTextBoxTagObjects();
            _isLoaded = true;
        }

        private void FillTextBoxList()
        {
            _textBoxes.Add(TextBoxKeyPressButtonOn);
            _textBoxes.Add(TextBoxKeyPressButtonOff);
            _textBoxes.Add(TextBoxDCSBIOSActionButtonOn);
            _textBoxes.Add(TextBoxDCSBIOSActionButtonOff);
            _textBoxes.Add(TextBoxOSCommandButtonOn);
            _textBoxes.Add(TextBoxOSCommandButtonOff);
            _textBoxes.Add(TextBoxLayerNavButtonOn);
            _textBoxes.Add(TextBoxLayerNavButtonOff);
        }

        public void Clear()
        {
            foreach (var textBox in _textBoxes)
            {
                textBox.Clear();
            }
            ComboBoxLayerNavigationButtonOn.ItemsSource = null;
            ComboBoxLayerNavigationButtonOff.ItemsSource = null;
            _isDirty = false;
            SetFormState();
        }

        public void SetFormState()
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                StackPanelButtonKeyPressSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.KeyPress ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.DCSBIOS ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonOSCommandSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.OSCommand ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonLayerNavigationSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                ButtonDeleteKeySequenceButtonOn.IsEnabled = ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsKeySequence() ||
                                                            ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsOSKeyPress();
                ButtonDeleteKeySequenceButtonOff.IsEnabled = ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsKeySequence() ||
                                                            ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsOSKeyPress();
                ButtonDeleteDCSBIOSActionButtonOn.IsEnabled = ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOn.Tag).ContainsDCSBIOS();
                ButtonDeleteDCSBIOSActionButtonOff.IsEnabled = ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOff.Tag).ContainsDCSBIOS();
                ButtonDeleteOSCommandButtonOn.IsEnabled = ((TagDataClassStreamDeck)TextBoxOSCommandButtonOn.Tag).ContainsOSCommand();
                ButtonDeleteOSCommandButtonOff.IsEnabled = ((TagDataClassStreamDeck)TextBoxOSCommandButtonOff.Tag).ContainsOSCommand();


            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }


        public void Update()
        {
            try
            {
                LoadComboBoxesLayers();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetTextBoxTagObjects()
        {
            foreach (var textBox in _textBoxes)
            {
                textBox.Tag = new TagDataClassStreamDeck(textBox, new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), !textBox.Name.Contains("Off")));
            }
        }


        private void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public bool HasConfig
        {
            get
            {
                switch (SDUIParent.GetButtonActionType())
                {
                    case EnumStreamDeckButtonActionType.KeyPress:
                        {
                            return ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsKeySequence() ||
                                   ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsSingleKey() ||
                                   ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsKeySequence() ||
                                   ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsSingleKey();
                        }
                    case EnumStreamDeckButtonActionType.DCSBIOS:
                        {
                            return ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOn.Tag).ContainsDCSBIOS() ||
                                   ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOff.Tag).ContainsDCSBIOS();
                        }
                    case EnumStreamDeckButtonActionType.OSCommand:
                        {
                            return ((TagDataClassStreamDeck)TextBoxOSCommandButtonOn.Tag).ContainsOSCommand() ||
                                   ((TagDataClassStreamDeck)TextBoxOSCommandButtonOff.Tag).ContainsOSCommand();
                        }
                    case EnumStreamDeckButtonActionType.LayerNavigation:
                        {
                            return ((TagDataClassStreamDeck)TextBoxLayerNavButtonOn.Tag).ContainsStreamDeckLayer() ||
                                   ((TagDataClassStreamDeck)TextBoxLayerNavButtonOff.Tag).ContainsStreamDeckLayer();
                        }
                }
                return false;
            }
        }

        public IStreamDeckButtonAction GetStreamDeckButtonActionForPress()
        {
            switch (SDUIParent.GetButtonActionType())
            {
                case EnumStreamDeckButtonActionType.KeyPress:
                    {
                        var result = new KeyBindingStreamDeck();
                        result.WhenTurnedOn = true;
                        result.OSKeyPress = ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).OSKeyPress;
                        return result;
                    }
                case EnumStreamDeckButtonActionType.DCSBIOS:
                    {
                        var result = new DCSBIOSActionBindingStreamDeck();
                        result.WhenTurnedOn = true;
                        return result;
                    }
                case EnumStreamDeckButtonActionType.OSCommand:
                    {
                        var result = new OSCommandBindingStreamDeck();
                        result.WhenTurnedOn = true;
                        result.OSCommandObject = ((TagDataClassStreamDeck)TextBoxOSCommandButtonOn.Tag).OSCommandObject;
                        return result;
                    }
                case EnumStreamDeckButtonActionType.SRS:
                    {
                        throw new NotImplementedException("SRS not yet implemented.");
                    }
                case EnumStreamDeckButtonActionType.LayerNavigation:
                    {
                        var result = new LayerBindingStreamDeck();
                        result.WhenTurnedOn = true;
                        result.StreamDeckLayerTarget = ((TagDataClassStreamDeck)TextBoxLayerNavButtonOn.Tag).StreamDeckLayerTarget;
                        return result;
                    }
            }

            throw new ArgumentException("GetStreamDeckButtonActionForPress, failed to determine Action Type for button");
        }
        
        public IGlobalHandler GlobalHandler
        {
            get => _globalHandler;
            set => _globalHandler = value;
        }

        public IStreamDeckUIParent SDUIParent
        {
            get => _streamDeckUIParent;
            set => _streamDeckUIParent = value;
        }


        public StreamDeckButtonOnOff GetStreamDeckButtonOnOff(TextBox textBox)
        {
            try
            {
                switch (_streamDeckUIParent.GetButtonActionType())
                {
                    case EnumStreamDeckButtonActionType.KeyPress:
                        {
                            if (textBox.Equals(TextBoxKeyPressButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), true);
                            }
                            if (textBox.Equals(TextBoxKeyPressButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.DCSBIOS:
                        {
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), true);
                            }
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.OSCommand:
                        {
                            if (textBox.Equals(TextBoxOSCommandButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), true);
                            }
                            if (textBox.Equals(TextBoxOSCommandButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.LayerNavigation:
                        {
                            /*if (textBox.Equals(ComboBoxLayerNavigationButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(ComboBoxLayerNavigationButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }
                            */
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to determine focused component (GetStreamDeckButtonOnOff) ");
        }













        private void ButtonAddEditKeySequenceButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOn);
        }

        private void ButtonAddEditKeySequenceButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOff);
        }

        private void AddEditKeyPress(TextBox textBox)
        {
            try
            {
                var keySequenceWindow = ((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence() ?
                    new KeySequenceWindow(textBox.Text, ((TagDataClassStreamDeck)textBox.Tag).GetKeySequence()) :
                    new KeySequenceWindow();
                keySequenceWindow.ShowDialog();
                if (keySequenceWindow.DialogResult.HasValue && keySequenceWindow.DialogResult.Value)
                {
                    //Clicked OK
                    //If the user added only a single key stroke combo then let's not treat this as a sequence
                    if (!keySequenceWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var sequenceList = keySequenceWindow.GetSequence;
                    if (sequenceList.Count > 1)
                    {
                        var osKeyPress = new KeyPress("Key press sequence", sequenceList);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassStreamDeck)textBox.Tag).ClearAll();
                        var osKeyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                    }
                    SetIsDirty();
                    SDUIParent.ChildChangesMade();
                }
                SetFormState();
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void ButtonDeleteKeySequenceButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ClearAll();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteKeySequenceButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ClearAll();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }














        private void AddEditDCSBIOS(TextBox textBox)
        {
            try
            {
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                var tagDataClass = (TagDataClassStreamDeck)textBox.Tag;
                DCSBIOSControlsConfigsWindow dcsbiosControlsConfigsWindow;

                if (tagDataClass.ContainsDCSBIOS())
                {
                    dcsbiosControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(),
                        textBox.Name.Replace("TextBox", ""),
                        tagDataClass.DCSBIOSBinding.DCSBIOSInputs,
                        textBox.Text);
                }
                else
                {
                    dcsbiosControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);
                }

                dcsbiosControlsConfigsWindow.ShowDialog();


                if (dcsbiosControlsConfigsWindow.DialogResult.HasValue && dcsbiosControlsConfigsWindow.DialogResult == true)
                {
                    var dcsBiosInputs = dcsbiosControlsConfigsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsbiosControlsConfigsWindow.Description) ? "DCS-BIOS" : dcsbiosControlsConfigsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    tagDataClass.Consume(dcsBiosInputs);
                    tagDataClass.DCSBIOSBinding.WhenTurnedOn = !textBox.Name.Contains("Off");
                    SetIsDirty();
                    SDUIParent.ChildChangesMade();
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }

        private void ButtonAddEditDCSBIOSActionButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditDCSBIOS(TextBoxDCSBIOSActionButtonOn);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteDCSBIOSActionButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOn.Tag).ClearAll();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonAddEditDCSBIOSActionButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditDCSBIOS(TextBoxDCSBIOSActionButtonOff);
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteDCSBIOSActionButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxDCSBIOSActionButtonOff.Tag).ClearAll();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }










        private void AddEditOSCommand(TextBox textBox)
        {
            try
            {
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                OSCommandWindow osCommandWindow;
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(((TagDataClassStreamDeck)textBox.Tag).OSCommandObject);
                }
                else
                {
                    osCommandWindow = new OSCommandWindow();
                }
                osCommandWindow.ShowDialog();
                if (osCommandWindow.DialogResult.HasValue && osCommandWindow.DialogResult.Value)
                {
                    //Clicked OK
                    if (!osCommandWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var osCommand = osCommandWindow.OSCommandObject;
                    ((TagDataClassStreamDeck)textBox.Tag).OSCommandObject = osCommand;
                    textBox.Text = osCommand.Name;
                    SetIsDirty();
                }
                ButtonFocus.Focus();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void ButtonAddEditOSCommandButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditOSCommand(TextBoxOSCommandButtonOn);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteOSCommandButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxOSCommandButtonOn.Tag).ClearAll();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonAddEditOSCommandButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditOSCommand(TextBoxOSCommandButtonOff);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteOSCommandButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TagDataClassStreamDeck)TextBoxOSCommandButtonOff.Tag).ClearAll();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }










        private void ComboBoxLayerNavigationButtonOn_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var target = new StreamDeckTargetLayer();
                target.TargetLayer = ComboBoxLayerNavigationButtonOn.Text;
                ((TagDataClassStreamDeck)TextBoxLayerNavButtonOn.Tag).StreamDeckLayerTarget = target;
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ComboBoxLayerNavigationButtonOff_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var target = new StreamDeckTargetLayer();
                target.TargetLayer = ComboBoxLayerNavigationButtonOn.Text;
                ((TagDataClassStreamDeck)ComboBoxLayerNavigationButtonOff.Tag).StreamDeckLayerTarget = target;
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LoadComboBoxesLayers()
        {
            LoadComboBoxLayers(SDUIParent.GetSelectedStreamDeckLayer().Name, 
                ComboBoxLayerNavigationButtonOn,
                ComboBoxLayerNavigationButtonOn_OnSelectionChanged);

            LoadComboBoxLayers(SDUIParent.GetSelectedStreamDeckLayer().Name,
                ComboBoxLayerNavigationButtonOff,
                ComboBoxLayerNavigationButtonOff_OnSelectionChanged);
        }

        private void LoadComboBoxLayers(StreamDeckLayer selectedLayer, ComboBox comboBox, SelectionChangedEventHandler eventHandler)
        {
            LoadComboBoxLayers(selectedLayer.Name, comboBox, eventHandler);
        }

        private void LoadComboBoxLayers(string selectedLayerName, ComboBox comboBox, SelectionChangedEventHandler eventHandler)
        {
            var selectedIndex = comboBox.SelectedIndex;

            comboBox.SelectionChanged -= eventHandler;
            var list = SDUIParent.GetStreamDeckLayerNames();
            list.Insert(0, "Back to previous layer");
            list.Insert(0, "Go to home layer");
            comboBox.ItemsSource = SDUIParent.GetStreamDeckLayerNames();
            comboBox.Items.Refresh();

            if (!string.IsNullOrEmpty(selectedLayerName))
            {
                foreach (string layer in comboBox.Items)
                {
                    if (layer == selectedLayerName)
                    {
                        comboBox.SelectedItem = layer;
                        break;
                    }
                }
                comboBox.SelectedItem = selectedLayerName;
            }
            else if (selectedIndex >= 0 && selectedIndex < SDUIParent.GetStreamDeckLayerNames().Count)
            {
                comboBox.SelectedIndex = selectedIndex;
            }
            else if (SDUIParent.GetStreamDeckLayerNames().Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
            comboBox.SelectionChanged += eventHandler;
        }
    }
}
