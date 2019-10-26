using NonVisuals.StreamDeck;

namespace DCSFlightpanels.TagDataClasses
{
    public class TagDataClassButtonImage
    {
        public StreamDeckButtonNames StreamDeckButtonName;
        public StreamDeckButton Button;

        public bool IsSelected { get; set; } = false;

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
