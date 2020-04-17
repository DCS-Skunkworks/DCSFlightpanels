using System.Drawing;

namespace NonVisuals.Interfaces
{
    interface IFontFace
    {
        Font TextFont { get; set; }
        Color FontColor { get; set; }
        Color BackgroundColor { get; set; }
    }
}
