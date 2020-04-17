using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.Interfaces
{
    interface IFontFace
    {
        Font TextFont { get; set; }
        Color FontColor { get; set; }
        Color BackgroundColor { get; set; }
    }
}
