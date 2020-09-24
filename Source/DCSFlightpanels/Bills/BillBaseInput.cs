using System;
using System.Collections.Generic;
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
    public abstract class BillBaseInput
    {
        private KeyPress _keyPress;
        private OSCommand _osCommand;
        private IGlobalHandler _globalHandler;
        private IPanelUI _panelUI;
        private TextBox _textBox;
        private readonly SaitekPanel _saitekPanel;
        private ContextMenuPanelTextBox _contextMenu;


        public abstract bool ContainsDCSBIOS();
        public abstract bool ContainsBIPLink();
        public abstract bool IsEmpty();
        public abstract bool IsEmptyNoCareBipLink();
        public abstract void Consume(List<DCSBIOSInput> dcsBiosInputs);
        public abstract void ClearAll();
        protected abstract void ClearDCSBIOSFromBill();



        protected BillBaseInput(IGlobalHandler globalHandler, TextBox textBox, IPanelUI panelUI, SaitekPanel saitekPanel)
        {
            _globalHandler = globalHandler;
            _textBox = textBox;
            _panelUI = panelUI;
            _saitekPanel = saitekPanel;
        }

        public abstract BIPLink BipLink
        {
            get;
            set;
        }

        public abstract List<DCSBIOSInput> DCSBIOSInputs
        {
            get;
            set;
        }

        public abstract DCSBIOSActionBindingBase DCSBIOSBinding
        {
            get;
            set;
        }

        private void CopySetting(CopyContentType copyContentType)
        {
            object content = null;
            string description = null;

            switch (copyContentType)
            {
                case CopyContentType.KeyStroke:
                {
                    description = "";
                    content = GetKeyPress();
                    break;
                }
                case CopyContentType.KeySequence:
                {
                    description = GetKeySequenceDescription();
                    content = GetKeySequence();
                    break;
                }
                case CopyContentType.DCSBIOS:
                {
                    description = DCSBIOSBinding.Description;
                    content = DCSBIOSBinding.DCSBIOSInputs;
                    break;
                }
                case CopyContentType.BIPLink:
                {
                    description = BipLink.Description;
                    content = BipLink;
                    break;
                }
                case CopyContentType.OSCommand:
                {
                    description = OSCommand.Command;
                    content = OSCommandObject;
                    break;
                }
            }

            if (content != null)
            {
                var copyPackage = new CopyPackage { ContentType = copyContentType, Content = content, SourceName = TextBox.Name };
                copyPackage.Description = description;
                Clipboard.SetDataObject(copyPackage);
            }
        }

        public void Paste()
        {
            var iDataObject = Clipboard.GetDataObject();
            if (iDataObject == null || !iDataObject.GetDataPresent("NonVisuals.CopyPackage"))
            {
                return;
            }

            var copyPackage = (CopyPackage)iDataObject.GetData("NonVisuals.CopyPackage");
            if (copyPackage?.Content == null || copyPackage.SourceName == TextBox.Name)
            {
                return;
            }

            switch (copyPackage.ContentType)
            {
                case CopyContentType.KeyStroke:
                {
                    if (IsEmptyNoCareBipLink())
                    {
                        AddKeyStroke((KeyPressInfo) copyPackage.Content);
                    }
                    break;
                }
                case CopyContentType.KeySequence:
                    {
                        if (IsEmptyNoCareBipLink())
                        {
                            AddKeySequence(copyPackage.Description, (SortedList<int, KeyPressInfo>)copyPackage.Content);
                        }
                        break;
                    }
                case CopyContentType.DCSBIOS:
                    {
                        if (IsEmptyNoCareBipLink())
                        {
                            AddDCSBIOS(copyPackage.Description, (List<DCSBIOSInput>)copyPackage.Content);
                        }
                        break;
                    }
                case CopyContentType.BIPLink:
                    {
                        if (!ContainsBIPLink())
                        {
                            AddBipLink((BIPLink) copyPackage.Content);
                        }
                        break;
                    }
                case CopyContentType.OSCommand:
                    {
                        if (IsEmptyNoCareBipLink())
                        {
                            AddOSCommand((OSCommand) copyPackage.Content);
                        }
                        break;
                    }
            }

        }

        protected void SetContextMenu()
        {
            _contextMenu = new ContextMenuPanelTextBox(Common.IsOperationModeFlagSet(EmulationMode.KeyboardEmulationOnly));

            _contextMenu.ContextMenuItemAddNullKey.Click += MenuItemAddNullKey_OnClick;
            _contextMenu.ContextMenuItemEditSequence.Click += MenuItemEditSequence_OnClick;
            _contextMenu.ContextMenuItemEditDCSBIOS.Click += MenuItemEditDCSBIOS_OnClick;
            _contextMenu.ContextMenuItemEditBIP.Click += MenuItemEditBIP_OnClick;
            _contextMenu.ContextMenuItemEditOSCommand.Click += MenuItemEditOSCommand_OnClick;

            _contextMenu.ContextMenuItemCopyKeyStroke.Click += MenuItemCopyKeyStroke_OnClick;
            _contextMenu.ContextMenuItemCopyKeySequence.Click += MenuItemCopyKeySequence_OnClick;
            _contextMenu.ContextMenuItemCopyDCSBIOS.Click += MenuItemCopyDCSBIOS_OnClick;
            _contextMenu.ContextMenuItemCopyBIPLink.Click += MenuItemCopyBIPLink_OnClick;
            _contextMenu.ContextMenuItemCopyOSCommand.Click += MenuItemCopyOSCommand_OnClick;

            _contextMenu.ContextMenuItemPaste.Click += MenuItemPaste_OnClick;

            _contextMenu.ContextMenuItemDeleteSettings.Click += MenuItemDeleteSettings_OnClick;
            _contextMenu.TextBox = TextBox;
            TextBox.ContextMenu = _contextMenu;
            TextBox.ContextMenuOpening += TextBoxContextMenuOpening;
        }

        private void MenuItemAddNullKey_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddVKNULL();
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
        
        private void MenuItemEditBIP_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EditBIPLink(GamingPanelEnum.PZ55SwitchPanel);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
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



        public void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (!(TextBox.IsFocused && Equals(TextBox.Background, Brushes.Yellow)))
                {
                    //UGLY Must use this to get around problems having different color for BIPLink and Right Clicks
                    _contextMenu.HideAll();
                    return;
                }

                _contextMenu.SetVisibility(IsEmpty(),
                    ContainsKeyStroke(),
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

        private void MenuItemPaste_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Paste();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemCopyKeyStroke_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopySetting(CopyContentType.KeyStroke);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                DeleteKeyStroke();
                DeleteSequence();
                DeleteDCSBIOS();
                DeleteBIPLink();
                DeleteOSCommand();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        public void AddVKNULL()
        {
            ClearAll();
            var vkNull = Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.VK_NULL);
            if (string.IsNullOrEmpty(vkNull))
            {
                return;
            }
            var keyPress = new KeyPress(vkNull, KeyPressLength.ThirtyTwoMilliSec);
            KeyPress = keyPress;
            KeyPress.Description = "VK_NULL";
            TextBox.Text = vkNull;
            UpdateKeyBindingKeyStroke();
        }

        public void EditKeyStroke()
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

                if (string.IsNullOrEmpty(keyPressReadingWindow.VirtualKeyCodesAsString))
                {
                    KeyPress = null;
                    TextBox.Text = "";
                }
                else
                {
                    var keyPress = new KeyPress(keyPressReadingWindow.VirtualKeyCodesAsString, keyPressReadingWindow.LengthOfKeyPress);
                    KeyPress = keyPress;
                    KeyPress.Description = "";
                    TextBox.Text = keyPressReadingWindow.VirtualKeyCodesAsString;
                }
                UpdateKeyBindingKeyStroke();
            }
        }

        public void AddKeyStroke(KeyPressInfo keyStroke)
        {
            var keyPress = new KeyPress();
            keyPress.KeySequence.Add(0, keyStroke);
            keyPress.Description = "";
            KeyPress = keyPress;
            TextBox.Text = keyStroke.VirtualKeyCodesAsString;
            UpdateKeyBindingKeyStroke();
        }

        public void AddKeySequence(string description, SortedList<int, KeyPressInfo> keySequence)
        {

            var keyPress = new KeyPress("Key stroke sequence", keySequence);
            KeyPress = keyPress;
            KeyPress.Description = description;
            if (!string.IsNullOrEmpty(description))
            {
                TextBox.Text = description;
            }
            UpdateKeyBindingSequencedKeyStrokes();
        }

        public void EditKeySequence()
        {
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
                var sequenceList = keySequenceWindow.KeySequence;
                if (sequenceList.Count == 0)
                {
                    DeleteSequence();
                }
                else if (sequenceList.Count > 1)
                {
                    AddKeySequence(keySequenceWindow.Description, keySequenceWindow.KeySequence);
                }
                else if (sequenceList.Count == 1)
                {
                    //If only one press was created treat it as a simple keypress
                    var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                    KeyPress = keyPress;
                    KeyPress.Description = keySequenceWindow.Description;
                    TextBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                    UpdateKeyBindingKeyStroke();
                }
            }
        }

        public void AddDCSBIOS(string description, List<DCSBIOSInput> dcsBiosInputs)
        {

            var text = string.IsNullOrWhiteSpace(description) ? "DCS-BIOS" : description;
            //1 appropriate text to textbox
            //2 update bindings
            TextBox.Text = text;
            Consume(dcsBiosInputs);
            UpdateDCSBIOSBinding();
        }

        public void EditDCSBIOS()
        {
            DCSBIOSInputControlsWindow dcsBIOSInputControlsWindow;
            if (ContainsDCSBIOS())
            {
                dcsBIOSInputControlsWindow = new DCSBIOSInputControlsWindow(_globalHandler.GetAirframe(), TextBox.Name.Replace("TextBox", ""), DCSBIOSInputs, TextBox.Text);
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
                    AddDCSBIOS(dcsBIOSInputControlsWindow.Description, dcsBiosInputs);
                }
            }
        }

        public void AddBipLink(BIPLink bipLink)
        {
            BipLink = bipLink;
            UpdateBIPLinkBindings();
        }

        public void EditBIPLink(GamingPanelEnum panelType)
        {
            BIPLinkWindow bipLinkWindow;
            switch (panelType)
            {
                case GamingPanelEnum.PZ55SwitchPanel:
                    {
                        var bipLink = ContainsBIPLink() ? BipLink : new BIPLinkPZ55();
                        bipLinkWindow = new BIPLinkWindow(bipLink);
                        break;
                    }
                case GamingPanelEnum.PZ70MultiPanel:
                    {
                        var bipLink = ContainsBIPLink() ? BipLink : new BIPLinkPZ70();
                        bipLinkWindow = new BIPLinkWindow(bipLink);
                        break;
                    }
                case GamingPanelEnum.PZ69RadioPanel:
                    {
                        var bipLink = ContainsBIPLink() ? BipLink : new BIPLinkPZ69();
                        bipLinkWindow = new BIPLinkWindow(bipLink);
                        break;
                    }
                case GamingPanelEnum.TPM:
                    {
                        var bipLink = ContainsBIPLink() ? BipLink : new BIPLinkTPM();
                        bipLinkWindow = new BIPLinkWindow(bipLink);
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
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
                    AddBipLink(tmpBIPLink);
                }
            }
        }

        public void AddOSCommand(OSCommand osCommand)
        {
            OSCommandObject = osCommand;
            TextBox.Text = osCommand.Name;
            UpdateOSCommandBindings();
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
                AddOSCommand(osCommandWindow.OSCommandObject);
            }
        }


        public void UpdateKeyBindingKeyStroke()
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

                _saitekPanel.AddOrUpdateKeyStrokeBinding(_panelUI.GetSwitch(TextBox), TextBox.Text, keyPressLength);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void UpdateKeyBindingSequencedKeyStrokes()
        {
            try
            {
                if (_keyPress != null)
                {
                    _saitekPanel.AddOrUpdateSequencedKeyBinding(_panelUI.GetSwitch(TextBox), TextBox.Text, _keyPress.KeySequence);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void UpdateDCSBIOSBinding()
        {
            try
            {
                _saitekPanel.AddOrUpdateDCSBIOSBinding(_panelUI.GetSwitch(TextBox), DCSBIOSInputs, TextBox.Text);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void UpdateBIPLinkBindings()
        {
            try
            {
                if (BipLink != null)
                {
                    _saitekPanel.AddOrUpdateBIPLinkBinding(_panelUI.GetSwitch(TextBox), BipLink);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void UpdateOSCommandBindings()
        {
            try
            {
                _saitekPanel.AddOrUpdateOSCommandBinding(_panelUI.GetSwitch(TextBox), OSCommandObject);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DeleteKeyStroke()
        {
            KeyPress?.KeySequence?.Clear();
            TextBox.Text = "";
            UpdateKeyBindingSequencedKeyStrokes();
        }

        public void DeleteSequence()
        {
            KeyPress?.KeySequence?.Clear();
            TextBox.Text = "";
            UpdateKeyBindingSequencedKeyStrokes();
        }

        public void DeleteDCSBIOS()
        {
            TextBox.Text = "";
            _saitekPanel.RemoveSwitchFromList(ControlListPZ55.DCSBIOS, _panelUI.GetSwitch(TextBox));
            ClearDCSBIOSFromBill();
        }

        public void DeleteBIPLink()
        {
            BipLink?.BIPLights?.Clear();
            TextBox.Background = Brushes.White;
            UpdateBIPLinkBindings();
        }

        public void DeleteOSCommand()
        {
            TextBox.Text = "";
            _saitekPanel.RemoveSwitchFromList(ControlListPZ55.OSCOMMANDS, _panelUI.GetSwitch(_textBox));
            OSCommandObject = null;
        }

        public bool ContainsOSCommand()
        {
            return _osCommand != null;
        }

        public bool ContainsKeyPress()
        {
            return _keyPress != null && _keyPress.KeySequence.Count > 0;
        }

        public bool ContainsKeySequence()
        {
            return _keyPress != null && _keyPress.IsMultiSequenced();
        }

        public bool ContainsKeyStroke()
        {
            return _keyPress != null && !_keyPress.IsMultiSequenced() && _keyPress.KeySequence.Count > 0;
        }

        public KeyPress KeyPress
        {
            get => _keyPress;
            set
            {
                if (value != null && ContainsDCSBIOS())
                {
                    throw new Exception("Cannot insert KeyPress, Bill already contains DCSBIOSInputs");
                }
                _keyPress = value;
                _textBox.Text = _keyPress != null ? _keyPress.GetKeyPressInformation() : "";
            }
        }

        public KeyPressInfo GetKeyPress()
        {
            return _keyPress.KeySequence[0];
        }

        public string GetKeySequenceDescription()
        {
            return _keyPress.Description;
        }

        public SortedList<int, KeyPressInfo> GetKeySequence()
        {
            return _keyPress.KeySequence;
        }

        public OSCommand OSCommandObject
        {
            get => _osCommand;
            set
            {
                _osCommand = value;
                _textBox.Text = _osCommand != null ? _osCommand.Name : "";
            }
        }

        public TextBox TextBox
        {
            get => _textBox;
            set => _textBox = value;
        }

        public OSCommand OSCommand
        {
            get => _osCommand;
            set => _osCommand = value;
        }

        public IGlobalHandler GlobalHandler
        {
            get => _globalHandler;
            set => _globalHandler = value;
        }

        public IPanelUI PanelUIParent
        {
            get => _panelUI;
            set => _panelUI = value;
        }
    }

}
