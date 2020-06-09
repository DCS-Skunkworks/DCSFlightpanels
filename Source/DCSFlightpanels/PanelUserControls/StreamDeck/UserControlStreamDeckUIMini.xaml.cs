using System.Windows;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckUIMini.xaml
    /// </summary>
    public partial class UserControlStreamDeckUIMini : UserControlStreamDeckUIBase
    {
        public UserControlStreamDeckUIMini()
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
            return 15;
        }

        private void FillControlLists()
        {
            ButtonImages.Add(ButtonImage1);
            ButtonImages.Add(ButtonImage2);
            ButtonImages.Add(ButtonImage3);
            ButtonImages.Add(ButtonImage4);
            ButtonImages.Add(ButtonImage5);
            ButtonImages.Add(ButtonImage6);

            DotImages.Add(DotImage1);
            DotImages.Add(DotImage2);
            DotImages.Add(DotImage3);
            DotImages.Add(DotImage4);
            DotImages.Add(DotImage5);
            DotImages.Add(DotImage6);
        }

    }
}
