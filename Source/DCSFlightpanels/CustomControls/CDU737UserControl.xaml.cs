using System.Windows.Controls;

namespace DCSFlightpanels.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour CDU737UserControl.xaml
    /// </summary>
    public partial class CDU737UserControl
    {
        public CDU737UserControl()
        {
            InitializeComponent();
        }

        public void displayLines(string[] lines, int number)
        {
            CDULines.Text = "";
            for (int i = 0; i < number; i++)
            {
                CDULines.Text += lines[i] + '\n';
            }
        }

    }
}
