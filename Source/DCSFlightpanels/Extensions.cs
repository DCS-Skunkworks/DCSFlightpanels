using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCSFlightpanels.CustomControls;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels
{
    public static class Extensions
    {

        public static void TestImage(this StreamDeckFaceTextBox textBox, IStreamDeckUIParent sdUIParent)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.Bill.TextFont, textBox.Bill.FontColor, textBox.Bill.BackgroundColor, textBox.Bill.OffsetX, textBox.Bill.OffsetY);
            sdUIParent.TestImage(bitmap);
        }
    }
}
