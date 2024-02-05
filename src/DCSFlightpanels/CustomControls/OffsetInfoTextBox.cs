using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NonVisuals;

namespace DCSFlightpanels.CustomControls
{
    public class OffsetInfoTextBox : TextBox
    {
        private int _offSetX = SettingsManager.OffsetX;
        private int _offSetY = SettingsManager.OffsetY;

        public OffsetInfoTextBox()
        {
            Height = 45;
            Width = 65;
            IsReadOnly = true;
            FontSize = 10;
            FontStyle = FontStyles.Italic;
            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#f2f5f3");
        }

        private void UpdateOffsetInfo()
        {
            Text = "OffSetX : " + OffSetX + "\nOffSetY : " + OffSetY;
        }

        public int OffSetX
        {
            get => _offSetX;
            set
            {
                _offSetX = value;
                UpdateOffsetInfo();
            }
        }

        public int OffSetY
        {
            get => _offSetY;
            set
            {
                _offSetY = value;
                UpdateOffsetInfo();
            }
        }
    }
}
