using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClassLibraryCommon;
using ControlReference.CustomControls;
using DCS_BIOS;
using ControlReference.Properties;
using DCS_BIOS.Json;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace ControlReference.Windows
{
    /// <summary>
    /// Interaction logic for ArduinoWindow.xaml
    /// </summary>
    public partial class ArduinoWindow
    {
        private bool _isLoaded;
        private DCSBIOSControl _dcsbiosControl;
        public ArduinoWindow(DCSBIOSControl dcsbiosControl)
        {
            InitializeComponent();
            _dcsbiosControl = dcsbiosControl;
            LabelControl.Content = _dcsbiosControl.Identifier;
        }

        private void SetFormState()
        {
        }

        private void ArduinoWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }
                
                var textBlock = new TextBlockSelectable("const byte aapCdupwrPins[2] = {PIN_0, PIN_1};\nDcsBios::SwitchMultiPos aapCdupwr(\"AAP_CDUPWR\", aapCdupwrPins, 2);");
                var border = new Border();
                border.Child = textBlock;
                StackPanelArduinoInfo.Children.Add(border);
                StackPanelArduinoInfo.Children.Add(new Line());
                
                var textBlock2 = new TextBlockSelectable("ASDB ASD#rtälakemfå23potjk'åq3omjtgfföslakmf2ol3pkmfr");
                var border2 = new Border();
                border2.Child = textBlock2;
                StackPanelArduinoInfo.Children.Add(border2);
                
                StackPanelArduinoInfo.UpdateLayout();

                SetFormState();
                _isLoaded = true;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }
        
        private void ArduinoWindow_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
