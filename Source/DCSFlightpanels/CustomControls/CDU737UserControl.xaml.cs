using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DCSFlightpanels.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour CDU737UserControl.xaml
    /// </summary>
    public partial class CDU737UserControl : UserControl
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
