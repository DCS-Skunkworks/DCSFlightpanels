using NonVisuals.StreamDeck;

namespace DCSFlightpanels.TagDataClasses
{
    public class TagDataClassButtonImage
    {
        public StreamDeckButtonNames StreamDeckButtonName;

        public bool IsSelected { get; set; } = false;

        public int ButtonNumber()
        {
            return int.Parse(StreamDeckButtonName.ToString().Replace("BUTTON",""));
        }
    }

}
