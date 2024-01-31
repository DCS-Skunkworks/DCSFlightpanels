using System.Linq;
using System.Windows;
using ClassLibraryCommon;
using DCSFlightpanels.CustomControls;
using NonVisuals.Panels.StreamDeck.Panels;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckUIPlus.xaml
    /// </summary>
    public partial class UserControlStreamDeckUIPlus
    {
        public UserControlStreamDeckUIPlus(StreamDeckPanel streamDeckPanel) : base(streamDeckPanel)
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckUIPlus_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!UserControlLoaded)
            {
                FillControlLists();
                SetImageEnvironment();
                ShowGraphicConfiguration();
                SetContextMenus();
                UserControlLoaded = true;
            }
            SetFormState();
        }

        protected override int ButtonAmount()
        {
            return 8;
        }
        protected override int ButtonPushRotaryAmount()
        {
            return 4;
        }

        private void FillControlLists()
        {
            Common.FindVisualChildren<StreamDeckImage>(GridButtons).ToList()
                .ForEach(x => ButtonImages.Add(x));

            Common.FindVisualChildren<StreamDeckPushRotaryCtrl>(GridButtons).ToList()
                .ForEach(x => ButtonPushRotary.Add(x));

            CheckButtonControlListValidity();

           SetRotariesImageVisibility();
        }

        private void SetRotariesImageVisibility()
        {
            StreamDeckPushRotary1.CCW.Visibility = Visibility.Visible;
            StreamDeckPushRotary2.Push.Visibility = Visibility.Visible;
            StreamDeckPushRotary3.CW.Visibility = Visibility.Visible;

            StreamDeckPushRotary4.CCW.Visibility = Visibility.Visible;
            StreamDeckPushRotary5.Push.Visibility = Visibility.Visible;
            StreamDeckPushRotary6.CW.Visibility = Visibility.Visible;

            StreamDeckPushRotary7.CCW.Visibility = Visibility.Visible;
            StreamDeckPushRotary8.Push.Visibility = Visibility.Visible;
            StreamDeckPushRotary9.CW.Visibility = Visibility.Visible;

            StreamDeckPushRotary10.CCW.Visibility = Visibility.Visible;
            StreamDeckPushRotary11.Push.Visibility = Visibility.Visible;
            StreamDeckPushRotary12.CW.Visibility = Visibility.Visible;
        }
    }
}
