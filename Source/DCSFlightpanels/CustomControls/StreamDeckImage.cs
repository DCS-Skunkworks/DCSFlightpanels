using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class StreamDeckImage : Image
    {
        private bool _isSelected;

        public BillStreamDeckFace Bill { get; set; }
        public bool IsSelected {
            get { 
                return _isSelected;           
            }
            set {
                _isSelected = value;
                Border borderParent = Common.FindVisualParent<Border>(this);
                borderParent.Style = _isSelected ? (Style)FindResource("BorderSelected") : (Style)FindResource("BorderMouseHoover");
            }
        }

        public void SetDefaultButtonImage()
        {
            Source = PanelUserControls.StreamDeck.Resources.GetDefaultButtonImageNamed(Bill.StreamDeckButtonName);
        }
    } 
}
