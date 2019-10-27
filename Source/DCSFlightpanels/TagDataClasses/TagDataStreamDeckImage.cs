using System.Drawing;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.TagDataClasses
{
    public class TagDataStreamDeckImage
    {
        public StreamDeckButtonNames StreamDeckButtonName;
        public StreamDeckButton Button;
        public Font TextFont { get; set; } = new Font("Consolas", 10);
        public Color FontColor { get; set; } = Color.Black;
        public Color BackgroundColor { get; set; } = Color.White;
        public bool IsSelected { get; set; } = false;
        public int OffsetY { get; set; } = 0;
        public int OffsetX { get; set; } = 0;

        public int ButtonNumber()
        {
            if (StreamDeckButtonName == StreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return 0;
            }
            
            return int.Parse(StreamDeckButtonName.ToString().Replace("BUTTON", ""));
            
        }

        public void Clear()
        {
            Button = null;
        }
    }

}
