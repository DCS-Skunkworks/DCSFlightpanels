using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using MEF;
using NonVisuals.Panels.StreamDeck.Panels;
using NonVisuals.Panels.StreamDeck;
using NonVisuals;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace DCSFlightpanels.CustomControls
{
    public class StreamDeckImage : Image
    {
        private bool _isSelected;
        public EnumStreamDeckButtonNames StreamDeckButtonName;
        public StreamDeckButton Button;
        private Color _backgroundColor = SettingsManager.DefaultBackgroundColor;

        public string BackgroundHex
        {
            get { return $"#{_backgroundColor.R:X2}{_backgroundColor.G:X2}{_backgroundColor.B:X2}"; }
        }

        public StreamDeckPanel StreamDeckPanelInstance { get; set; }

        public int ButtonNumber()
        {
            if (StreamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return 0;
            }
            return int.Parse(StreamDeckButtonName.ToString().Replace("BUTTON", string.Empty));
        }
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
            Source = PanelUserControls.StreamDeck.Resources.GetDefaultButtonImageNamed(StreamDeckButtonName);
        }
        
        public void Clear()
        {
            Button = null;
            _backgroundColor = SettingsManager.DefaultBackgroundColor;
        }
    } 
}
