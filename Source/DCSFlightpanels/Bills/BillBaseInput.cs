using System.Linq;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.BindingClasses.DCSBIOSBindings;

namespace DCSFlightpanels.Bills
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using Interfaces;
    using Windows;


    using MEF;

    using NonVisuals;
    using NonVisuals.Panels.Saitek.Panels;


    /*
     * A Bill(Input) is attached to each UI TextBox that is used for adding configuration
     * to a panel switch/knob/key.
     * A TextBox by itself can't hold complex information as such.
     * Each UI Panel Control (Switch/Multi/etc) has its own TextBox type which holds the specific Bill class.
     *
     * The Bill *communicates directly* to the Saitek Panel class and updates the settings whenever they are changed.
     *
     * The purpose of the Bill is to :
     * 1) Hold the setting(s) the user has specified
     * 2) Provide a contextmenu which enables the user to edit/copy/paste/delete settings
     *    held by the specific TextBox
     * 3) Handle double clicking
     *
     * Each panel type has its own different classes (each panel has unique switches/buttons) for binding a switch / key to a setting.
     * Using this solution it is possible to for example copy settings from one panel to another.
     * It is also easy to create the UI infrastructure for a new panel when needed.
     */
    public abstract class BillBaseInput
    {
        private readonly SaitekPanel _saitekPanel;
        private KeyPress _keyPress;
        private ContextMenuPanelTextBox _contextMenu;

        public abstract bool ContainsDCSBIOS();
        public abstract bool ContainsBIPLink();
        public abstract bool IsEmpty();
        protected abstract bool IsEmptyNoCareBipLink();
        protected abstract void Consume(List<DCSBIOSInput> dcsBiosInputs, bool isSequenced);
        public abstract void ClearAll();
        protected abstract void ClearDCSBIOSFromBill();
        public abstract BIPLinkBase BipLink { get; set; }
        protected abstract List<DCSBIOSInput> DCSBIOSInputs { get; }
        public abstract DCSBIOSActionBindingBase DCSBIOSBinding { get; set; }

        protected TextBox TextBox { get; init; }
        private OSCommand OSCommand { get; set; }
        public IPanelUI PanelUIParent { get; init; }

        public OSCommand OSCommandObject
        {
            get => OSCommand;
            set
            {
                OSCommand = value;
                TextBox.Text = OSCommand != null ? OSCommand.Name : string.Empty;
            }
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
                TextBox.Text = _keyPress != null ? _keyPress.GetKeyPressInformation() : string.Empty;
            }
        }

        protected void SetTextBoxText(DCSBIOSActionBindingBase dcsbiosActionBindingBase)
        {
            if (dcsbiosActionBindingBase != null)
            {
                if (!string.IsNullOrEmpty(dcsbiosActionBindingBase.Description))
                {
                    TextBox.Text = dcsbiosActionBindingBase.Description;
                }
                else if (dcsbiosActionBindingBase.DCSBIOSInputs.Any())
                {
                    TextBox.Text = dcsbiosActionBindingBase.DCSBIOSInputs[0].ControlId;
                }
                else
                {
                    TextBox.Text = "DCS-BIOS";
                }
            }
            else
            {
                TextBox.Text = string.Empty;
            }
        }

        private void SetTextBoxText(string description, List<DCSBIOSInput> dcsBiosInputs)
        {
            if (dcsBiosInputs != null)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    TextBox.Text = description;
                }
                else if (dcsBiosInputs.Any())
                {
                    TextBox.Text = dcsBiosInputs[0].ControlId;
                }
                else
                {
                    TextBox.Text = "DCS-BIOS";
                }
            }
            else
            {
                TextBox.Text = string.Empty;
            }
        }

        protected BillBaseInput(TextBox textBox, IPanelUI panelUI, SaitekPanel saitekPanel)
        {
            TextBox = textBox;
            PanelUIParent = panelUI;
            _saitekPanel = saitekPanel;
        }

        private void CopySetting(CopyContentType copyContentType)
        {
            object content = null;
            string description = null;

            switch (copyContentType)
            {
                case CopyContentType.KeyStroke:
                    {
                        description = string.Empty;
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
                        content = DCSBIOSBinding;
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
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null || !dataObject.GetDataPresent("NonVisuals.CopyPackage"))
            {
                return;
            }

            var copyPackage = (CopyPackage)dataObject.GetData("NonVisuals.CopyPackage");
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
                            AddKeyStroke((KeyPressInfo)copyPackage.Content);
                        }
                        break;
                    }

                case CopyContentType.KeySequence:
                    {
                        if (IsEmptyNoCareBipLink())
                        {
                            AddKeySequence(copyPackage.Description, (SortedList<int, IKeyPressInfo>)copyPackage.Content);
                        }
                        break;
                    }

                case CopyContentType.DCSBIOS:
                    {
                        if (IsEmptyNoCareBipLink())
                        {
                            var dcsbiosActionBindingBase = (DCSBIOSActionBindingBase)copyPackage.Content;
                            AddDCSBIOS(copyPackage.Description, dcsbiosActionBindingBase.DCSBIOSInputs, dcsbiosActionBindingBase.IsSequenced);
                        }
                        break;
                    }

                case CopyContentType.BIPLink:
                    {
                        if (!ContainsBIPLink())
                        {
                            AddBipLink((BIPLinkBase)copyPackage.Content);
                        }
                        break;
                    }

                case CopyContentType.OSCommand:
                    {
                        if (IsEmptyNoCareBipLink())
                        {
                            AddOSCommand((OSCommand)copyPackage.Content);
                        }
                        break;
                    }
            }
        }

        protected void SetContextMenu()
        {
            _contextMenu = new ContextMenuPanelTextBox(Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly));

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
                EditBIPLink(_saitekPanel.TypeOfPanel);
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

        private void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (!(TextBox.IsFocused && Equals(TextBox.Background, Brushes.Yellow)))
                {
                    // UGLY Must use this to get around problems having different color for BIPLink and Right Clicks
                    _contextMenu.HideAll();
                    return;
                }

                /*
                 * User convenience!
                 * The deal with this objectCount is this; a textbox can have multiple configurations and when a user copies
                 * from one of these the user will see a submenu to copy where the items are listed and he can choose which to copy.
                 * Now, mostly there are only one item and for those there will not be a copy menu with subitems, just a "Copy" menu.
                 */
                var objectCount = _contextMenu.SetVisibility(
                    IsEmpty(),
                    ContainsKeyStroke(),
                    ContainsKeySequence(),
                    ContainsDCSBIOS(),
                    ContainsBIPLink(),
                    ContainsOSCommand());

                if (objectCount == 1)
                {
                    //Show only simple copy menu and set correct event handler.
                    if (ContainsKeyStroke())
                    {
                        _contextMenu.ContextMenuItemCopySingle.Click += MenuItemCopyKeyStroke_OnClick;
                    }
                    else if (ContainsKeySequence())
                    {
                        _contextMenu.ContextMenuItemCopySingle.Click += MenuItemCopyKeySequence_OnClick;
                    }
                    else if (ContainsDCSBIOS())
                    {
                        _contextMenu.ContextMenuItemCopySingle.Click += MenuItemCopyDCSBIOS_OnClick;
                    }
                    else if (ContainsBIPLink())
                    {
                        _contextMenu.ContextMenuItemCopySingle.Click += MenuItemCopyBIPLink_OnClick;
                    }
                    else if (ContainsOSCommand())
                    {
                        _contextMenu.ContextMenuItemCopySingle.Click += MenuItemCopyOSCommand_OnClick;
                    }
                }
                
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

        private void AddVKNULL()
        {
            ClearAll();
            var virtualKeyNull = Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.VK_NULL);
            if (string.IsNullOrEmpty(virtualKeyNull))
            {
                return;
            }

            KeyPress = new KeyPress(virtualKeyNull, KeyPressLength.ThirtyTwoMilliSec)
            {
                Description = "VK_NULL"
            };
            TextBox.Text = virtualKeyNull;
            UpdateKeyBindingKeyStroke();
        }

        public void EditKeyStroke()
        {
            var supportIndefinite = _saitekPanel.TypeOfPanel != GamingPanelEnum.FarmingPanel;

            KeyPressReadingSmallWindow keyPressReadingWindow;
            if (ContainsKeyPress())
            {
                keyPressReadingWindow = new KeyPressReadingSmallWindow(GetKeyPress().LengthOfKeyPress, GetKeyPress().VirtualKeyCodesAsString, supportIndefinite);
            }
            else
            {
                keyPressReadingWindow = new KeyPressReadingSmallWindow(supportIndefinite);
            }

            keyPressReadingWindow.ShowDialog();
            if (keyPressReadingWindow.DialogResult.HasValue && keyPressReadingWindow.DialogResult.Value)
            {
                // Clicked OK
                // If the user added only a single key stroke combo then let's not treat this as a sequence
                if (!keyPressReadingWindow.IsDirty)
                {
                    // User made no changes
                    return;
                }

                if (string.IsNullOrEmpty(keyPressReadingWindow.VirtualKeyCodesAsString))
                {
                    KeyPress = null;
                    TextBox.Text = string.Empty;
                }
                else
                {
                    KeyPress = new KeyPress(keyPressReadingWindow.VirtualKeyCodesAsString, keyPressReadingWindow.LengthOfKeyPress)
                        {
                            Description = string.Empty
                        };
                    TextBox.Text = keyPressReadingWindow.VirtualKeyCodesAsString;
                }
                UpdateKeyBindingKeyStroke();
            }
        }

        private void AddKeyStroke(KeyPressInfo keyStroke)
        {
            var keyPress = new KeyPress();
            keyPress.KeyPressSequence.Add(0, keyStroke.CloneJson());
            keyPress.Description = string.Empty;
            KeyPress = keyPress;
            TextBox.Text = keyStroke.VirtualKeyCodesAsString;
            UpdateKeyBindingKeyStroke();
        }

        private void AddKeySequence(string description, SortedList<int, IKeyPressInfo> keySequence)
        {
            KeyPress = new KeyPress("Key stroke sequence", keySequence.CloneJson())
            {
                Description = description
            };
            if (!string.IsNullOrEmpty(description))
            {
                TextBox.Text = description;
            }
            UpdateKeyBindingSequencedKeyStrokes();
        }

        public void EditKeySequence()
        {
            var supportIndefinite = _saitekPanel.TypeOfPanel != GamingPanelEnum.FarmingPanel;
            var keySequenceWindow = ContainsKeySequence() ? new KeySequenceWindow(TextBox.Text, GetKeySequence(), supportIndefinite) : new KeySequenceWindow(supportIndefinite);
            keySequenceWindow.ShowDialog();

            if (keySequenceWindow.DialogResult.HasValue && keySequenceWindow.DialogResult.Value)
            {
                // Clicked OK
                // If the user added only a single key stroke combo then let's not treat this as a sequence
                if (!keySequenceWindow.IsDirty)
                {
                    // User made no changes
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
                    // If only one press was created treat it as a simple keypress
                    KeyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress)
                        {
                            Description = keySequenceWindow.Description
                        };
                    TextBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                    UpdateKeyBindingKeyStroke();
                }
            }
        }

        private void AddDCSBIOS(string description, List<DCSBIOSInput> dcsBiosInputs, bool isSequenced)
        {
            // 1 appropriate text to textbox
            // 2 update bindings
            SetTextBoxText(description, dcsBiosInputs.CloneJson());
            Consume(dcsBiosInputs.CloneJson(), isSequenced);
            UpdateDCSBIOSBinding();
        }

        public void EditDCSBIOS()
        {
            DCSBIOSInputControlsWindow dcsBIOSInputControlsWindow;
            if (ContainsDCSBIOS())
            {
                dcsBIOSInputControlsWindow = new DCSBIOSInputControlsWindow(TextBox.Name.Replace("TextBox", string.Empty), DCSBIOSInputs, TextBox.Text,   DCSBIOSBinding.IsSequenced,  true);
            }
            else
            {
                dcsBIOSInputControlsWindow = new DCSBIOSInputControlsWindow( TextBox.Name.Replace("TextBox", string.Empty), null, false, true);
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
                    AddDCSBIOS(dcsBIOSInputControlsWindow.Description, dcsBiosInputs, dcsBIOSInputControlsWindow.IsSequenced);
                }
            }
        }

        private void AddBipLink(BIPLinkBase bipLink)
        {
            //Don't know how to get around this. Json can't clone an abstract class.
            if (bipLink.GetType() == typeof(BIPLinkPZ55))
            {
                BipLink = ((BIPLinkPZ55)bipLink).CloneJson();
            }
            else if (bipLink.GetType() == typeof(BIPLinkPZ70))
            {
                BipLink = ((BIPLinkPZ70)bipLink).CloneJson();
            }
            else if (bipLink.GetType() == typeof(BIPLinkPZ69))
            {
                BipLink = ((BIPLinkPZ69)bipLink).CloneJson();
            }
            else if (bipLink.GetType() == typeof(BIPLinkFarmingPanel))
            {
                BipLink = ((BIPLinkFarmingPanel)bipLink).CloneJson();
            }
            else if (bipLink.GetType() == typeof(BIPLinkTPM))
            {
                BipLink = ((BIPLinkTPM)bipLink).CloneJson();
            }
            UpdateBIPLinkBindings();
        }

        private void EditBIPLink(GamingPanelEnum panelType)
        {
            BIPLinkWindow bipLinkWindow;
            switch (panelType)
            {
                case GamingPanelEnum.FarmingPanel:
                    {
                        var bipLink = ContainsBIPLink() ? BipLink : new BIPLinkFarmingPanel();
                        bipLinkWindow = new BIPLinkWindow(bipLink);
                        break;
                    }
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
                var tmpBIPLink = (BIPLinkBase)bipLinkWindow.BIPLink;

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

        private void AddOSCommand(OSCommand operatingSystemCommand)
        {
            OSCommandObject = operatingSystemCommand.CloneJson();
            TextBox.Text = operatingSystemCommand.Name;
            UpdateOSCommandBindings();
        }

        public void EditOSCommand()
        {
            var operatingSystemCommandWindow = ContainsOSCommand() ? new OSCommandWindow(OSCommandObject) : new OSCommandWindow();
            operatingSystemCommandWindow.ShowDialog();
            if (operatingSystemCommandWindow.DialogResult.HasValue && operatingSystemCommandWindow.DialogResult.Value)
            {
                // Clicked OK
                if (!operatingSystemCommandWindow.IsDirty)
                {
                    // User made no changes
                    return;
                }

                AddOSCommand(operatingSystemCommandWindow.OSCommandObject);
            }
        }

        private void UpdateKeyBindingKeyStroke()
        {
            try
            {
                KeyPressLength keyPressLength;
                if (!ContainsKeyPress() || KeyPress.KeyPressSequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.ThirtyTwoMilliSec;
                }
                else
                {
                    keyPressLength = KeyPress.GetLengthOfKeyPress();
                }

                _saitekPanel.AddOrUpdateKeyStrokeBinding(PanelUIParent.GetSwitch(TextBox), TextBox.Text, keyPressLength);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateKeyBindingSequencedKeyStrokes()
        {
            try
            {
                if (_keyPress != null)
                {
                    _saitekPanel.AddOrUpdateSequencedKeyBinding(PanelUIParent.GetSwitch(TextBox), TextBox.Text, _keyPress.KeyPressSequence);
                }
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
                _saitekPanel.AddOrUpdateDCSBIOSBinding(PanelUIParent.GetSwitch(TextBox), DCSBIOSInputs, TextBox.Text, DCSBIOSBinding.IsSequenced);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateBIPLinkBindings()
        {
            try
            {
                if (BipLink != null)
                {
                    _saitekPanel.AddOrUpdateBIPLinkBinding(PanelUIParent.GetSwitch(TextBox), BipLink);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateOSCommandBindings()
        {
            try
            {
                _saitekPanel.AddOrUpdateOSCommandBinding(PanelUIParent.GetSwitch(TextBox), OSCommandObject);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DeleteKeyStroke()
        {
            KeyPress?.KeyPressSequence?.Clear();
            TextBox.Text = string.Empty;
            UpdateKeyBindingSequencedKeyStrokes();
        }

        private void DeleteSequence()
        {
            KeyPress?.KeyPressSequence?.Clear();
            TextBox.Text = string.Empty;
            UpdateKeyBindingSequencedKeyStrokes();
        }

        private void DeleteDCSBIOS()
        {
            TextBox.Text = string.Empty;
            _saitekPanel.RemoveSwitchFromList(ControlList.DCSBIOS, PanelUIParent.GetSwitch(TextBox));
            ClearDCSBIOSFromBill();
        }

        private void DeleteBIPLink()
        {
            BipLink?.BIPLights?.Clear();
            if (!TextBox.IsFocused && TextBox.Background != Brushes.Yellow)
            {
                TextBox.Background = Brushes.White;
            }
            UpdateBIPLinkBindings();
        }

        private void DeleteOSCommand()
        {
            TextBox.Text = string.Empty;
            _saitekPanel.RemoveSwitchFromList(ControlList.OSCOMMANDS, PanelUIParent.GetSwitch(TextBox));
            OSCommandObject = null;
        }

        public bool ContainsOSCommand()
        {
            return OSCommand != null;
        }

        public bool ContainsKeyPress()
        {
            return _keyPress != null && _keyPress.KeyPressSequence.Count > 0;
        }

        public bool ContainsKeySequence()
        {
            return _keyPress != null && _keyPress.IsMultiSequenced();
        }

        public bool ContainsKeyStroke()
        {
            return _keyPress != null && !_keyPress.IsMultiSequenced() && _keyPress.KeyPressSequence.Count > 0;
        }

        private IKeyPressInfo GetKeyPress()
        {
            return _keyPress.KeyPressSequence[0];
        }

        private string GetKeySequenceDescription()
        {
            return _keyPress.Description;
        }

        public SortedList<int, IKeyPressInfo> GetKeySequence()
        {
            return _keyPress.KeyPressSequence;
        }
    }
}
