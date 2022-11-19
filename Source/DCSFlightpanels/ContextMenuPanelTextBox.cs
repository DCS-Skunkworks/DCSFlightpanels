namespace DCSFlightpanels
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    using ClassLibraryCommon;

    using NonVisuals;
    using NonVisuals.Panels.Saitek;


    public class ContextMenuPanelTextBox : ContextMenu
    {
        private readonly bool _keyboardEmulationOnly;
        private MenuItem _contextMenuItemAddNullKey;
        private MenuItem _contextMenuItemEditSequence;
        private MenuItem _contextMenuItemEditDCSBIOS;
        private MenuItem _contextMenuItemEditBIP;
        private MenuItem _contextMenuItemEditOSCommand;
        private MenuItem _contextMenuItemCopySingle;
        private MenuItem _contextMenuItemCopySubMenu;
        private MenuItem _contextMenuItemCopyKeyStroke;
        private MenuItem _contextMenuItemCopyKeySequence;
        private MenuItem _contextMenuItemCopyDCSBIOS;
        private MenuItem _contextMenuItemCopyBIPLink;
        private MenuItem _contextMenuItemCopyOSCommand;
        private MenuItem _contextMenuItemPaste;
        private MenuItem _contextMenuItemDeleteSettings;
        
        private TextBox _textBox;
        
        public ContextMenuPanelTextBox(bool keyboardEmulationOnly)
        {
            _keyboardEmulationOnly = keyboardEmulationOnly;
            
            _contextMenuItemAddNullKey = new MenuItem() { Header = "Add VK_NULL key" };
            Items.Add(_contextMenuItemAddNullKey);
            _contextMenuItemEditSequence = new MenuItem() { Header = "Edit Key Sequence" };
            Items.Add(_contextMenuItemEditSequence);

            _contextMenuItemEditDCSBIOS = new MenuItem() { Header = "Edit DCS-BIOS Control" };
            Items.Add(_contextMenuItemEditDCSBIOS);
            _contextMenuItemEditBIP = new MenuItem() { Header = "Edit B.I.P." };
            Items.Add(_contextMenuItemEditBIP);
            _contextMenuItemEditOSCommand = new MenuItem() { Header = "Edit OS Command" };
            Items.Add(_contextMenuItemEditOSCommand);

            _contextMenuItemCopySingle = new MenuItem() { Header = "Copy" };
            Items.Add(_contextMenuItemCopySingle);

            _contextMenuItemCopySubMenu = new MenuItem() { Header = "Copy" };
            Items.Add(_contextMenuItemCopySubMenu);
            _contextMenuItemCopyKeyStroke = new MenuItem() { Header = "Key Stroke" };
            _contextMenuItemCopySubMenu.Items.Add(_contextMenuItemCopyKeyStroke);
            _contextMenuItemCopyKeySequence = new MenuItem() { Header = "Key Sequence" };
            _contextMenuItemCopySubMenu.Items.Add(_contextMenuItemCopyKeySequence);
            _contextMenuItemCopyDCSBIOS = new MenuItem() { Header = "DCS-BIOS" };
            _contextMenuItemCopySubMenu.Items.Add(_contextMenuItemCopyDCSBIOS);
            _contextMenuItemCopyBIPLink = new MenuItem() { Header = "BIP Link" };
            _contextMenuItemCopySubMenu.Items.Add(_contextMenuItemCopyBIPLink);
            _contextMenuItemCopyOSCommand = new MenuItem() { Header = "OS Command" };
            _contextMenuItemCopySubMenu.Items.Add(_contextMenuItemCopyOSCommand);

            _contextMenuItemPaste = new MenuItem() { Header = "Paste" };
            Items.Add(_contextMenuItemPaste);

            _contextMenuItemDeleteSettings = new MenuItem() { Header = "Delete Settings" };
            Items.Add(_contextMenuItemDeleteSettings);
        }

        public void OpenCopySubMenuItem()
        {
            _contextMenuItemCopySubMenu.IsSubmenuOpen = true;
        }

        public int SetVisibility(bool isEmpty, bool containsKeystroke, bool containsKeySequence, bool containsDCSBIOS, bool containsBIPLink, bool containsOSCommand)
        {
            var objectCount = 0;
            try
            {
                HideAll();

                var menuItemVisibilities = GetVisibility(ref objectCount, isEmpty, containsKeystroke, containsKeySequence, containsDCSBIOS, containsBIPLink, containsOSCommand);

                _contextMenuItemAddNullKey.Visibility = menuItemVisibilities.AddNullKeyVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemEditSequence.Visibility = menuItemVisibilities.EditSequenceVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemEditDCSBIOS.Visibility = menuItemVisibilities.EditDCSBIOSVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemEditBIP.Visibility = menuItemVisibilities.EditBIPVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemEditOSCommand.Visibility = menuItemVisibilities.EditOSCommandVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemCopySingle.Visibility = menuItemVisibilities.CopySingleVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemCopySubMenu.Visibility = menuItemVisibilities.CopySubMenuVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemCopyKeyStroke.Visibility = menuItemVisibilities.CopyKeyStrokeVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemCopyKeySequence.Visibility = menuItemVisibilities.CopyKeySequenceVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemCopyDCSBIOS.Visibility = menuItemVisibilities.CopyDCSBIOSVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemCopyBIPLink.Visibility = menuItemVisibilities.CopyBIPLinkVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemCopyOSCommand.Visibility = menuItemVisibilities.CopyOSCommandVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemPaste.Visibility = menuItemVisibilities.PasteVisible ? Visibility.Visible : Visibility.Collapsed;
                _contextMenuItemDeleteSettings.Visibility = menuItemVisibilities.DeleteSettingsVisible ? Visibility.Visible : Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
            return objectCount;
        }


        public DCSFPContextMenuVisibility GetVisibility(ref int objectCount, bool isEmpty, bool containsKeyStroke, bool containsKeySequence, bool containsDCSBIOS, bool containsBIPLink, bool containsOSCommand)
        {
            var result = new DCSFPContextMenuVisibility();
            try
            {
                CopyPackage copyPackage = null;
                var dataObject = Clipboard.GetDataObject();
                if (dataObject != null && dataObject.GetDataPresent("NonVisuals.CopyPackage"))
                {
                    copyPackage = (CopyPackage)dataObject.GetData("NonVisuals.CopyPackage");
                    /*
                    31052022
                    In PZ70 you can paste into the same textbox under different knob positions like ALT VS IAS etc. 
                    if (copyPackage?.SourceName == TextBox.Name)
                    {
                        copyPackage = null; < not valid
                    }*/
                }

                objectCount = 0;
                objectCount += containsKeyStroke ? 1 : 0;
                objectCount += containsKeySequence ? 1 : 0;
                objectCount += containsDCSBIOS ? 1 : 0;
                objectCount += containsBIPLink ? 1 : 0;
                objectCount += containsOSCommand ? 1 : 0;

                if (isEmpty)
                {
                    result.AddNullKeyVisible = true;
                    result.EditSequenceVisible = true; 
                    if (Common.FullDCSBIOSEnabled())
                    {
                        result.EditDCSBIOSVisible = true;
                    }

                    if (BipFactory.HasBips())
                    {
                        result.EditBIPVisible = true;
                    }

                    result.EditOSCommandVisible = true;

                    if (copyPackage != null)
                    {
                        result.PasteVisible = true;
                    }
                }
                
                if (containsKeyStroke)
                {
                    if (BipFactory.HasBips())
                    {
                        result.EditBIPVisible = true;
                    }

                    if (objectCount > 1)
                    {
                        result.CopySubMenuVisible = true;
                        result.CopyKeyStrokeVisible = true;
                    }
                    else
                    {
                        result.CopySingleVisible = true;
                    }

                    if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                    {
                        result.PasteVisible = true;
                    }
                }

                if (containsKeySequence)
                {
                    result.EditSequenceVisible = true;
                    if (BipFactory.HasBips())
                    {
                        result.EditBIPVisible = true;
                    }

                    if (objectCount > 1)
                    {
                        result.CopySubMenuVisible = true;
                        result.CopyKeySequenceVisible = true;
                    }
                    else
                    {
                        result.CopySingleVisible = true;
                    }

                    if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                    {
                        result.PasteVisible = true;
                    }
                }

                if (containsDCSBIOS)
                {
                    if (!_keyboardEmulationOnly)
                    {
                        result.EditDCSBIOSVisible = true;
                        
                        if (objectCount > 1)
                        {
                            result.CopySubMenuVisible = true;
                            result.CopyDCSBIOSVisible = true;
                        }
                        else
                        {
                            result.CopySingleVisible = true;
                        }
                    }

                    if (BipFactory.HasBips())
                    {
                        result.EditBIPVisible = true;
                    }

                    if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                    {
                        result.PasteVisible = true;
                    }
                }

                if (containsBIPLink)
                {
                    result.EditBIPVisible = true;
                    if (objectCount > 1)
                    {
                        result.CopySubMenuVisible = true;
                        result.CopyBIPLinkVisible = true;
                    }
                    else
                    {
                        result.CopySingleVisible = true;
                    }

                    if (copyPackage != null)
                    {
                        switch (copyPackage.ContentType)
                        {
                            case CopyContentType.KeySequence:
                                {
                                    if (!containsKeyStroke && !containsKeySequence && !containsDCSBIOS && !containsOSCommand)
                                    {
                                        result.PasteVisible = true;
                                    }

                                    break;
                                }
                            case CopyContentType.DCSBIOS:
                                {
                                    if (!containsKeyStroke && !containsKeySequence && !containsDCSBIOS && !containsOSCommand)
                                    {
                                        result.PasteVisible = true;
                                    }

                                    break;
                                }
                            case CopyContentType.BIPLink:
                                {
                                    //Cannot paste BIPLink on BIPLink
                                    break;
                                }
                            case CopyContentType.OSCommand:
                                {
                                    if (!containsKeyStroke && !containsKeySequence && !containsDCSBIOS && !containsOSCommand)
                                    {
                                        result.PasteVisible = true;
                                    }

                                    break;
                                }
                        }
                    }
                }

                if (containsOSCommand)
                {
                    result.EditOSCommandVisible = true;
                    if (objectCount > 1)
                    {
                        result.CopySubMenuVisible = true;
                        result.CopyOSCommandVisible = true;
                    }
                    else
                    {
                        result.CopySingleVisible = true;
                    }

                    if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                    {
                        result.PasteVisible = true;
                    }
                }

                if (!isEmpty)
                {
                    result.DeleteSettingsVisible = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

            return result;
        }


        public MenuItem ContextMenuItemAddNullKey
        {
            get => _contextMenuItemAddNullKey;
            set => _contextMenuItemAddNullKey = value;
        }

        public MenuItem ContextMenuItemEditSequence
        {
            get => _contextMenuItemEditSequence;
            set => _contextMenuItemEditSequence = value;
        }

        public MenuItem ContextMenuItemEditDCSBIOS
        {
            get => _contextMenuItemEditDCSBIOS;
            set => _contextMenuItemEditDCSBIOS = value;
        }

        public MenuItem ContextMenuItemEditBIP
        {
            get => _contextMenuItemEditBIP;
            set => _contextMenuItemEditBIP = value;
        }

        public MenuItem ContextMenuItemEditOSCommand
        {
            get => _contextMenuItemEditOSCommand;
            set => _contextMenuItemEditOSCommand = value;
        }
        public MenuItem ContextMenuItemCopySingle
        {
            get => _contextMenuItemCopySingle;
            set => _contextMenuItemCopySingle = value;
        }

        public MenuItem ContextMenuItemCopySubMenu
        {
            get => _contextMenuItemCopySubMenu;
            set => _contextMenuItemCopySubMenu = value;
        }

        public MenuItem ContextMenuItemCopyKeyStroke
        {
            get => _contextMenuItemCopyKeyStroke;
            set => _contextMenuItemCopyKeyStroke = value;
        }

        public MenuItem ContextMenuItemCopyKeySequence
        {
            get => _contextMenuItemCopyKeySequence;
            set => _contextMenuItemCopyKeySequence = value;
        }

        public MenuItem ContextMenuItemCopyDCSBIOS
        {
            get => _contextMenuItemCopyDCSBIOS;
            set => _contextMenuItemCopyDCSBIOS = value;
        }

        public MenuItem ContextMenuItemCopyBIPLink
        {
            get => _contextMenuItemCopyBIPLink;
            set => _contextMenuItemCopyBIPLink = value;
        }

        public MenuItem ContextMenuItemCopyOSCommand
        {
            get => _contextMenuItemCopyOSCommand;
            set => _contextMenuItemCopyOSCommand = value;
        }

        public MenuItem ContextMenuItemPaste
        {
            get => _contextMenuItemPaste;
            set => _contextMenuItemPaste = value;
        }

        public MenuItem ContextMenuItemDeleteSettings
        {
            get => _contextMenuItemDeleteSettings;
            set => _contextMenuItemDeleteSettings = value;
        }

        public TextBox TextBox
        {
            get => _textBox;
            set => _textBox = value;
        }

        public void HideAll()
        {
            HideItems(Items);
        }

        private void HideItems(ItemCollection itemCollection)
        {
            foreach (var item in itemCollection)
            {
                if (item is MenuItem contextMenuItem)
                {
                    contextMenuItem.Visibility = Visibility.Collapsed;
                    HideItems(contextMenuItem.Items);
                }
            }
        }
    }
}
