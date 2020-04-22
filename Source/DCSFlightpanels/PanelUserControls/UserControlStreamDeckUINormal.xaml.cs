using System;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Interfaces;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckNormal.xaml
    /// </summary>
    public partial class UserControlStreamDeckUINormal : UserControlStreamDeckUIBase, IStreamDeckUI
    {
        public UserControlStreamDeckUINormal()
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckNormal_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!UserControlLoaded)
            {
                FillControlLists();
                SetImageBills();
                ShowGraphicConfiguration();
                SetContextMenus();
                UserControlLoaded = true;
            }
            SetFormState();
        }

        protected override void SetFormState()
        {
            try
            {
                var selectedButtonNumber = SelectedButtonNumber;
                SetButtonGridStatus(StreamDeckPanel.GetInstance(SDUIParent.GetStreamDeckInstanceId()).HasLayers);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void FillControlLists()
        {
            ButtonImages.Add(ButtonImage1);
            ButtonImages.Add(ButtonImage2);
            ButtonImages.Add(ButtonImage3);
            ButtonImages.Add(ButtonImage4);
            ButtonImages.Add(ButtonImage5);
            ButtonImages.Add(ButtonImage6);
            ButtonImages.Add(ButtonImage7);
            ButtonImages.Add(ButtonImage8);
            ButtonImages.Add(ButtonImage9);
            ButtonImages.Add(ButtonImage10);
            ButtonImages.Add(ButtonImage11);
            ButtonImages.Add(ButtonImage12);
            ButtonImages.Add(ButtonImage13);
            ButtonImages.Add(ButtonImage14);
            ButtonImages.Add(ButtonImage15);

            DotImages.Add(DotImage1);
            DotImages.Add(DotImage2);
            DotImages.Add(DotImage3);
            DotImages.Add(DotImage4);
            DotImages.Add(DotImage5);
            DotImages.Add(DotImage6);
            DotImages.Add(DotImage7);
            DotImages.Add(DotImage8);
            DotImages.Add(DotImage9);
            DotImages.Add(DotImage10);
            DotImages.Add(DotImage11);
            DotImages.Add(DotImage12);
            DotImages.Add(DotImage13);
            DotImages.Add(DotImage14);
            DotImages.Add(DotImage15);
        }




    }
}
