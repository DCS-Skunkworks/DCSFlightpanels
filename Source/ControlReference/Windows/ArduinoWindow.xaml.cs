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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing;
using System.Windows.Media.TextFormatting;

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

                var arduinoStrings = DCSBIOSArduinoInformation.GetInformation(_dcsbiosControl);
                
                foreach (var str in arduinoStrings)
                {
                    
                    var textBlock = new TextBlockSelectable(str);
                    textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                    if (str == "Input" || str == "Output")
                    {
                        textBlock.FontWeight = FontWeights.Bold;
                    }
                    var border = new Border();
                    border.Child = textBlock;
                    StackPanelArduinoInfo.Children.Add(border);
                    StackPanelArduinoInfo.Children.Add(new Line());
                }
                
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
    }
}
