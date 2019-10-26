using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            ComboBoxReleaseDelaySetHandlerState(true);
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
            _textBoxes.Add(TextBoxLayerNavButton);
        }

        public void Clear()
        {
            foreach (var textBox in _textBoxes)
            {
                ((TagDataClassStreamDeck)textBox.Tag).ClearAll();
            }

            ComboBoxLayerNavigationButton.SelectedIndex = 0;

            ComboBoxReleaseDelaySetHandlerState(false);
            ComboBoxReleaseDelayKeyPress.SelectedIndex = 0;
            ComboBoxReleaseDelayDCSBIOS.SelectedIndex = 0;
            ComboBoxReleaseDelayOSCommand.SelectedIndex = 0;
            ComboBoxReleaseDelayLayerNav.SelectedIndex = 0;
            ComboBoxReleaseDelaySetHandlerState(true);

            _isDirty = false;
            SDUIParent.ChildChangesMade();
        }

        public void SetFormState()
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                StackPanelButtonKeyPressSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckButtonActionType.KeyPress ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckButtonActionType.DCSBIOS ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonOSCommandSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckButtonActionType.OSCommand ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonLayerNavigationSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckButtonActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                ButtonDeleteKeySequenceButtonOn.IsEnabled = ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsKeySequence() ||
                                                            ((TagDataClassStreamDeck)TextBoxKeyPressButtonOn.Tag).ContainsKeyPress();
                ButtonDeleteKeySequenceButtonOff.IsEnabled = ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsKeySequence() ||
                                                            ((TagDataClassStreamDeck)TextBoxKeyPressButtonOff.Tag).ContainsKeyPress();
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

        public void StateSaved()
        {
            _isDirty = false;
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
                switch (SDUIParent.GetSelectedActionType())
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
                            return ((TagDataClassStreamDeck)TextBoxLayerNavButton.Tag).ContainsStreamDeckLayer();
                        }
                }
                return false;
            }
        }

        public void ShowActionConfiguration(StreamDeckButton streamDeckButton)
        {
            Clear();
            if (streamDeckButton == null)
            {
                return;
            }
            ShowActionConfiguration(streamDeckButton.StreamDeckButtonActionForPress);
            ShowActionConfiguration(streamDeckButton.StreamDeckButtonActionForRelease);
        }

        public void ShowActionConfiguration(IStreamDeckButtonAction streamDeckButtonAction)
        {
            if (streamDeckButtonAction == null)
            {
                return;
            }

            switch (streamDeckButtonAction.ActionType)
            {
                case EnumStreamDeckButtonActionType.KeyPress:
                    {
                        var keyBindingStreamDeck = (KeyBindingStreamDeck)streamDeckButtonAction;
                        var textBoxKeyPress = keyBindingStreamDeck.WhenTurnedOn ? TextBoxKeyPressButtonOn : TextBoxKeyPressButtonOff;
                        ((TagDataClassStreamDeck)textBoxKeyPress.Tag).KeyPress = keyBindingStreamDeck.OSKeyPress;
                        if (!keyBindingStreamDeck.WhenTurnedOn)
                        {
                            ComboBoxReleaseDelayKeyPress.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
                            ComboBoxReleaseDelayKeyPress.SelectedValue = keyBindingStreamDeck.ExecutionDelay;
                            ComboBoxReleaseDelayKeyPress.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
                        }
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckButtonActionType.DCSBIOS:
                    {
                        var dcsBIOSBinding = (DCSBIOSActionBindingStreamDeck)streamDeckButtonAction;
                        var textBoxDCSBIOS = dcsBIOSBinding.WhenTurnedOn ? TextBoxDCSBIOSActionButtonOn : TextBoxDCSBIOSActionButtonOff;
                        ((TagDataClassStreamDeck)textBoxDCSBIOS.Tag).DCSBIOSBinding = dcsBIOSBinding;
                        if (!dcsBIOSBinding.WhenTurnedOn)
                        {
                            ComboBoxReleaseDelayDCSBIOS.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
                            ComboBoxReleaseDelayDCSBIOS.SelectedValue = dcsBIOSBinding.ExecutionDelay;
                            ComboBoxReleaseDelayDCSBIOS.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
                        }
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckButtonActionType.OSCommand:
                    {
                        var osCommandBindingStreamDeck = (OSCommandBindingStreamDeck)streamDeckButtonAction;
                        var textBoxOSCommand = osCommandBindingStreamDeck.WhenTurnedOn ? TextBoxOSCommandButtonOn : TextBoxOSCommandButtonOff;
                        ((TagDataClassStreamDeck)textBoxOSCommand.Tag).OSCommandObject = osCommandBindingStreamDeck.OSCommandObject;
                        if (!osCommandBindingStreamDeck.WhenTurnedOn)
                        {
                            ComboBoxReleaseDelayOSCommand.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
                            ComboBoxReleaseDelayOSCommand.SelectedValue = osCommandBindingStreamDeck.ExecutionDelay;
                            ComboBoxReleaseDelayOSCommand.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
                        }
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckButtonActionType.LayerNavigation:
                    {
                        var layerBindingStreamDeck = (LayerBindingStreamDeck)streamDeckButtonAction;
                        ((TagDataClassStreamDeck)TextBoxLayerNavButton.Tag).StreamDeckLayerTarget = layerBindingStreamDeck.StreamDeckLayerTarget;
                        if (!layerBindingStreamDeck.WhenTurnedOn)
                        {
                            ComboBoxReleaseDelayLayerNav.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
                            ComboBoxReleaseDelayLayerNav.SelectedValue = layerBindingStreamDeck.ExecutionDelay;
                            ComboBoxReleaseDelayLayerNav.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
                        }
                        SetFormState();
                        return;
                    }
            }

            throw new ArgumentException("ShowActionConfiguration, failed to determine Action Type for button");
        }


        public IStreamDeckButtonAction GetStreamDeckButtonAction(bool forButtonPressed)
        {
            //TextBox textBoxSRS;

            var textBoxKeyPress = forButtonPressed ? TextBoxKeyPressButtonOn : TextBoxKeyPressButtonOff;
            var textBoxDCSBIOS = forButtonPressed ? TextBoxDCSBIOSActionButtonOn : TextBoxDCSBIOSActionButtonOff;
            var textBoxOSCommand = forButtonPressed ? TextBoxOSCommandButtonOn : TextBoxOSCommandButtonOff;

            switch (SDUIParent.GetSelectedActionType())
            {
                case EnumStreamDeckButtonActionType.KeyPress:
                    {
                        KeyBindingStreamDeck result;

                        if (((TagDataClassStreamDeck)textBoxKeyPress.Tag).ContainsKeyPress())
                        {
                            result = new KeyBindingStreamDeck();
                            result.WhenTurnedOn = forButtonPressed;
                            result.OSKeyPress = ((TagDataClassStreamDeck)textBoxKeyPress.Tag).KeyPress;
                            result.ExecutionDelay = forButtonPressed ? 0 : int.Parse(ComboBoxReleaseDelayKeyPress.Text);
                            
                            return result;
                        }

                        return null;
                    }
                case EnumStreamDeckButtonActionType.DCSBIOS:
                    {
                        if (((TagDataClassStreamDeck)textBoxDCSBIOS.Tag).ContainsDCSBIOS())
                        {
                            ((TagDataClassStreamDeck)textBoxDCSBIOS.Tag).DCSBIOSBinding.WhenTurnedOn = forButtonPressed;
                            ((TagDataClassStreamDeck)textBoxDCSBIOS.Tag).DCSBIOSBinding.ExecutionDelay = forButtonPressed ? 0 : int.Parse(ComboBoxReleaseDelayDCSBIOS.Text);
                            
                            return ((TagDataClassStreamDeck)textBoxDCSBIOS.Tag).DCSBIOSBinding;
                        }
                        return null;
                    }
                case EnumStreamDeckButtonActionType.OSCommand:
                    {
                        if (((TagDataClassStreamDeck)textBoxOSCommand.Tag).ContainsOSCommand())
                        {
                            var result = new OSCommandBindingStreamDeck();
                            result.WhenTurnedOn = forButtonPressed;
                            result.OSCommandObject = ((TagDataClassStreamDeck)textBoxOSCommand.Tag).OSCommandObject;
                            result.ExecutionDelay = forButtonPressed ? 0 : int.Parse(ComboBoxReleaseDelayOSCommand.Text);
                            
                            return result;
                        }
                        return null;
                    }
                case EnumStreamDeckButtonActionType.LayerNavigation:
                    {
                        if (((TagDataClassStreamDeck)TextBoxLayerNavButton.Tag).ContainsStreamDeckLayer())
                        {
                            var result = new LayerBindingStreamDeck();
                            result.WhenTurnedOn = forButtonPressed;
                            result.StreamDeckLayerTarget = ((TagDataClassStreamDeck)TextBoxLayerNavButton.Tag).StreamDeckLayerTarget;
                            result.ExecutionDelay = forButtonPressed ? 0 : int.Parse(ComboBoxReleaseDelayLayerNav.Text);

                            return result;
                        }

                        return null;
                    }
            }

            throw new ArgumentException("GetStreamDeckButtonAction, failed to determine Action Type for button");
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
                switch (_streamDeckUIParent.GetSelectedActionType())
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
                        var keyPress = new KeyPress("Key press sequence", sequenceList);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = keyPress;
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
                        var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = keyPress;
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






        private void ClearLayerComboBoxes()
        {
            ClearComboBox(ComboBoxLayerNavigationButton, ComboBoxLayerNavigationButton_OnSelectionChanged);
        }

        private void ClearComboBox(ComboBox comboBox, SelectionChangedEventHandler eventHandler)
        {
            comboBox.SelectionChanged -= eventHandler;
            comboBox.ItemsSource = null;
            comboBox.SelectionChanged += eventHandler;
        }

        private void ComboBoxLayerNavigationButton_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var target = new StreamDeckTargetLayer();
                target.TargetLayer = ComboBoxLayerNavigationButton.Text;
                ((TagDataClassStreamDeck)TextBoxLayerNavButton.Tag).StreamDeckLayerTarget = target;
                SetIsDirty();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void LoadComboBoxesLayers()
        {
            if (SDUIParent.GetSelectedStreamDeckLayer() == null)
            {
                return;
            }
            LoadComboBoxLayers(SDUIParent.GetSelectedStreamDeckLayer().Name,
                ComboBoxLayerNavigationButton,
                ComboBoxLayerNavigationButton_OnSelectionChanged);
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
            if (list == null)
            {
                return;
            }
            list.Insert(0, "Back to previous layer");
            list.Insert(0, "Go to home layer");
            comboBox.ItemsSource = list;
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

        private void LabelDelayedReleaseInfo_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                MessageBox.Show(
                    "Stream Deck doesn't send information when a button is released, only when the button is pressed.\nSet therefore a time delay when the button can be considered released after have being pressed.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelDelayedReleaseInfo_OnMouseEnter(object sender, MouseEventArgs e)
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

        private void LabelDelayedReleaseInfo_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ComboBoxReleaseDelaySetHandlerState(bool enableEventHandler)
        {
            if (enableEventHandler)
            {
                ComboBoxReleaseDelayKeyPress.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
                ComboBoxReleaseDelayDCSBIOS.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
                ComboBoxReleaseDelayOSCommand.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
                ComboBoxReleaseDelayLayerNav.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
            }
            else
            {
                ComboBoxReleaseDelayKeyPress.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
                ComboBoxReleaseDelayDCSBIOS.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
                ComboBoxReleaseDelayOSCommand.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
                ComboBoxReleaseDelayLayerNav.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
            }
        }

        private void ComboBoxReleaseDelay_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetIsDirty();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
