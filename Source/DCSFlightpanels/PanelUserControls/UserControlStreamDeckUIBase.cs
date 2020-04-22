using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;
using StreamDeckSharp;

namespace DCSFlightpanels.PanelUserControls
{
    public abstract class UserControlStreamDeckUIBase : UserControl, IIsDirty
    {
        protected IStreamDeckUIParent SDUIParent;
        protected readonly List<StreamDeckImage> ButtonImages = new List<StreamDeckImage>();
        protected readonly List<System.Windows.Controls.Image> DotImages = new List<System.Windows.Controls.Image>();
        protected bool UserControlLoaded;
        protected StreamDeckButton StreamDeckButtonInstance;
        protected bool _isDirty = false;
        private StreamDeckButton _pastedStreamDeckButton;



        public void SetSDUIParent(IStreamDeckUIParent sduiParent)
        {
            SDUIParent = sduiParent;
        }

        protected virtual void SetFormState()
        {}


        public void UIShowLayer(string layerName)
        {
            try
            {
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
                    SetFormState();
                }
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
                UpdateAllButtonsSelectedStatus(EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON);
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
                var selectedStreamDeckButton = StreamDeckPanel.GetInstance(SDUIParent.GetStreamDeckInstanceId()).GetActiveLayer().GetStreamDeckButton(SelectedButtonName);
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

        protected StreamDeckPanel StreamDeckPanelInstance
        {
            get
            {
                return StreamDeckPanel.GetInstance(SDUIParent.GetStreamDeckInstanceId());
            }
        }

        protected void ShowGraphicConfiguration()
        {
            try
            {
                UIShowLayer(StreamDeckPanelInstance.HomeLayer.Name);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void UpdateAllButtonsSelectedStatus(EnumStreamDeckButtonNames selectedButtonName)
        {

            //System.Diagnostics.Debugger.Break();

            foreach (var buttonImage in ButtonImages)
            {
                try
                {
                    if (selectedButtonName == buttonImage.Bill.StreamDeckButtonName)
                    {
                        if (buttonImage.Bill.IsSelected)
                        {
                            buttonImage.Source = buttonImage.Bill.DeselectedImage;
                            buttonImage.Bill.IsSelected = false;
                        }
                        else
                        {
                            buttonImage.Source = buttonImage.Bill.SelectedImage;
                            buttonImage.Bill.IsSelected = true;
                        }
                    }
                    else
                    {
                        buttonImage.Source = buttonImage.Bill.DeselectedImage;
                        buttonImage.Bill.IsSelected = false;
                    }
                }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
        }

        public void SetButtonGridStatus(bool enabled)
        {
            foreach (var streamDeckImage in ButtonImages)
            {
                streamDeckImage.IsEnabled = enabled;
            }
        }

        public void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }
        
        public void StateSaved()
        {
            _isDirty = false;
        }

        protected void ButtonImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                /*
                 * bselect 
                 */
                var image = (StreamDeckImage)sender;


                SDUIParent.ActionPanel.Clear();
                SDUIParent.FacePanel.Clear();
                SDUIParent.SetFormState();

                UpdateAllButtonsSelectedStatus(image.Bill.StreamDeckButtonName);

                if (image.Bill.IsSelected)
                {
                    StreamDeckButtonInstance = image.Bill.Button;
                    if (StreamDeckButtonInstance != null)
                    {
                        SDUIParent.ActionPanel.ShowActionConfiguration(StreamDeckButtonInstance);
                        SDUIParent.FacePanel.ShowFaceConfiguration(StreamDeckButtonInstance);
                    }
                }

                CheckIfNoneSelected();
                SDUIParent.SetFormState();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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

                SDUIParent.FacePanel.SetButton(SelectedButtonName);

                var image = (StreamDeckImage)sender;

                if (SelectedButtonName != image.Bill.StreamDeckButtonName && (SDUIParent.ActionPanel.IsDirty || SDUIParent.FacePanel.IsDirty))
                {
                    if (MessageBox.Show("Discard Changes to " + SelectedButtonName + " ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        SDUIParent.ActionPanel.Clear();
                        SDUIParent.FacePanel.Clear();
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        protected void CheckIfNoneSelected()
        {
            if (!ButtonImages.Exists(o => o.Bill.IsSelected == true))
            {
                SDUIParent.ActionPanel.Clear();
                SDUIParent.FacePanel.Clear();
            }
        }

        public int SelectedButtonNumber => SelectedImageBill?.ButtonNumber() ?? 0;

        public StreamDeckButton StreamDeckButton => StreamDeckButtonInstance;

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

        public BillStreamDeckFace SelectedImageBill
        {
            get
            {
                foreach (var image in ButtonImages)
                {
                    if (image.Bill.IsSelected)
                    {
                        return image.Bill;
                    }
                }
                return null;
            }
            
        }

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
                buttonImage.Source = buttonImage.Bill.DeselectedImage;
            }
        }

        protected void Copy()
        {
            var streamDeckButton = StreamDeckPanel.GetInstance(SDUIParent.GetStreamDeckInstanceId()).GetActiveLayer().GetStreamDeckButton(SelectedButtonName);
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
            var oldStreamDeckButton = StreamDeckPanel.GetInstance(SDUIParent.GetStreamDeckInstanceId()).GetActiveLayer().GetStreamDeckButton(SelectedButtonName);
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
                SDUIParent.ChildChangesMade();
            }
            return result;
        }

        public void Refresh()
        {
            try
            {
                HideAllDotImages();
                UnSelect();
                var selectedLayer = StreamDeckPanelInstance.GetLayer(SDUIParent.GetUISelectedLayer().Name);

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
                Common.ShowErrorMessageBox( ex);
            }
        }

        public StreamDeckButton PastedStreamDeckButton
        {
            get => _pastedStreamDeckButton;
            set => _pastedStreamDeckButton = value;
        }
    }
}
