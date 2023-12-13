using NonVisuals.Panels.StreamDeck.Panels;
using NonVisuals.Panels.StreamDeck;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using System.Drawing;
using System.Windows.Media;

namespace DCSFlightpanels.CustomControls
{
    /// <summary>
    /// Interaction logic for StreamDeckPushRotaryCtrl.xaml
    /// </summary>
    public partial class StreamDeckPushRotaryCtrl : UserControl
    {
        private bool _isSelected;
        public StreamDeckPanel StreamDeckPanelInstance { get; set; }
        public StreamDeckPushRotary StreamDeckPushRotary { get; set; } = new();

        public StreamDeckPushRotaryCtrl()
        {
            InitializeComponent();
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;

                GridBack.Background = _isSelected ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.DimGray);
            }
        }
    }
}
