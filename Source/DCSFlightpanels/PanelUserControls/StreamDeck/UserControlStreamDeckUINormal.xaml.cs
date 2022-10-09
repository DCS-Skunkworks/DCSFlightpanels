using System.Linq;
using System.Windows;
using ClassLibraryCommon;
using DCSFlightpanels.CustomControls;
using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckUINormal.xaml
    /// </summary>
    public partial class UserControlStreamDeckUINormal : UserControlStreamDeckUIBase
    {
        public UserControlStreamDeckUINormal(StreamDeckPanel streamDeckPanel) : base(streamDeckPanel)
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

        protected override int ButtonAmount()
        {
            return 15;
        }

        private void FillControlLists()
        {
            Common.FindVisualChildren<StreamDeckImage>(GridButtons).ToList()
                .ForEach(x => ButtonImages.Add(x));

            CheckButtonControlListValidity();
        }
    }
}
