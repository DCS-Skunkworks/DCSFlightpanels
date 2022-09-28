using NonVisuals;
using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using ClassLibraryCommon;

    using DCSFlightpanels.Bills;
    using DCSFlightpanels.CustomControls;
    using DCSFlightpanels.Shared;

    using MEF;
    using NLog;
    using NonVisuals.Interfaces;
    using NonVisuals.StreamDeck;
    using NonVisuals.StreamDeck.Events;

    public abstract class UserControlStreamDeckUIBase : UserControl, IIsDirty, INvStreamDeckListener, IStreamDeckConfigListener, IOledImageListener
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        protected readonly List<StreamDeckImage> ButtonImages = new List<StreamDeckImage>();
        protected bool UserControlLoaded;
        protected StreamDeckPanel _streamDeckPanel;
        private string _lastShownLayer = string.Empty;
        private BillStreamDeckFace SelectedImageBill => (from image in ButtonImages where image.IsSelected select image.Bill).FirstOrDefault();
        private EnumStreamDeckButtonNames SelectedButtonName
        {
            get
            {
                if (SelectedImageBill == null)
                {
                    return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
                }
                return SelectedImageBill.StreamDeckButtonName;
            }
        }

        public bool IsDirty { get; set; }

        protected virtual void SetFormState() { }

        protected virtual int ButtonAmount()
        {
            return 0;
        }

        protected UserControlStreamDeckUIBase(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }

        protected void ButtonImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var image = (StreamDeckImage)sender;

                SetSelectedButtonUIOnly(image.Bill.StreamDeckButtonName);

                if (image.IsSelected)
                {
                    StreamDeckPanelInstance.SelectedButtonName = image.Bill.Button.StreamDeckButtonName;
                    image.Focus();
                }
                else
                {
                    StreamDeckPanelInstance.SelectedButtonName = EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
                }

                /*Debug.WriteLine(StreamDeckPanelInstance.GetLayerHandlerInformation());
                Debug.WriteLine(StreamDeckPanelInstance.GetConfigurationInformation());
                Debug.WriteLine(EventHandlers.GetInformation());*/
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        protected void ButtonImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (SelectedButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
                {
                    return;
                }

                var newlySelectedImage = (StreamDeckImage)sender;

                /*
                 * Here we must check if event if we can change the button that is selected. If there are unsaved configurations we can't
                 */
                if (newlySelectedImage.Bill.Button != _streamDeckPanel.SelectedButton && SDEventHandler.AreThereDirtyListeners(this))
                {
                    if (CommonUI.DoDiscardAfterMessage(true, $"Discard Changes to {SelectedButtonName} ?"))
                    {
                        SetFormState();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void UIShowLayer()
        {
            try
            {
                var selectedButton = StreamDeckPanelInstance.SelectedButtonName;

                UpdateButtonInfoFromSource();

                SetSelectedButtonUIOnly(selectedButton);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        protected void SetButtonPicture(StreamDeckButton streamdeckButton)
        {
            var button = ButtonImages.FirstOrDefault(x => x.Bill.StreamDeckButtonName == streamdeckButton.StreamDeckButtonName);
            if (button != null)
            {
                switch (streamdeckButton.Face.FaceType)
                {
                    case EnumStreamDeckFaceType.Image:
                        var ftImage = (FaceTypeImage)streamdeckButton.Face;
                        var bitmap = new Bitmap(ftImage.ImageFile);
                        button.Source = BitMapCreator.Bitmap2BitmapImage(bitmap);
                        break;

                    case EnumStreamDeckFaceType.Text:
                        var ftText = (FaceTypeText)streamdeckButton.Face;
                        var bitmapText = BitMapCreator.CreateStreamDeckBitmap(ftText.ButtonTextTemplate, ftText.TextFont, ftText.FontColor, ftText.BackgroundColor, ftText.OffsetX, ftText.OffsetY);
                        button.Source = BitMapCreator.Bitmap2BitmapImage(bitmapText);
                        break;
                    case EnumStreamDeckFaceType.DCSBIOS:
                        var ftDcsBios = (FaceTypeDCSBIOS)streamdeckButton.Face;
                        var dcsBiosDecoder = (DCSBIOSDecoder)streamdeckButton.Face;
                        BitmapImage bitmapDcsBios = dcsBiosDecoder.DecoderOutputType switch
                        {
                            EnumDCSBIOSDecoderOutputType.Raw => BitMapCreator.Bitmap2BitmapImage(BitMapCreator.CreateStreamDeckBitmap(ftDcsBios.ButtonTextTemplate, ftDcsBios.TextFont, ftDcsBios.FontColor, ftDcsBios.BackgroundColor, ftDcsBios.OffsetX, ftDcsBios.OffsetY)),
                            EnumDCSBIOSDecoderOutputType.Converter => StreamDeck.Resources.GetButtonDcsBiosDecoderRule(),
                            _ => throw new Exception("Unexepected DecoderOutputType")
                        };
                        button.Source = bitmapDcsBios;
                        break;
                }
                if (streamdeckButton.Face.FaceType == EnumStreamDeckFaceType.Image)
                {
                    var faceTypeImage = (FaceTypeImage)streamdeckButton.Face;
                    var bitmap = new Bitmap(faceTypeImage.ImageFile);
                    button.Source = BitMapCreator.Bitmap2BitmapImage(bitmap);
                }
            }
        }

        private void UpdateButtonInfoFromSource()
        {
            HideAllDotImages();

            foreach (StreamDeckImage buttonImage in ButtonImages)
            {
                buttonImage.Bill.Clear();

                var streamDeckButton = StreamDeckPanelInstance.SelectedLayer.GetStreamDeckButton(buttonImage.Bill.StreamDeckButtonName);

                buttonImage.Bill.Button = streamDeckButton;

                if (streamDeckButton.HasConfig)
                {
                    SetButtonPicture(streamDeckButton);
                } 
                else
                {
                    buttonImage.SetDefaultButtonImage();
                }
            }
        }

        private void UnSelect()
        {
            try
            {
                SetSelectedButtonUIOnly(EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void Clear()
        {

            HideAllDotImages();

            foreach (var buttonImage in ButtonImages)
            {
                buttonImage.Bill.Clear();
            }
        }

        protected void SetContextMenus()
        {
            /*
                <ContextMenu x:Key="ButtonContextMenu" Opened="ButtonContextMenu_OnOpened" >
                    <MenuItem Name="MenuItemCopy" Header="Copy" Click="ButtonContextMenuCopy_OnClick"/>
                    <MenuItem Name="MenuItemPaste" Header="Paste" Click="ButtonContextMenuPaste_OnClick"/>
                </ContextMenu>
             */
            var contextMenu = new ContextMenu
            {
                Name = "ButtonContextMenu"
            };
            contextMenu.Opened += ButtonContextMenu_OnOpened;

            var menuItem = new MenuItem
            {
                Name = "MenuItemCopy",
                Header = "Copy"
            };
            menuItem.Click += ButtonContextMenuCopy_OnClick;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem
            {
                Name = "MenuItemPaste",
                Header = "Paste"
            };
            menuItem.Click += ButtonContextMenuPaste_OnClick;
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());

            menuItem = new MenuItem
            {
                Name = "MenuItemDelete",
                Header = "Delete"
            };
            menuItem.Click += ButtonContextMenuDelete_OnClick;
            contextMenu.Items.Add(menuItem);

            foreach (var streamDeckImage in ButtonImages)
            {
                streamDeckImage.ContextMenu = contextMenu;
            }
        }

        private void ButtonContextMenuCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Copy();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonContextMenuPaste_OnClick(object sender, RoutedEventArgs e)
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

        private void ButtonContextMenuDelete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var streamDeckButton = _streamDeckPanel.SelectedLayer.GetStreamDeckButton(SelectedButtonName);
                if (MessageBox.Show("Delete button" + streamDeckButton.StreamDeckButtonName.ToString() + "?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _streamDeckPanel.SelectedLayer.RemoveButton(streamDeckButton);
                    SDEventHandler.ClearSettings(this, true, true, true, _streamDeckPanel.BindingHash); 
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                 * Can't get context menu [ContextMenuOpening] events to work. Workaround.
                 */
                var contextMenu = (ContextMenu)sender;
                MenuItem menuItemCopy = null;
                MenuItem menuItemPaste = null;
                MenuItem menuItemDelete = null;

                foreach (var contextMenuItem in contextMenu.Items)
                {
                    if (contextMenuItem.GetType() == typeof(MenuItem) && ((MenuItem)contextMenuItem).Name == "MenuItemCopy")
                    {
                        menuItemCopy = ((MenuItem)contextMenuItem);
                    }
                }
                foreach (var contextMenuItem in contextMenu.Items)
                {
                    if (contextMenuItem.GetType() == typeof(MenuItem) && ((MenuItem)contextMenuItem).Name == "MenuItemPaste")
                    {
                        menuItemPaste = (MenuItem)contextMenuItem;
                    }
                }
                foreach (var contextMenuItem in contextMenu.Items)
                {
                    if (contextMenuItem.GetType() == typeof(MenuItem) && ((MenuItem)contextMenuItem).Name == "MenuItemPaste")
                    {
                        menuItemDelete = (MenuItem)contextMenuItem;
                    }
                }

                if (menuItemCopy == null || menuItemPaste == null || menuItemDelete == null)
                {
                    return;
                }
                var selectedStreamDeckButton = _streamDeckPanel.SelectedLayer.GetStreamDeckButton(SelectedButtonName);
                menuItemCopy.IsEnabled = selectedStreamDeckButton.HasConfig;
                menuItemDelete.IsEnabled = selectedStreamDeckButton.HasConfig;

                var dataObject = Clipboard.GetDataObject();
                menuItemPaste.IsEnabled = dataObject != null && dataObject.GetDataPresent("NonVisuals.StreamDeck.StreamDeckButton");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void HideAllDotImages()
        {

        }

        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set => _streamDeckPanel = value;
        }

        protected void ShowGraphicConfiguration()
        {
            try
            {
                UIShowLayer();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        protected void SetSelectedButtonUIOnly(EnumStreamDeckButtonNames selectedButtonName)
        {
            //Deselect everything selected (normaly should only be 1 currently selected but we never know...
            ButtonImages.Where(x => x.IsSelected).ToList().ForEach(x => 
                {
                    x.IsSelected = false;
                });

            //Select the one
            var selectedButton = ButtonImages.FirstOrDefault(x => x.Bill.StreamDeckButtonName == selectedButtonName);
            if (selectedButton != null)
            {
                selectedButton.IsSelected = true;
            }
        }

        public void SetIsDirty()
        {
            IsDirty = true;
        }

        public void StateSaved()
        {
            IsDirty = false;
        }

        protected void SetImageBills()
        {
            foreach (var buttonImage in ButtonImages)
            {
                if (buttonImage.Bill != null)
                {
                    continue;
                }
                buttonImage.Bill = new BillStreamDeckFace
                {
                    StreamDeckButtonName = (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + buttonImage.Name.Replace("ButtonImage", string.Empty)),
                };

                buttonImage.Bill.StreamDeckPanelInstance = _streamDeckPanel;
                buttonImage.SetDefaultButtonImage();                
            }
        }

        protected void Copy()
        {
            var streamDeckButton = _streamDeckPanel.SelectedLayer.GetStreamDeckButton(SelectedButtonName);
            if (streamDeckButton != null)
            {
                Clipboard.SetDataObject(streamDeckButton.CloneJson());
            }
        }

        protected bool Paste()
        {
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null || !dataObject.GetDataPresent("NonVisuals.StreamDeck.StreamDeckButton"))
            {
                return false;
            }

            bool result;
            var newStreamDeckButton = (StreamDeckButton)dataObject.GetData("NonVisuals.StreamDeck.StreamDeckButton");
            
            var oldStreamDeckButton = _streamDeckPanel.SelectedLayer.GetStreamDeckButton(SelectedButtonName);
            if (oldStreamDeckButton.CheckIfWouldOverwrite(newStreamDeckButton) &&
                MessageBox.Show("Overwrite previous configuration (partial or fully)", "Overwrite?)", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                result = oldStreamDeckButton.Consume(true, newStreamDeckButton);
            }
            else
            {
                result = oldStreamDeckButton.Consume(true, newStreamDeckButton);
            }
            /*
             * 15 Dec 2021 JDA
             * For some reason some properties does not follow through the copying phase, e.g. StreamDeckPanelInstance is null for Face object after copy. Why?
             * Have to set it, otherwise nullpointer exception.
             */
            oldStreamDeckButton.StreamDeckPanelInstance = oldStreamDeckButton.StreamDeckPanelInstance;
            if (result)
            {
                _streamDeckPanel.SelectedLayer.AddButton(oldStreamDeckButton);
                UpdateButtonInfoFromSource();
                SetIsDirty();
            }
            return result;
        }

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash && _lastShownLayer != e.SelectedLayerName)
                {
                    Dispatcher?.BeginInvoke((Action)(UIShowLayer));
                    _lastShownLayer = e.SelectedLayerName;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                /*
                 * Only do it when it is a different button selected. Should make more comments...
                 */
                if ((_streamDeckPanel.BindingHash == e.BindingHash && SelectedImageBill == null) || (SelectedImageBill != null && SelectedImageBill.Button.GetHash() != e.SelectedButton.GetHash()))
                {
                    SetSelectedButtonUIOnly(e.SelectedButton.StreamDeckButtonName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void ClearSettings(object sender, StreamDeckClearSettingsArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash && e.ClearUIConfiguration)
                {
                    Dispatcher?.BeginInvoke((Action)HideAllDotImages);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        
        public void SyncConfiguration(object sender, StreamDeckSyncConfigurationArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    Dispatcher?.BeginInvoke((Action)UpdateButtonInfoFromSource);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void ConfigurationChanged(object sender, StreamDeckConfigurationChangedArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    Dispatcher?.BeginInvoke((Action)UpdateButtonInfoFromSource);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (e.RemoteBindingHash == _streamDeckPanel.BindingHash)
                {
                    Dispatcher?.BeginInvoke((Action)SetFormState);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
      

        protected void CheckButtonControlListValidity()
        {
            if (ButtonImages.Count != ButtonAmount())
            {
                //Error messages only flashes briefly to the user :-( but is logged in error log :-).
                //This error should not happen in theory if the screen is correctly designed, Debug.assert to warn the dev.
                //Clear lists to show to the user that something wrong happened.
                Common.ShowErrorMessageBox(
                    new Exception($"Error initializing streamdeck buttons list. Expecting [{ButtonAmount()}] got [{ButtonImages.Count()}]"
                    ));
                Debug.Assert(false);
                ButtonImages.Clear();
            }
        }


    }
}
