namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using ClassLibraryCommon;
    using CustomControls;
    using MEF;
    using NLog;
    using NonVisuals.Interfaces;
    using NonVisuals.Panels.StreamDeck.Events;
    using NonVisuals.Panels.StreamDeck.Panels;
    using NonVisuals.Panels.StreamDeck;
    using Newtonsoft.Json;
    using System.Drawing;

    public abstract class UserControlStreamDeckUIBase : UserControl, IIsDirty, INvStreamDeckListener, IStreamDeckConfigListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly List<StreamDeckImage> ButtonImages = new();
        protected readonly List<StreamDeckPushRotaryCtrl> ButtonPushRotary = new();
        protected bool UserControlLoaded;
        private StreamDeckPanel _streamDeckPanel;
        private string _lastShownLayer = string.Empty;
        private StreamDeckImage SelectedImage => ButtonImages.FirstOrDefault(o => o.IsSelected);
        private StreamDeckPushRotaryCtrl SelectedPushRotaryCtrl => (from pushRotaryCtrl in ButtonPushRotary where pushRotaryCtrl.IsSelected select pushRotaryCtrl).FirstOrDefault();

        private readonly JsonSerializerSettings _jsonSettings = new()
        {
            ContractResolver = new ExcludeObsoletePropertiesResolver(),
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) =>
            {
                Logger.Error($"JSON Error.{args.ErrorContext.Error.Message}");
            }
        };

        private EnumStreamDeckButtonNames SelectedButtonName
        {
            get
            {
                if (SelectedImage == null)
                {
                    return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
                }
                return SelectedImage.StreamDeckButtonName;
            }
        }

        public bool IsDirty { get; set; }

        protected virtual void SetFormState() { }

        protected virtual int ButtonAmount()
        {
            return 0;
        }

        protected virtual int ButtonPushRotaryAmount()
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

                SetSelectedButtonUIOnly(image.StreamDeckButtonName);

                if (image.IsSelected)
                {
                    StreamDeckPanelInstance.SelectedButtonName = image.Button.StreamDeckButtonName;
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

        protected void StreamDeckPushRotary_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var pushRotary = (StreamDeckPushRotaryCtrl)sender;

                SetSelectedPushRotaryUIOnly(pushRotary.StreamDeckPushRotary.StreamDeckPushRotaryName);
                if (pushRotary.IsSelected)
                {
                    StreamDeckPanelInstance.SelectedPushRotaryName = pushRotary.StreamDeckPushRotary.StreamDeckPushRotaryName;
                    pushRotary.Focus();
                }
                else
                {
                    StreamDeckPanelInstance.SelectedPushRotaryName = EnumStreamDeckPushRotaryNames.PUSHROTARY0_NO_PUSHROTARY;
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
            var button = ButtonImages.FirstOrDefault(x => x.StreamDeckButtonName == streamdeckButton.StreamDeckButtonName);
            if (streamdeckButton.Face == null)
            {
                return; // bug
            }

            if (button != null)
            {
                switch (streamdeckButton.Face.FaceType)
                {
                    case EnumStreamDeckFaceType.Image:
                        var ftImage = (FaceTypeImage)streamdeckButton.Face;
                        Bitmap bitmap = ftImage.GetLoadedBitmap_FallBackLoadFile();
                        button.Source = BitMapCreator.Bitmap2BitmapImage(bitmap);
                        break;

                    case EnumStreamDeckFaceType.Text:
                        var ftText = (FaceTypeText)streamdeckButton.Face;
                        var bitmapText = BitMapCreator.CreateStreamDeckBitmap(ftText.ButtonTextTemplate, ftText.TextFont, ftText.FontColor, ftText.OffsetX, ftText.OffsetY, ftText.BackgroundColor);
                        button.Source = BitMapCreator.Bitmap2BitmapImage(bitmapText);
                        break;
                    case EnumStreamDeckFaceType.DCSBIOS:
                        var ftDcsBios = (FaceTypeDCSBIOS)streamdeckButton.Face;
                        var dcsBiosDecoder = (DCSBIOSDecoder)streamdeckButton.Face;
                        BitmapImage bitmapDcsBios = dcsBiosDecoder.DecoderOutputType switch
                        {
                            EnumDCSBIOSDecoderOutputType.Raw => BitMapCreator.Bitmap2BitmapImage(BitMapCreator.CreateStreamDeckBitmap(ftDcsBios.ButtonTextTemplate, ftDcsBios.TextFont, ftDcsBios.FontColor, ftDcsBios.OffsetX, ftDcsBios.OffsetY, ftDcsBios.BackgroundColor)),
                            EnumDCSBIOSDecoderOutputType.Converter => StreamDeck.Resources.GetButtonDcsBiosDecoderRule(),
                            _ => throw new Exception("Unexepected DecoderOutputType")
                        };
                        button.Source = bitmapDcsBios;
                        break;
                }
                if (streamdeckButton.Face.FaceType == EnumStreamDeckFaceType.Image)
                {
                    var faceTypeImage = (FaceTypeImage)streamdeckButton.Face;
                    var bitmap = faceTypeImage.GetLoadedBitmap_FallBackLoadFile();
                    button.Source = BitMapCreator.Bitmap2BitmapImage(bitmap);
                }
            }
        }

        private void UpdateButtonInfoFromSource()
        {
            HideAllDotImages();

            foreach (StreamDeckImage buttonImage in ButtonImages)
            {
                buttonImage.Clear();

                var streamDeckButton = StreamDeckPanelInstance.SelectedLayer.GetStreamDeckButton(buttonImage.StreamDeckButtonName);

                buttonImage.Button = streamDeckButton;

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
                
                menuItemPaste.IsEnabled = Clipboard.GetData(DataFormats.StringFormat) != null;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public static void HideAllDotImages()
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
            DeselectEveryButtonControls();

            //Select the one
            var selectedButton = ButtonImages.FirstOrDefault(x => x.StreamDeckButtonName == selectedButtonName);
            if (selectedButton != null)
            {
                selectedButton.IsSelected = true;
            }
        }

        private void DeselectEveryButtonControls()
        {
            ButtonImages.Where(x => x.IsSelected).ToList().ForEach(x => { x.IsSelected = false; });
            ButtonPushRotary.Where(x => x.IsSelected).ToList().ForEach(x => { x.IsSelected = false; });
        }

        protected void SetSelectedPushRotaryUIOnly(EnumStreamDeckPushRotaryNames pushRotaryName)
        {
            DeselectEveryButtonControls();

             //Select the one
             var selectedPushRotaryCtrl = ButtonPushRotary.FirstOrDefault(x => x.StreamDeckPushRotary.StreamDeckPushRotaryName == pushRotaryName);
            if (selectedPushRotaryCtrl != null)
            {
                selectedPushRotaryCtrl.IsSelected = true;
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

        protected void SetImageEnvironment()
        {
            foreach (var buttonImage in ButtonImages)
            {
                if (buttonImage.StreamDeckButtonName != EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
                {
                    continue;
                }

                buttonImage.StreamDeckButtonName = (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + buttonImage.Name.Replace("ButtonImage", string.Empty));
                buttonImage.StreamDeckPanelInstance = _streamDeckPanel;
                buttonImage.SetDefaultButtonImage();
            }

            foreach (var buttonPushRotaryCtrl in ButtonPushRotary)
            {
                if (buttonPushRotaryCtrl.StreamDeckPanelInstance != null)
                {
                    continue;
                }
                buttonPushRotaryCtrl.StreamDeckPushRotary.StreamDeckPushRotaryName = (EnumStreamDeckPushRotaryNames)Enum.Parse(typeof(EnumStreamDeckPushRotaryNames), "PUSHROTARY" + buttonPushRotaryCtrl.Name.Replace("StreamDeckPushRotary", string.Empty));
                buttonPushRotaryCtrl.StreamDeckPanelInstance = _streamDeckPanel;
            }
        }

        protected void Copy()
        {
            var streamDeckButton = _streamDeckPanel.SelectedLayer.GetStreamDeckButton(SelectedButtonName);
            if (streamDeckButton != null)
            {
                var buttonJSON = JsonConvert.SerializeObject(streamDeckButton, Formatting.Indented, _jsonSettings);
                Clipboard.SetData(DataFormats.StringFormat, buttonJSON);
            }
        }

        protected bool Paste()
        {
            var jsonData = (string)Clipboard.GetData(DataFormats.StringFormat);
            if (jsonData == null)
            {
                return false;
            }

            bool result;
            var newStreamDeckButton = JsonConvert.DeserializeObject<StreamDeckButton>(jsonData, _jsonSettings);

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
             * Have to set it, otherwise null pointer exception.
             */
            oldStreamDeckButton.StreamDeckPanelInstance = oldStreamDeckButton.StreamDeckPanelInstance;
            if (result)
            {
                _streamDeckPanel.SelectedLayer.AddButton(oldStreamDeckButton);
                UpdateButtonInfoFromSource();
                SetIsDirty();
                Clipboard.Clear();
            }
            return result;
        }

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash && _lastShownLayer != e.SelectedLayerName)
                {
                    Dispatcher?.BeginInvoke(UIShowLayer);
                    _lastShownLayer = e.SelectedLayerName;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                if (e.SelectedButton != null)
                {
                    /*
                     * Only do it when it is a different button selected. Should make more comments...
                     */
                    if ((_streamDeckPanel.BindingHash == e.BindingHash && SelectedImage == null) || (SelectedImage != null && SelectedImage.Button.GetHash() != e.SelectedButton.GetHash()))
                    {
                        SetSelectedButtonUIOnly(e.SelectedButton.StreamDeckButtonName);
                    }
                }
                if (e.SelectedPushRotary != null)
                {
                    if ((_streamDeckPanel.BindingHash == e.BindingHash && SelectedPushRotaryCtrl == null) || (SelectedPushRotaryCtrl != null && SelectedPushRotaryCtrl.StreamDeckPushRotary.GetHash() != e.SelectedPushRotary.GetHash()))
                    {
                        SetSelectedPushRotaryUIOnly(e.SelectedPushRotary.StreamDeckPushRotaryName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                    new Exception($"Error initializing streamdeck buttons list. Expecting [{ButtonAmount()}] got [{ButtonImages.Count}]"
                    ));
                Debug.Assert(false);
                ButtonImages.Clear();
            }
        }


    }
}
