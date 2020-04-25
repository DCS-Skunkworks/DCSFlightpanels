using System;
using System.Windows;
using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckNormal.xaml
    /// </summary>
    public partial class UserControlStreamDeckUIXL : UserControlStreamDeckUIBase, IStreamDeckUI
    {

        public UserControlStreamDeckUIXL():base()
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
            ButtonImages.Add(ButtonImage16);
            ButtonImages.Add(ButtonImage17);
            ButtonImages.Add(ButtonImage18);
            ButtonImages.Add(ButtonImage19);
            ButtonImages.Add(ButtonImage20);
            ButtonImages.Add(ButtonImage21);
            ButtonImages.Add(ButtonImage22);
            ButtonImages.Add(ButtonImage23);
            ButtonImages.Add(ButtonImage24);
            ButtonImages.Add(ButtonImage25);
            ButtonImages.Add(ButtonImage26);
            ButtonImages.Add(ButtonImage27);
            ButtonImages.Add(ButtonImage28);
            ButtonImages.Add(ButtonImage29);
            ButtonImages.Add(ButtonImage30);
            ButtonImages.Add(ButtonImage31);
            ButtonImages.Add(ButtonImage32);

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
            DotImages.Add(DotImage16);
            DotImages.Add(DotImage17);
            DotImages.Add(DotImage18);
            DotImages.Add(DotImage19);
            DotImages.Add(DotImage20);
            DotImages.Add(DotImage21);
            DotImages.Add(DotImage22);
            DotImages.Add(DotImage23);
            DotImages.Add(DotImage24);
            DotImages.Add(DotImage25);
            DotImages.Add(DotImage26);
            DotImages.Add(DotImage27);
            DotImages.Add(DotImage28);
            DotImages.Add(DotImage29);
            DotImages.Add(DotImage30);
            DotImages.Add(DotImage31);
            DotImages.Add(DotImage32);
        }
    }
}
