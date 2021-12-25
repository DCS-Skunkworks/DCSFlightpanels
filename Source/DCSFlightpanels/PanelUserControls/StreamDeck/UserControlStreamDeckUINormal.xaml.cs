using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DCSFlightpanels.CustomControls;
using NonVisuals.StreamDeck;

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
            FindVisualChildren<StreamDeckImage>(GridButtons).ToList()
                .ForEach(x => ButtonImages.Add(x));
            
            FindVisualChildren<Image>(GridButtons).ToList().Where(x => x.Name.StartsWith("DotImage")).ToList()
                .ForEach(x => DotImages.Add(x));

            CheckButonControlListValidity();
        }
    }
}
