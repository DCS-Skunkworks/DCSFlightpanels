using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    public abstract class UserControlStreamDeckUIBase : UserControl
    {
        protected IStreamDeckUIParent SDUIParent;
        protected readonly List<StreamDeckImage> ButtonImages = new List<StreamDeckImage>();
        protected readonly List<System.Windows.Controls.Image> DotImages = new List<System.Windows.Controls.Image>();
        protected bool UserControlLoaded;
        protected StreamDeckButton StreamDeckButtonInstance;


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

        public void Clear()
        {

            HideAllDotImages();

            foreach (var buttonImage in ButtonImages)
            {
                buttonImage.Bill.Clear();
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
                Common.ShowErrorMessageBox(993013, ex);
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
                Common.ShowErrorMessageBox(20135444, ex);
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
                Common.ShowErrorMessageBox(20135444, ex);
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

    }
}
