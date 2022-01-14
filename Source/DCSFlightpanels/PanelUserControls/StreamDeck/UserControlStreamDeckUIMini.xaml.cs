using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DCSFlightpanels.CustomControls;
using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckUIMini.xaml
    /// </summary>
    public partial class UserControlStreamDeckUIMini : UserControlStreamDeckUIBase
    {
        public UserControlStreamDeckUIMini(StreamDeckPanel streamDeckPanel) : base(streamDeckPanel)
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckUIMini_OnLoaded(object sender, RoutedEventArgs e)
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
            return 6;
        }

        private void FillControlLists()
        {
            FindVisualChildren<StreamDeckImage>(GridButtons).ToList()
                .ForEach(x => ButtonImages.Add(x));

            FindVisualChildren<Image>(GridButtons).ToList().Where(x => x.Name.StartsWith("DotImage")).ToList()
                .ForEach(x => DotImages.Add(x));

            CheckButtonControlListValidity();
        }
    }
}
