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
                SetImageBills();
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

        private void FillControlLists()
        {
            Common.FindVisualChildren<StreamDeckImage>(GridButtons).ToList()
                .ForEach(x => ButtonImages.Add(x));

            CheckButtonControlListValidity();
        }
    }
}
