using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using NonVisuals.Saitek;
using CommonClassLibraryJD;
using NonVisuals;

namespace DCSFlightpanels
{
    public class ContextMenuPanelTextBox : ContextMenu
    {
        private MenuItem _contextMenuItemAddNullKey;
        private MenuItem _contextMenuItemEditSequence;
        private MenuItem _contextMenuItemEditDCSBIOS;
        private MenuItem _contextMenuItemEditBIP;
        private MenuItem _contextMenuItemEditOSCommand;
        private MenuItem _contextMenuItemCopy;
        private MenuItem _contextMenuItemCopyKeySequence;
        private MenuItem _contextMenuItemCopyDCSBIOS;
        private MenuItem _contextMenuItemCopyBIPLink;
        private MenuItem _contextMenuItemCopyOSCommand;
        private MenuItem _contextMenuItemPaste;
        private MenuItem _contextMenuItemDeleteSettings;



        private bool _keyboardEmulationOnly;

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


            _contextMenuItemCopy = new MenuItem() { Header = "Copy" };
            Items.Add(_contextMenuItemCopy);
            _contextMenuItemCopyKeySequence = new MenuItem() { Header = "Key Sequence" };
            _contextMenuItemCopy.Items.Add(_contextMenuItemCopyKeySequence);
            _contextMenuItemCopyDCSBIOS = new MenuItem() { Header = "DCS-BIOS" };
            _contextMenuItemCopy.Items.Add(_contextMenuItemCopyDCSBIOS);
            _contextMenuItemCopyBIPLink = new MenuItem() { Header = "BIP Link" };
            _contextMenuItemCopy.Items.Add(_contextMenuItemCopyBIPLink);
            _contextMenuItemCopyOSCommand = new MenuItem() { Header = "OS Command" };
            _contextMenuItemCopy.Items.Add(_contextMenuItemCopyOSCommand);

            _contextMenuItemPaste = new MenuItem() { Header = "Paste" };
            Items.Add(_contextMenuItemPaste);

            _contextMenuItemDeleteSettings = new MenuItem() { Header = "Delete Settings" };
            Items.Add(_contextMenuItemDeleteSettings);
        }

        public void SetVisibility(bool isEmpty, bool containsSinglePress, bool containsKeySequence, bool containsDCSBIOS, bool containsBIPLink, bool containsOSCommand)
        {
            try
            {
                HideAll();

                CopyPackage copyPackage = null;
                var iDataObject = Clipboard.GetDataObject();
                if (iDataObject != null && iDataObject.GetDataPresent("NonVisuals.CopyPackage"))
                {
                    copyPackage = (CopyPackage)iDataObject.GetData("NonVisuals.CopyPackage");
                }

                if (isEmpty)
                {
                    _contextMenuItemAddNullKey.Visibility = Visibility.Visible;
                    _contextMenuItemEditSequence.Visibility = Visibility.Visible;
                    if (Common.FullDCSBIOSEnabled())
                    {
                        _contextMenuItemEditDCSBIOS.Visibility = Visibility.Visible;
                    }
                    if (BipFactory.HasBips())
                    {
                        _contextMenuItemEditBIP.Visibility = Visibility.Visible;
                    }
                    _contextMenuItemEditOSCommand.Visibility = Visibility.Visible;

                    if (copyPackage != null)
                    {
                        _contextMenuItemPaste.Visibility = Visibility.Visible;
                    }
                }
                if (containsSinglePress)
                {
                    if (BipFactory.HasBips())
                    {
                        _contextMenuItemEditBIP.Visibility = Visibility.Visible;
                    }

                    if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                    {
                        _contextMenuItemPaste.Visibility = Visibility.Visible;
                    }
                }
                if (containsKeySequence)
                {
                    _contextMenuItemEditSequence.Visibility = Visibility.Visible;
                    if (BipFactory.HasBips())
                    {
                        _contextMenuItemEditBIP.Visibility = Visibility.Visible;
                    }
                    _contextMenuItemCopy.Visibility = Visibility.Visible;
                    _contextMenuItemCopyKeySequence.Visibility = Visibility.Visible;

                    if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                    {
                        _contextMenuItemPaste.Visibility = Visibility.Visible;
                    }
                }
                if (containsDCSBIOS)
                {
                    if (!_keyboardEmulationOnly)
                    {
                        _contextMenuItemEditDCSBIOS.Visibility = Visibility.Visible;
                        _contextMenuItemCopy.Visibility = Visibility.Visible;
                        _contextMenuItemCopyDCSBIOS.Visibility = Visibility.Visible;
                    }

                    if (BipFactory.HasBips())
                    {
                        _contextMenuItemEditBIP.Visibility = Visibility.Visible;
                    }

                    if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                    {
                        _contextMenuItemPaste.Visibility = Visibility.Visible;
                    }
                }

                if (containsBIPLink)
                {
                    _contextMenuItemEditBIP.Visibility = Visibility.Visible;
                    _contextMenuItemCopy.Visibility = Visibility.Visible;
                    _contextMenuItemCopyBIPLink.Visibility = Visibility.Visible;

                    if (copyPackage != null)
                    {
                        switch (copyPackage.ContentType)
                        {
                            case CopyContentType.KeySequence:
                            {
                                if (!containsSinglePress && !containsKeySequence && !containsDCSBIOS && !containsOSCommand)
                                {
                                    _contextMenuItemPaste.Visibility = Visibility.Visible;
                                }

                                break;
                            }
                            case CopyContentType.DCSBIOS:
                            {
                                if (!containsSinglePress && !containsKeySequence && !containsDCSBIOS && !containsOSCommand)
                                {
                                    _contextMenuItemPaste.Visibility = Visibility.Visible;
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
                                if (!containsSinglePress && !containsKeySequence && !containsDCSBIOS && !containsOSCommand)
                                {
                                    _contextMenuItemPaste.Visibility = Visibility.Visible;
                                }

                                break;
                            }
                        }
                    }

                    if (containsOSCommand)
                    {
                        _contextMenuItemEditOSCommand.Visibility = Visibility.Visible;
                        _contextMenuItemCopy.Visibility = Visibility.Visible;
                        _contextMenuItemCopyOSCommand.Visibility = Visibility.Visible;

                        if (copyPackage != null && copyPackage.ContentType == CopyContentType.BIPLink)
                        {
                            _contextMenuItemPaste.Visibility = Visibility.Visible;
                        }
                    }
                }

                if (!isEmpty)
                {
                    _contextMenuItemDeleteSettings.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
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

        public MenuItem ContextMenuItemCopy
        {
            get => _contextMenuItemCopy;
            set => _contextMenuItemCopy = value;
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

        public void HideAll()
        {
            HideItems(Items);
        }

        private void HideItems(ItemCollection itemCollection)
        {
            foreach (var item in itemCollection)
            {
                if (item is MenuItem)
                {
                    var contextMenuItem = (MenuItem)item;
                    contextMenuItem.Visibility = Visibility.Collapsed;
                    HideItems(contextMenuItem.Items);
                }
            }
        }
    }
}
