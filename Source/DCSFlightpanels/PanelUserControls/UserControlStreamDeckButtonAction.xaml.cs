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
                Tagg(textBox).ClearAll();
            }

            ComboBoxLayerNavigationButton.SelectedIndex = 0;
            
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
                StackPanelButtonKeyPressSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckActionType.KeyPress ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckActionType.DCSBIOS ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonOSCommandSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckActionType.OSCommand ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonLayerNavigationSettings.Visibility = SDUIParent.GetSelectedActionType() == EnumStreamDeckActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                ButtonDeleteKeySequenceButtonOn.IsEnabled = Tagg(TextBoxKeyPressButtonOn).ContainsKeySequence() ||
                                                            Tagg(TextBoxKeyPressButtonOn).ContainsKeyPress();
                ButtonDeleteKeySequenceButtonOff.IsEnabled = Tagg(TextBoxKeyPressButtonOff).ContainsKeySequence() ||
                                                            Tagg(TextBoxKeyPressButtonOff).ContainsKeyPress();
                ButtonDeleteDCSBIOSActionButtonOn.IsEnabled = Tagg(TextBoxDCSBIOSActionButtonOn).ContainsDCSBIOS();
                ButtonDeleteDCSBIOSActionButtonOff.IsEnabled = Tagg(TextBoxDCSBIOSActionButtonOff).ContainsDCSBIOS();
                ButtonDeleteOSCommandButtonOn.IsEnabled = Tagg(TextBoxOSCommandButtonOn).ContainsOSCommand();
                ButtonDeleteOSCommandButtonOff.IsEnabled = Tagg(TextBoxOSCommandButtonOff).ContainsOSCommand();
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
                textBox.Tag = new TagDataStreamDeckAction(textBox, new StreamDeckButtonOnOff(_streamDeckUIParent.GetSelectedButtonName(), !textBox.Name.Contains("Off")));
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
                    case EnumStreamDeckActionType.KeyPress:
                        {
                            return Tagg(TextBoxKeyPressButtonOn).ContainsKeySequence() ||
                                   Tagg(TextBoxKeyPressButtonOn).ContainsSingleKey() ||
                                   Tagg(TextBoxKeyPressButtonOff).ContainsKeySequence() ||
                                   Tagg(TextBoxKeyPressButtonOff).ContainsSingleKey();
                        }
                    case EnumStreamDeckActionType.DCSBIOS:
                        {
                            return Tagg(TextBoxDCSBIOSActionButtonOn).ContainsDCSBIOS() ||
                                   Tagg(TextBoxDCSBIOSActionButtonOff).ContainsDCSBIOS();
                        }
                    case EnumStreamDeckActionType.OSCommand:
                        {
                            return Tagg(TextBoxOSCommandButtonOn).ContainsOSCommand() ||
                                   Tagg(TextBoxOSCommandButtonOff).ContainsOSCommand();
                        }
                    case EnumStreamDeckActionType.LayerNavigation:
                        {
                            return Tagg(TextBoxLayerNavButton).ContainsStreamDeckLayer();
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
                case EnumStreamDeckActionType.KeyPress:
                    {
                        var keyBindingStreamDeck = (KeyBindingStreamDeck)streamDeckButtonAction;
                        var textBoxKeyPress = keyBindingStreamDeck.WhenTurnedOn ? TextBoxKeyPressButtonOn : TextBoxKeyPressButtonOff;
                        Tagg(textBoxKeyPress).KeyPress = keyBindingStreamDeck.OSKeyPress;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckActionType.DCSBIOS:
                    {
                        var dcsBIOSBinding = (DCSBIOSActionBindingStreamDeck)streamDeckButtonAction;
                        var textBoxDCSBIOS = dcsBIOSBinding.WhenTurnedOn ? TextBoxDCSBIOSActionButtonOn : TextBoxDCSBIOSActionButtonOff;
                        Tagg(textBoxDCSBIOS).DCSBIOSBinding = dcsBIOSBinding;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckActionType.OSCommand:
                    {
                        var osCommandBindingStreamDeck = (OSCommandBindingStreamDeck)streamDeckButtonAction;
                        var textBoxOSCommand = osCommandBindingStreamDeck.WhenTurnedOn ? TextBoxOSCommandButtonOn : TextBoxOSCommandButtonOff;
                        Tagg(textBoxOSCommand).OSCommandObject = osCommandBindingStreamDeck.OSCommandObject;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckActionType.LayerNavigation:
                    {
                        var layerBindingStreamDeck = (LayerBindingStreamDeck)streamDeckButtonAction;
                        Tagg(TextBoxLayerNavButton).StreamDeckLayerTarget = layerBindingStreamDeck.StreamDeckLayerTarget;
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
                case EnumStreamDeckActionType.KeyPress:
                    {
                        KeyBindingStreamDeck result;

                        if (Tagg(textBoxKeyPress).ContainsKeyPress())
                        {
                            result = new KeyBindingStreamDeck();
                            result.WhenTurnedOn = forButtonPressed;
                            result.OSKeyPress = Tagg(textBoxKeyPress).KeyPress;
                            
                            return result;
                        }

                        return null;
                    }
                case EnumStreamDeckActionType.DCSBIOS:
                    {
                        if (Tagg(textBoxDCSBIOS).ContainsDCSBIOS())
                        {
                            Tagg(textBoxDCSBIOS).DCSBIOSBinding.WhenTurnedOn = forButtonPressed;
                            
                            return Tagg(textBoxDCSBIOS).DCSBIOSBinding;
                        }
                        return null;
                    }
                case EnumStreamDeckActionType.OSCommand:
                    {
                        if (Tagg(textBoxOSCommand).ContainsOSCommand())
                        {
                            var result = new OSCommandBindingStreamDeck();
                            result.WhenTurnedOn = forButtonPressed;
                            result.OSCommandObject = Tagg(textBoxOSCommand).OSCommandObject;
                            
                            return result;
                        }
                        return null;
                    }
                case EnumStreamDeckActionType.LayerNavigation:
                    {
                        if (Tagg(TextBoxLayerNavButton).ContainsStreamDeckLayer())
                        {
                            var result = new LayerBindingStreamDeck();
                            result.WhenTurnedOn = forButtonPressed;
                            result.StreamDeckLayerTarget = Tagg(TextBoxLayerNavButton).StreamDeckLayerTarget;

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
                    case EnumStreamDeckActionType.KeyPress:
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
                    case EnumStreamDeckActionType.DCSBIOS:
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
                    case EnumStreamDeckActionType.OSCommand:
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
                    case EnumStreamDeckActionType.LayerNavigation:
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
                var keySequenceWindow = Tagg(textBox).ContainsKeySequence() ?
                    new KeySequenceWindow(textBox.Text, Tagg(textBox).GetKeySequence()) :
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
                        Tagg(textBox).KeyPress = keyPress;
                        Tagg(textBox).KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        Tagg(textBox).ClearAll();
                        var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        Tagg(textBox).KeyPress = keyPress;
                        Tagg(textBox).KeyPress.Information = keySequenceWindow.GetInformation;
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
                Tagg(TextBoxKeyPressButtonOn).ClearAll();
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
                Tagg(TextBoxKeyPressButtonOff).ClearAll();
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

                var tagDataClass = (TagDataStreamDeckAction)textBox.Tag;
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
                Tagg(TextBoxDCSBIOSActionButtonOn).ClearAll();
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
                Tagg(TextBoxDCSBIOSActionButtonOff).ClearAll();
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
                if (Tagg(textBox).ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(Tagg(textBox).OSCommandObject);
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
                    Tagg(textBox).OSCommandObject = osCommand;
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
                Tagg(TextBoxOSCommandButtonOn).ClearAll();
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
                Tagg(TextBoxOSCommandButtonOff).ClearAll();
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
                Tagg(TextBoxLayerNavButton).StreamDeckLayerTarget = target;
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




        private TagDataStreamDeckAction Tagg(TextBox textBox)
        {
            return (TagDataStreamDeckAction) textBox.Tag;
        }
    }
}
