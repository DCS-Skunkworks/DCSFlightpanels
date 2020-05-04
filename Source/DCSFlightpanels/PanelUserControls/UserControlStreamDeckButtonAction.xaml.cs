using System;
using System.Collections.Generic;
using System.Windows;
using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Windows;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;
using NonVisuals.StreamDeck.Events;


namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonAction.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonAction : UserControlBase, IIsDirty, IStreamDeckListener
    {
        private List<StreamDeckActionTextBox> _textBoxes = new List<StreamDeckActionTextBox>();

        private StreamDeckButton _streamDeckButton;
        private IGlobalHandler _globalHandler;
        private bool _isLoaded = false;
        private bool _isDirty = false;
        public string PanelHash;




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
            SetTextBoxBills();
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
                textBox.Bill.Clear();
            }

            RadioButtonKeyPress.IsChecked = false;
            RadioButtonDCSBIOS.IsChecked = false;
            RadioButtonOSCommand.IsChecked = false;
            RadioButtonLayerNav.IsChecked = false;

            ComboBoxLayerNavigationButton.SelectedIndex = 0;

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
                StackPanelButtonKeyPressSettings.Visibility = GetSelectedActionType() == EnumStreamDeckActionType.KeyPress ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSSettings.Visibility = GetSelectedActionType() == EnumStreamDeckActionType.DCSBIOS ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonOSCommandSettings.Visibility = GetSelectedActionType() == EnumStreamDeckActionType.OSCommand ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonLayerNavigationSettings.Visibility = GetSelectedActionType() == EnumStreamDeckActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                //StackPanelChooseButtonActionType.IsEnabled = StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName != EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;

                ButtonDeleteKeySequenceButtonOn.IsEnabled = TextBoxKeyPressButtonOn.Bill.ContainsKeySequence() ||
                                                            TextBoxKeyPressButtonOn.Bill.ContainsKeyPress();
                ButtonDeleteKeySequenceButtonOff.IsEnabled = TextBoxKeyPressButtonOff.Bill.ContainsKeySequence() ||
                                                            TextBoxKeyPressButtonOff.Bill.ContainsKeyPress();
                ButtonDeleteDCSBIOSActionButtonOn.IsEnabled = TextBoxDCSBIOSActionButtonOn.Bill.ContainsDCSBIOS();
                ButtonDeleteDCSBIOSActionButtonOff.IsEnabled = TextBoxDCSBIOSActionButtonOff.Bill.ContainsDCSBIOS();
                ButtonDeleteOSCommandButtonOn.IsEnabled = TextBoxOSCommandButtonOn.Bill.ContainsOSCommand();
                ButtonDeleteOSCommandButtonOff.IsEnabled = TextBoxOSCommandButtonOff.Bill.ContainsOSCommand();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void Update()
        {
            try
            {
                LoadComboBoxLayers();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        public void SetButtonActionType()
        {

            if (_streamDeckButton == null)
            {
                return;
            }

            RadioButtonKeyPress.IsChecked = false;
            RadioButtonDCSBIOS.IsChecked = false;
            RadioButtonLayerNav.IsChecked = false;
            RadioButtonOSCommand.IsChecked = false;

            switch (_streamDeckButton.ActionType)
            {
                case EnumStreamDeckActionType.KeyPress:
                    {
                        RadioButtonKeyPress.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckActionType.DCSBIOS:
                    {
                        RadioButtonDCSBIOS.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckActionType.OSCommand:
                    {
                        RadioButtonOSCommand.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckActionType.LayerNavigation:
                    {
                        RadioButtonLayerNav.IsChecked = true;
                        break;
                    }
            }
        }

        private void RadioButtonButtonActionTypePress_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        public EnumStreamDeckActionType GetSelectedActionType()
        {
            if (RadioButtonKeyPress.IsChecked == true)
            {
                return EnumStreamDeckActionType.KeyPress;
            }
            if (RadioButtonDCSBIOS.IsChecked == true)
            {
                return EnumStreamDeckActionType.DCSBIOS;
            }
            if (RadioButtonOSCommand.IsChecked == true)
            {
                return EnumStreamDeckActionType.OSCommand;
            }
            if (RadioButtonLayerNav.IsChecked == true)
            {
                return EnumStreamDeckActionType.LayerNavigation;
            }

            return EnumStreamDeckActionType.Unknown;
        }

        private void SetTextBoxBills()
        {
            foreach (var textBox in _textBoxes)
            {
                textBox.Bill = new BillStreamDeckAction(textBox, new StreamDeckButtonOnOff(StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName, !textBox.Name.Contains("Off")));
            }
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        public void SetIsDirty()
        {
            _isDirty = true;
            EventHandlers.SenderNotifiesIsDirty(this, _streamDeckButton.StreamDeckButtonName, "");
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
                switch (GetSelectedActionType())
                {
                    case EnumStreamDeckActionType.KeyPress:
                        {
                            return TextBoxKeyPressButtonOn.Bill.ContainsKeySequence() ||
                                   TextBoxKeyPressButtonOn.Bill.ContainsSingleKey() ||
                                   TextBoxKeyPressButtonOff.Bill.ContainsKeySequence() ||
                                   TextBoxKeyPressButtonOff.Bill.ContainsSingleKey();
                        }
                    case EnumStreamDeckActionType.DCSBIOS:
                        {
                            return TextBoxDCSBIOSActionButtonOn.Bill.ContainsDCSBIOS() ||
                                   TextBoxDCSBIOSActionButtonOff.Bill.ContainsDCSBIOS();
                        }
                    case EnumStreamDeckActionType.OSCommand:
                        {
                            return TextBoxOSCommandButtonOn.Bill.ContainsOSCommand() ||
                                   TextBoxOSCommandButtonOff.Bill.ContainsOSCommand();
                        }
                    case EnumStreamDeckActionType.LayerNavigation:
                        {
                            return TextBoxLayerNavButton.Bill.ContainsStreamDeckLayer();
                        }
                }
                return false;
            }
        }

        public void ShowStreamDeckButton(StreamDeckButton streamDeckButton)
        {
            Clear();
            _streamDeckButton = streamDeckButton;
            LoadComboBoxLayers();
            if (streamDeckButton == null)
            {
                return;
            }
            SetButtonActionType();
            ShowActionConfiguration(streamDeckButton.ActionForPress);
            ShowActionConfiguration(streamDeckButton.ActionForRelease);
        }

        private void ShowActionConfiguration(IStreamDeckButtonAction streamDeckButtonAction)
        {
            if (streamDeckButtonAction == null)
            {
                return;
            }

            switch (streamDeckButtonAction.ActionType)
            {
                case EnumStreamDeckActionType.KeyPress:
                    {
                        var keyBindingStreamDeck = (ActionTypeKey)streamDeckButtonAction;
                        var textBoxKeyPress = keyBindingStreamDeck.WhenTurnedOn ? TextBoxKeyPressButtonOn : TextBoxKeyPressButtonOff;
                        textBoxKeyPress.Bill.KeyPress = keyBindingStreamDeck.OSKeyPress;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckActionType.DCSBIOS:
                    {
                        var dcsBIOSBinding = (ActionTypeDCSBIOS)streamDeckButtonAction;
                        var textBoxDCSBIOS = dcsBIOSBinding.WhenTurnedOn ? TextBoxDCSBIOSActionButtonOn : TextBoxDCSBIOSActionButtonOff;
                        textBoxDCSBIOS.Bill.DCSBIOSBinding = dcsBIOSBinding;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckActionType.OSCommand:
                    {
                        var osCommandBindingStreamDeck = (ActionTypeOS)streamDeckButtonAction;
                        var textBoxOSCommand = osCommandBindingStreamDeck.WhenTurnedOn ? TextBoxOSCommandButtonOn : TextBoxOSCommandButtonOff;
                        textBoxOSCommand.Bill.OSCommandObject = osCommandBindingStreamDeck.OSCommandObject;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckActionType.LayerNavigation:
                    {
                        var layerBindingStreamDeck = (ActionTypeLayer)streamDeckButtonAction;
                        TextBoxLayerNavButton.Bill.StreamDeckLayerTarget = layerBindingStreamDeck;
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

            switch (GetSelectedActionType())
            {
                case EnumStreamDeckActionType.KeyPress:
                    {
                        if (textBoxKeyPress.Bill.ContainsKeyPress())
                        {
                            ActionTypeKey result;
                            result = new ActionTypeKey();
                            result.StreamDeckButtonName = _streamDeckButton.StreamDeckButtonName;
                            result.PanelHash = PanelHash;
                            result.WhenTurnedOn = forButtonPressed;
                            result.OSKeyPress = textBoxKeyPress.Bill.KeyPress;

                            return result;
                        }

                        return null;
                    }
                case EnumStreamDeckActionType.DCSBIOS:
                    {
                        if (textBoxDCSBIOS.Bill.ContainsDCSBIOS())
                        {
                            textBoxDCSBIOS.Bill.DCSBIOSBinding.WhenTurnedOn = forButtonPressed;
                            textBoxDCSBIOS.Bill.DCSBIOSBinding.StreamDeckButtonName = _streamDeckButton.StreamDeckButtonName;
                            textBoxDCSBIOS.Bill.DCSBIOSBinding.PanelHash = PanelHash;
                            return textBoxDCSBIOS.Bill.DCSBIOSBinding;
                        }
                        return null;
                    }
                case EnumStreamDeckActionType.OSCommand:
                    {
                        if (textBoxOSCommand.Bill.ContainsOSCommand())
                        {
                            var result = new ActionTypeOS();
                            result.WhenTurnedOn = forButtonPressed;
                            result.OSCommandObject = textBoxOSCommand.Bill.OSCommandObject;
                            result.StreamDeckButtonName = _streamDeckButton.StreamDeckButtonName;
                            result.PanelHash = PanelHash;
                            return result;
                        }
                        return null;
                    }
                case EnumStreamDeckActionType.LayerNavigation:
                    {
                        if (!forButtonPressed)
                        {
                            return null;
                        }
                        if (TextBoxLayerNavButton.Bill.ContainsStreamDeckLayer())
                        {
                            var result = TextBoxLayerNavButton.Bill.StreamDeckLayerTarget;
                            result.StreamDeckButtonName = _streamDeckButton.StreamDeckButtonName;
                            result.PanelHash = PanelHash;
                            return result;
                        }

                        return null;
                    }
                case EnumStreamDeckActionType.Unknown:
                    {
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
        
        private void ButtonAddEditKeySequenceButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOn);
        }

        private void ButtonAddEditKeySequenceButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOff);
        }

        private void AddEditKeyPress(StreamDeckActionTextBox textBox)
        {
            try
            {
                var keySequenceWindow = textBox.Bill.ContainsKeySequence() ?
                    new KeySequenceWindow(textBox.Text, textBox.Bill.GetKeySequence()) :
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
                        textBox.Bill.KeyPress = keyPress;
                        textBox.Bill.KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        textBox.Bill.Clear();
                        var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        textBox.Bill.KeyPress = keyPress;
                        textBox.Bill.KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                    }
                    SetIsDirty();
                }
                SetFormState();
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonDeleteKeySequenceButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxKeyPressButtonOn.Bill.Clear();
                SetIsDirty();
                
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
                TextBoxKeyPressButtonOff.Bill.Clear();
                SetIsDirty();
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void AddEditDCSBIOS(StreamDeckActionTextBox textBox)
        {
            try
            {
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                DCSBIOSInputControlsWindow dcsbiosControlsConfigsWindow;

                if (textBox.Bill.ContainsDCSBIOS())
                {
                    dcsbiosControlsConfigsWindow = new DCSBIOSInputControlsWindow(_globalHandler.GetAirframe(),
                        textBox.Name.Replace("TextBox", ""),
                        textBox.Bill.DCSBIOSBinding.DCSBIOSInputs,
                        textBox.Text,
                        true);

                    dcsbiosControlsConfigsWindow.IsSequenced = textBox.Bill.DCSBIOSBinding.IsSequenced; //Add on, not optimal structure
                }
                else
                {
                    dcsbiosControlsConfigsWindow = new DCSBIOSInputControlsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null, true);
                }

                dcsbiosControlsConfigsWindow.ShowDialog();


                if (dcsbiosControlsConfigsWindow.DialogResult.HasValue && dcsbiosControlsConfigsWindow.DialogResult == true)
                {
                    var dcsBiosInputs = dcsbiosControlsConfigsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsbiosControlsConfigsWindow.Description) ? "DCS-BIOS" : dcsbiosControlsConfigsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    textBox.Bill.Consume(dcsBiosInputs);
                    textBox.Bill.DCSBIOSBinding.WhenTurnedOn = !textBox.Name.Contains("Off");
                    textBox.Bill.DCSBIOSBinding.IsSequenced = dcsbiosControlsConfigsWindow.IsSequenced;
                    SetIsDirty();
                    
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                TextBoxDCSBIOSActionButtonOn.Bill.Clear();
                
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
                TextBoxDCSBIOSActionButtonOff.Bill.Clear();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void AddEditOSCommand(StreamDeckActionTextBox textBox)
        {
            try
            {
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                OSCommandWindow osCommandWindow;
                if (textBox.Bill.ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(textBox.Bill.OSCommandObject);
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
                    textBox.Bill.OSCommandObject = osCommand;
                    textBox.Text = osCommand.Name;
                    SetIsDirty();
                }
                ButtonFocus.Focus();
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                TextBoxOSCommandButtonOn.Bill.Clear();
                
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
                TextBoxOSCommandButtonOff.Bill.Clear();
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ComboBoxLayerNavigationButton_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                var target = new ActionTypeLayer();
                target.TargetLayer = ComboBoxLayerNavigationButton.Text;
                switch (ComboBoxLayerNavigationButton.Text)
                {
                    case StreamDeckConstants.GO_TO_HOME_LAYER_STRING:
                        {
                            target.NavigationType = LayerNavType.Home;
                            break;
                        }
                    case StreamDeckConstants.GO_BACK_ONE_LAYER_STRING:
                        {
                            target.NavigationType = LayerNavType.Back;
                            break;
                        }
                    default:
                        {
                            target.NavigationType = LayerNavType.SwitchToSpecificLayer;
                            break;
                        }
                }

                TextBoxLayerNavButton.Bill.StreamDeckLayerTarget = target;
                ActionTypeChangedLayerNavigation(StreamDeckConstants.TranslateLayerName(ComboBoxLayerNavigationButton.Text));
                SetIsDirty();
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LoadComboBoxLayers()
        {

            if (StreamDeckPanel.GetInstance(PanelHash).SelectedLayer == null)
            {
                return;
            }

            var selectedLayerName = StreamDeckPanel.GetInstance(PanelHash).SelectedLayer.Name;

            var selectedIndex = ComboBoxLayerNavigationButton.SelectedIndex;

            var layerList = StreamDeckPanel.GetInstance(PanelHash).GetStreamDeckLayerNames();

            if (layerList == null)
            {
                return;
            }
            layerList.Insert(0, StreamDeckConstants.GO_BACK_ONE_LAYER_STRING);
            layerList.Insert(0, StreamDeckConstants.GO_TO_HOME_LAYER_STRING);
            ComboBoxLayerNavigationButton.ItemsSource = layerList;
            ComboBoxLayerNavigationButton.Items.Refresh();

            if (!string.IsNullOrEmpty(selectedLayerName))
            {
                foreach (string layer in ComboBoxLayerNavigationButton.Items)
                {
                    if (layer == selectedLayerName)
                    {
                        ComboBoxLayerNavigationButton.SelectedItem = layer;
                        break;
                    }
                }
                ComboBoxLayerNavigationButton.SelectedItem = selectedLayerName;
            }
            else if (selectedIndex >= 0 && selectedIndex < layerList.Count)
            {
                ComboBoxLayerNavigationButton.SelectedIndex = selectedIndex;
            }
            else if (layerList.Count > 0)
            {
                ComboBoxLayerNavigationButton.SelectedIndex = 0;
            }
        }

        public interface IStreamDeckButtonActionListener
        {
            void ActionTypeChangedEvent(object sender, ActionTypeChangedEventArgs e);
        }

        public virtual void AttachListener(IStreamDeckButtonActionListener buttonActionListener)
        {
            OnActionTypeChanged += buttonActionListener.ActionTypeChangedEvent;
        }

        public virtual void DetachListener(IStreamDeckButtonActionListener buttonActionListener)
        {
            OnActionTypeChanged -= buttonActionListener.ActionTypeChangedEvent;
        }

        public delegate void ActionTypeChangedEventHandler(object sender, ActionTypeChangedEventArgs e);
        public event ActionTypeChangedEventHandler OnActionTypeChanged;

        public class ActionTypeChangedEventArgs : EventArgs
        {
            public EnumStreamDeckActionType ActionType { get; set; }
            public string TargetLayerName { get; set; }
        }

        private void ActionTypeChangedLayerNavigation(string layerName)
        {
            var arguments = new ActionTypeChangedEventArgs();
            arguments.ActionType = GetSelectedActionType();
            arguments.TargetLayerName = layerName;
            OnActionTypeChanged?.Invoke(this, arguments);
        }
        
        public StreamDeckButtonOnOff GetStreamDeckButtonOnOff(StreamDeckActionTextBox textBox)
        {
            try
            {
                switch (GetSelectedActionType())
                {
                    case EnumStreamDeckActionType.KeyPress:
                        {
                            if (textBox.Equals(TextBoxKeyPressButtonOn))
                            {
                                return new StreamDeckButtonOnOff( StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName, true);
                            }
                            if (textBox.Equals(TextBoxKeyPressButtonOff))
                            {
                                return new StreamDeckButtonOnOff(StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName, false);
                            }

                            break;
                        }
                    case EnumStreamDeckActionType.DCSBIOS:
                        {
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOn))
                            {
                                return new StreamDeckButtonOnOff(StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName, true);
                            }
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOff))
                            {
                                return new StreamDeckButtonOnOff(StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName, false);
                            }

                            break;
                        }
                    case EnumStreamDeckActionType.OSCommand:
                        {
                            if (textBox.Equals(TextBoxOSCommandButtonOn))
                            {
                                return new StreamDeckButtonOnOff(StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName, true);
                            }
                            if (textBox.Equals(TextBoxOSCommandButtonOff))
                            {
                                return new StreamDeckButtonOnOff(StreamDeckPanel.GetInstance(PanelHash).SelectedButtonName, false);
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
                Common.ShowErrorMessageBox( ex);
            }
            throw new Exception("Failed to determine focused component (GetStreamDeckButtonOnOff) ");
        }

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() =>
                {
                    Clear();
                    SetFormState();
                }));
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                ShowStreamDeckButton(StreamDeckPanel.GetInstance(PanelHash).SelectedButton);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e)
        {
            try
            {
                if (sender.Equals(this))
                {
                    return;
                }
                e.Cancel = IsDirty;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void ClearSettings(object sender, StreamDeckClearSettingsArgs e)
        {
            try
            {
                if (e.ClearActionConfiguration)
                {
                    Clear();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }
    }
}
