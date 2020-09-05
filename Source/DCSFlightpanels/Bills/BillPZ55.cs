using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Interfaces;
using DCSFlightpanels.Windows;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Bills
{
    public class BillPZ55 : BillBaseInput
    {
        private DCSBIOSActionBindingPZ55 _dcsbiosBindingPZ55;
        private BIPLinkPZ55 _bipLinkPZ55;
        private ContextMenuPanelTextBox _contextMenuTextBox;
        private SwitchPanelPZ55 _switchPanelPZ55;
        private IGlobalHandler _globalHandler;
        private IPanelUIPZ55 _panelUI;
        private PZ55TextBox _pz55TextBox;

        public BillPZ55(IGlobalHandler globalHandler, IPanelUIPZ55 panelUI, SwitchPanelPZ55 switchPanelPZ55, PZ55TextBox textBox, SwitchPanelPZ55KeyOnOff key) : base()
        {
            _globalHandler = globalHandler;
            _panelUI = panelUI;
            _switchPanelPZ55 = switchPanelPZ55;
            TextBox = textBox;
            _pz55TextBox = textBox;
            Key = key;
            SetContextMenu();
        }

        private void SetContextMenu()
        {
            _contextMenuTextBox = new ContextMenuPanelTextBox(Common.IsOperationModeFlagSet(EmulationMode.KeyboardEmulationOnly));
            
            _contextMenuTextBox.IsVisibleChanged += TextBoxContextMenuIsVisibleChanged;
            _contextMenuTextBox.ContextMenuItemAddNullKey.Click += MenuItemAddNullKey_OnClick;
            _contextMenuTextBox.ContextMenuItemEditSequence.Click += MenuItemEditSequence_OnClick;
            _contextMenuTextBox.ContextMenuItemEditDCSBIOS.Click += MenuItemEditDCSBIOS_OnClick;
            _contextMenuTextBox.ContextMenuItemEditBIP.Click += MenuItemEditBIP_OnClick;
            _contextMenuTextBox.ContextMenuItemEditOSCommand.Click += MenuItemEditOSCommand_OnClick;

            _contextMenuTextBox.ContextMenuItemCopyKeySequence.Click += MenuItemCopyKeySequence_OnClick;
            _contextMenuTextBox.ContextMenuItemCopyDCSBIOS.Click += MenuItemCopyDCSBIOS_OnClick;
            _contextMenuTextBox.ContextMenuItemCopyBIPLink.Click += MenuItemCopyBIPLink_OnClick;
            _contextMenuTextBox.ContextMenuItemCopyOSCommand.Click += MenuItemCopyOSCommand_OnClick;

            _contextMenuTextBox.ContextMenuItemPaste.Click += MenuItemPaste_OnClick;

            _contextMenuTextBox.ContextMenuItemDeleteSettings.Click += MenuItemDeleteSettings_OnClick;

            TextBox.ContextMenu = _contextMenuTextBox;
            TextBox.ContextMenuOpening += TextBoxContextMenuOpening;
        }



        public void EditSingleKeyPress()
        {
            KeyPressReadingWindow keyPressReadingWindow;
            if (ContainsKeyPress())
            {
                keyPressReadingWindow = new KeyPressReadingWindow(GetKeyPress().LengthOfKeyPress, GetKeyPress().VirtualKeyCodesAsString);
            }
            else
            {
                keyPressReadingWindow = new KeyPressReadingWindow();
            }
            keyPressReadingWindow.ShowDialog();
            if (keyPressReadingWindow.DialogResult.HasValue && keyPressReadingWindow.DialogResult.Value)
            {
                //Clicked OK
                //If the user added only a single key stroke combo then let's not treat this as a sequence
                if (!keyPressReadingWindow.IsDirty)
                {
                    //User made no changes
                    return;
                }

                Clear();
                var keyPress = new KeyPress(keyPressReadingWindow.VirtualKeyCodesAsString, keyPressReadingWindow.LengthOfKeyPress);
                KeyPress = keyPress;
                KeyPress.Information = "";
                TextBox.Text = keyPressReadingWindow.VirtualKeyCodesAsString;
                UpdateKeyBindingProfileSimpleKeyStrokes();
            }
        }

        private void UpdateBIPLinkBindings()
        {
            try
            {
                _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(Key.SwitchPanelPZ55Key, BIPLink, Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ55()
        {
            try
            {
                _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(TextBox.Text, Key.SwitchPanelPZ55Key, GetKeySequence(), Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateOSCommandBindingsPZ55()
        {
            try
            {
                _switchPanelPZ55.AddOrUpdateOSCommandBinding(Key.SwitchPanelPZ55Key, OSCommandObject, Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateKeyBindingProfileSimpleKeyStrokes()
        {
            try
            {
                KeyPressLength keyPressLength;
                if (!ContainsKeyPress() || KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.ThirtyTwoMilliSec;
                }
                else
                {
                    keyPressLength = KeyPress.GetLengthOfKeyPress();
                }

                _switchPanelPZ55.AddOrUpdateSingleKeyBinding(Key.SwitchPanelPZ55Key, TextBox.Text, keyPressLength, Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateDCSBIOSBinding()
        {
            try
            {
                _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(Key.SwitchPanelPZ55Key, DCSBIOSBinding.DCSBIOSInputs, TextBox.Text, Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void MenuItemAddNullKey_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clear();
                var vkNull = Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.VK_NULL);
                if (string.IsNullOrEmpty(vkNull))
                {
                    return;
                }
                var keyPress = new KeyPress(vkNull, KeyPressLength.ThirtyTwoMilliSec);
                KeyPress = keyPress;
                KeyPress.Information = "VK_NULL";
                TextBox.Text = vkNull;
                UpdateKeyBindingProfileSimpleKeyStrokes();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemEditSequence_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EditKeySequence();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        public void EditKeySequence()
        {
            dessa  ska flyttas in i basklassen
            var keySequenceWindow = ContainsKeySequence() ? new KeySequenceWindow(TextBox.Text, GetKeySequence()) : new KeySequenceWindow();
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
                if (sequenceList.Count == 0)
                {
                    DeleteSequence();
                }
                else if (sequenceList.Count > 1)
                {
                    var keyPress = new KeyPress("Key press sequence", sequenceList);
                    KeyPress = keyPress;
                    KeyPress.Information = keySequenceWindow.GetInformation;
                    if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                    {
                        TextBox.Text = keySequenceWindow.GetInformation;
                    }
                    UpdateKeyBindingProfileSequencedKeyStrokesPZ55();
                }
                else if (sequenceList.Count == 1)
                {
                    //If only one press was created treat it as a simple keypress
                    Clear();
                    var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                    KeyPress = keyPress;
                    KeyPress.Information = keySequenceWindow.GetInformation;
                    TextBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                    UpdateKeyBindingProfileSimpleKeyStrokes();
                }
            }
        }


        private void DeleteSequence()
        {
            KeyPress.KeySequence.Clear();
            TextBox.Text = "";
            UpdateKeyBindingProfileSequencedKeyStrokesPZ55();
        }

        private void MenuItemEditDCSBIOS_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EditDCSBIOS();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void EditDCSBIOS()
        {
            DCSBIOSInputControlsWindow dcsBIOSInputControlsWindow;
            if (ContainsDCSBIOS())
            {
                dcsBIOSInputControlsWindow = new DCSBIOSInputControlsWindow(_globalHandler.GetAirframe(), TextBox.Name.Replace("TextBox", ""), _dcsbiosBindingPZ55.DCSBIOSInputs, TextBox.Text);
            }
            else
            {
                dcsBIOSInputControlsWindow = new DCSBIOSInputControlsWindow(_globalHandler.GetAirframe(), TextBox.Name.Replace("TextBox", ""), null);
            }
            dcsBIOSInputControlsWindow.ShowDialog();
            if (dcsBIOSInputControlsWindow.DialogResult.HasValue && dcsBIOSInputControlsWindow.DialogResult == true)
            {
                var dcsBiosInputs = dcsBIOSInputControlsWindow.DCSBIOSInputs;
                if (dcsBiosInputs.Count == 0)
                {
                    DeleteDCSBIOS();
                }
                else
                {
                    var text = string.IsNullOrWhiteSpace(dcsBIOSInputControlsWindow.Description) ? "DCS-BIOS" : dcsBIOSInputControlsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    TextBox.Text = text;
                    Consume(dcsBiosInputs);
                    UpdateDCSBIOSBinding();
                }
            }
        }

        private void DeleteDCSBIOS()
        {
            TextBox.Text = "";
            _switchPanelPZ55.RemoveSwitchPanelKeyFromList(ControlListPZ55.DCSBIOS, _panelUI.GetPZ55Key(_pz55TextBox).SwitchPanelPZ55Key, _panelUI.GetPZ55Key(_pz55TextBox).ButtonState);
            DCSBIOSBinding = null;
        }

        private void MenuItemEditBIP_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EditBIPLink();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void EditBIPLink()
        {
            var bipLink = ContainsBIPLink()  ? BIPLink : new BIPLinkPZ55();
            var bipLinkWindow = new BIPLinkWindow(bipLink);
            bipLinkWindow.ShowDialog();

            if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null)
            {
                var tmpBIPLink = (BIPLinkPZ55)bipLinkWindow.BIPLink;

                if (tmpBIPLink.BIPLights.Count == 0)
                {
                    DeleteBIPLink();
                }
                else
                {
                    BIPLink = tmpBIPLink;
                    UpdateBIPLinkBindings();
                }
            }
        }

        private void DeleteBIPLink()
        {
            BIPLink.BIPLights.Clear();
            TextBox.Background = Brushes.White;
            UpdateBIPLinkBindings();
        }

        private void MenuItemEditOSCommand_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EditOSCommand();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void EditOSCommand()
        {
            var osCommandWindow = ContainsOSCommand() ? new OSCommandWindow(OSCommandObject) : new OSCommandWindow();
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
                OSCommandObject = osCommand;
                TextBox.Text = osCommand.Name;
                UpdateOSCommandBindingsPZ55();
            }
        }

        private void DeleteOSCommandPZ55()
        {
            TextBox.Text = "";
            _switchPanelPZ55.RemoveSwitchPanelKeyFromList(ControlListPZ55.OSCOMMANDS, _panelUI.GetPZ55Key(_pz55TextBox).SwitchPanelPZ55Key, _panelUI.GetPZ55Key(_pz55TextBox).ButtonState);
            OSCommandObject = null;
        }

        public void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (!(TextBox.IsFocused && Equals(TextBox.Background, Brushes.Yellow)))
                {
                    //UGLY Must use this to get around problems having different color for BIPLink and Right Clicks
                    _contextMenuTextBox.HideAll();
                    return;
                }

                _contextMenuTextBox.SetVisibility(IsEmpty(),
                    ContainsSingleKey(),
                    ContainsKeySequence(),
                    ContainsDCSBIOS(),
                    ContainsBIPLink(),
                    ContainsOSCommand());

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxContextMenuIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        private void MenuItemPaste_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CopySetting(CopyContentType copyContentType)
        {
            object content = null;

            switch (copyContentType)
            {
                case CopyContentType.KeySequence:
                    {
                        content = GetKeySequence();
                        break;
                    }
                case CopyContentType.DCSBIOS:
                    {
                        content = DCSBIOSBinding;
                        break;
                    }
                case CopyContentType.BIPLink:
                    {
                        content = BIPLink;
                        break;
                    }
                case CopyContentType.OSCommand:
                    {
                        content = OSCommandObject;
                        break;
                    }
            }

            if (content != null)
            {
                var copyPackage = new CopyPackage();
                copyPackage.ContentType = copyContentType;
                copyPackage.Content = content;
                copyPackage.SourceName = TextBox.Name;
                Clipboard.SetDataObject(copyPackage);
            }
        }

        private void MenuItemCopyKeySequence_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopySetting(CopyContentType.KeySequence);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemCopyDCSBIOS_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopySetting(CopyContentType.DCSBIOS);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemCopyBIPLink_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopySetting(CopyContentType.BIPLink);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemCopyOSCommand_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopySetting(CopyContentType.OSCommand);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemDeleteSettings_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteSequence();
                DeleteDCSBIOS();
                DeleteBIPLink();
                DeleteOSCommandPZ55();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ55 != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ55 != null && _bipLinkPZ55.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ55 == null || _bipLinkPZ55.BIPLights.Count == 0) && (_dcsbiosBindingPZ55?.DCSBIOSInputs == null || _dcsbiosBindingPZ55.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeySequence.Count == 0);
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ55 == null)
            {
                _dcsbiosBindingPZ55 = new DCSBIOSActionBindingPZ55();
            }
            _dcsbiosBindingPZ55.DCSBIOSInputs = dcsBiosInputs;
        }

        public DCSBIOSActionBindingPZ55 DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ55;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingPZ55 = value;
                if (_dcsbiosBindingPZ55 != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingPZ55.Description))
                    {
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingPZ55.Description;
                    }
                }
                else
                {
                    TextBox.Text = "";
                }
            }
        }

        public BIPLinkPZ55 BIPLink
        {
            get => _bipLinkPZ55;
            set
            {
                _bipLinkPZ55 = value;
                if (_bipLinkPZ55 != null)
                {
                    TextBox.Background = Brushes.Bisque;
                }
                else
                {
                    TextBox.Background = Brushes.White;
                }
            }
        }

        public SwitchPanelPZ55KeyOnOff Key { get; set; }


        public override void Clear()
        {
            _dcsbiosBindingPZ55 = null;
            _bipLinkPZ55 = null;
            KeyPress = null;
            OSCommandObject = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
