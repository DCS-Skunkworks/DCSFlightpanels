namespace NonVisuals.Interfaces
{
    using System.Drawing;

    interface IFontFace
    {
        Font TextFont { get; set; }
        Color FontColor { get; set; }
        Color BackgroundColor { get; set; }
    }
}
