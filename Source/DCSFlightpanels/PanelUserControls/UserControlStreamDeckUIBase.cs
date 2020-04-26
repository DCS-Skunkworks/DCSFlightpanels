using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Shared;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;
using NonVisuals.StreamDeck.Events;


namespace DCSFlightpanels.PanelUserControls
{
    public abstract class UserControlStreamDeckUIBase : UserControl, IIsDirty, IStreamDeckListener
    {
        protected readonly List<StreamDeckImage> ButtonImages = new List<StreamDeckImage>();
        protected readonly List<System.Windows.Controls.Image> DotImages = new List<System.Windows.Controls.Image>();
        protected bool UserControlLoaded;
        protected StreamDeckButton StreamDeckButtonInstance;
        private StreamDeckButton _pastedStreamDeckButton;
        public string StreamDeckInstanceId;

        private string _lastShownLayer = "";

        protected virtual void SetFormState() { }

        protected virtual int ButtonAmount()
        {
            return 0;
        }


        protected void ButtonImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var image = (StreamDeckImage)sender;

                SetSelectedButton(image.Bill.StreamDeckButtonName);

                if (image.IsSelected)
                {
                    StreamDeckPanelInstance.SelectedButtonName = image.Bill.Button.StreamDeckButtonName;
                    image.Focus();
                }
                else
                {
                    StreamDeckPanelInstance.SelectedButtonName = EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
                }

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
                if (EventHandlers.AreThereDirtyListeners(this))
                {
                    if (CommonUI.DoDiscardAfterMessage(true, "Discard Changes to " + SelectedButtonName + " ?"))
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
        
        public void UIShowLayer(string layerName)
        {
            try
            {
                var selectedButton = StreamDeckPanelInstance.SelectedButtonName;

                HideAllDotImages();

                var selectedLayer = StreamDeckPanelInstance.GetLayer(layerName);

                foreach (var buttonImage in ButtonImages)
                {
                    buttonImage.Bill.Clear();

                    var streamDeckButton = selectedLayer.GetStreamDeckButton(buttonImage.Bill.StreamDeckButtonName);

                    buttonImage.Bill.Button = streamDeckButton;

                    if (streamDeckButton.HasConfig)
                    {
                        SetDotImageStatus(true, StreamDeckFunction.ButtonNumber(streamDeckButton.StreamDeckButtonName));
                    }
                }

                SetSelectedButton(selectedButton);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        public void UnSelect()
        {
            try
            {
                SetSelectedButton(EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON);
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
            var contextMenu = new ContextMenu();
            contextMenu.Name = "ButtonContextMenu";
            contextMenu.Opened += ButtonContextMenu_OnOpened;

            var menuItem = new MenuItem();
            menuItem.Name = "MenuItemCopy";
            menuItem.Header = "Copy";
            menuItem.Click += ButtonContextMenuCopy_OnClick;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Name = "MenuItemPaste";
            menuItem.Header = "Paste";
            menuItem.Click += ButtonContextMenuPaste_OnClick;
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
                foreach (MenuItem contextMenuItem in contextMenu.Items)
                {
                    if (contextMenuItem.Name == "MenuItemCopy")
                    {
                        menuItemCopy = contextMenuItem;
                    }
                }
                foreach (MenuItem contextMenuItem in contextMenu.Items)
                {
                    if (contextMenuItem.Name == "MenuItemPaste")
                    {
                        menuItemPaste = contextMenuItem;
                    }
                }

                if (menuItemCopy == null || menuItemPaste == null)
                {
                    return;
                }
                var selectedStreamDeckButton = StreamDeckPanel.GetInstance(StreamDeckInstanceId).SelectedLayer.GetStreamDeckButton(SelectedButtonName);
                menuItemCopy.IsEnabled = selectedStreamDeckButton.HasConfig;

                var iDataObject = Clipboard.GetDataObject();
                menuItemPaste.IsEnabled = iDataObject != null && iDataObject.GetDataPresent("NonVisuals.StreamDeck.StreamDeckButton");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        protected void SetDotImageStatus(bool show, int number, bool allOthersNegated = false)
        {

            foreach (var dotImage in DotImages)
            {
                if (allOthersNegated)
                {
                    dotImage.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
                }

                if (dotImage.Name == "DotImage" + number)
                {
                    dotImage.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public void HideAllDotImages()
        {
            foreach (var dotImage in DotImages)
            {
                dotImage.Visibility = Visibility.Collapsed;
            }
        }

        protected StreamDeckPanel StreamDeckPanelInstance => StreamDeckPanel.GetInstance(StreamDeckInstanceId);

        protected void ShowGraphicConfiguration()
        {
            try
            {
                UIShowLayer(StreamDeckPanelInstance.HomeLayer.Name);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        protected void SetSelectedButton(EnumStreamDeckButtonNames selectedButtonName)
        {
            foreach (var buttonImage in ButtonImages)
            {
                try
                {
                    if (selectedButtonName == buttonImage.Bill.StreamDeckButtonName)
                    {
                        buttonImage.Source = buttonImage.Bill.SelectedImage;
                        buttonImage.IsSelected = true;
                    }
                    else
                    {
                        buttonImage.Source = buttonImage.Bill.DeselectedImage;
                        buttonImage.IsSelected = false;
                    }
                }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
        }

        public void SetIsDirty()
        {
            IsDirty = true;
        }

        public bool IsDirty { get; set; }

        public void StateSaved()
        {
            IsDirty = false;
        }

        public EnumStreamDeckButtonNames SelectedButtonName
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

        private BillStreamDeckFace SelectedImageBill => (from image in ButtonImages where image.IsSelected select image.Bill).FirstOrDefault();

        protected void SetImageBills()
        {
            foreach (var buttonImage in ButtonImages)
            {
                if (buttonImage.Bill != null)
                {
                    continue;
                }
                buttonImage.Bill = new BillStreamDeckFace();
                buttonImage.Bill.StreamDeckButtonName = (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + buttonImage.Name.Replace("ButtonImage", ""));
                buttonImage.Bill.SelectedImage = BitMapCreator.GetButtonNumberImage(buttonImage.Bill.StreamDeckButtonName, System.Drawing.Color.Green);
                buttonImage.Bill.DeselectedImage = BitMapCreator.GetButtonNumberImage(buttonImage.Bill.StreamDeckButtonName, Color.Blue);
                buttonImage.Bill.StreamDeckInstanceId = StreamDeckInstanceId;
                buttonImage.Source = buttonImage.Bill.DeselectedImage;
            }
        }

        protected void Copy()
        {
            var streamDeckButton = StreamDeckPanel.GetInstance(StreamDeckInstanceId).SelectedLayer.GetStreamDeckButton(SelectedButtonName);
            if (streamDeckButton != null)
            {
                Clipboard.SetDataObject(streamDeckButton);
            }
        }

        protected bool Paste()
        {
            var iDataObject = Clipboard.GetDataObject();
            if (iDataObject == null || !iDataObject.GetDataPresent("NonVisuals.StreamDeck.StreamDeckButton"))
            {
                return false;
            }

            var result = false;
            var newStreamDeckButton = (StreamDeckButton)iDataObject.GetData("NonVisuals.StreamDeck.StreamDeckButton");
            var oldStreamDeckButton = StreamDeckPanel.GetInstance(StreamDeckInstanceId).SelectedLayer.GetStreamDeckButton(SelectedButtonName);
            if (oldStreamDeckButton.CheckIfWouldOverwrite(newStreamDeckButton))
            {
                if (MessageBox.Show("Overwrite previous configuration (partial or fully)", "Overwrite?)", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    result = oldStreamDeckButton.Consume(true, newStreamDeckButton);
                }
            }
            else
            {
                result = oldStreamDeckButton.Consume(true, newStreamDeckButton);
            }

            if (result)
            {
                _pastedStreamDeckButton = oldStreamDeckButton;
                Refresh();
                SetIsDirty();
            }
            return result;
        }

        public void Refresh()
        {
            try
            {
                HideAllDotImages();
                UnSelect();
                var selectedLayer = StreamDeckPanelInstance.SelectedLayer;

                foreach (var buttonImage in ButtonImages)
                {
                    buttonImage.Bill.Clear();

                    var streamDeckButton = selectedLayer.GetStreamDeckButton(buttonImage.Bill.StreamDeckButtonName);

                    buttonImage.Bill.Button = streamDeckButton;

                    if (streamDeckButton.HasConfig)
                    {
                        SetDotImageStatus(true, StreamDeckFunction.ButtonNumber(streamDeckButton.StreamDeckButtonName));
                    }
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public StreamDeckButton PastedStreamDeckButton
        {
            get => _pastedStreamDeckButton;
            set
            {
                _pastedStreamDeckButton = value;
                Refresh();
            }
        }

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (_lastShownLayer != e.SelectedLayerName)
                {
                    UIShowLayer(e.SelectedLayerName);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckShowNewButtonArgs e)
        {
            try
            {
                var selectedButton = StreamDeckPanel.GetInstance(StreamDeckInstanceId).SelectedButton;
                if (SelectedImageBill == null || SelectedImageBill.Button != e.SelectedButton)
                {
                    SetSelectedButton(e.SelectedButton.StreamDeckButtonName);
                }
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
                if (e.ClearUIConfiguration)
                {
                    HideAllDotImages();
                    _pastedStreamDeckButton = null;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }
    }
}
