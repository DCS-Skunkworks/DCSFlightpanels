using System.Linq;
using System.Windows;
using ClassLibraryCommon;
using DCSFlightpanels.CustomControls;
using NonVisuals.Panels.StreamDeck.Panels;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckUIXL.xaml
    /// </summary>
    public partial class UserControlStreamDeckUIXL
    {
        public UserControlStreamDeckUIXL(StreamDeckPanel streamDeckPanel) : base(streamDeckPanel)
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckXL_OnLoaded(object sender, RoutedEventArgs e)
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
            return 32;
        }

        private void FillControlLists()
        {
            Common.FindVisualChildren<StreamDeckImage>(GridButtons).ToList()
                      .ForEach(x => ButtonImages.Add(x));

            CheckButtonControlListValidity();
        }
    }
}
