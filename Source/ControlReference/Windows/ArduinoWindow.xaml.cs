using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ClassLibraryCommon;
using ControlReference.CustomControls;
using DCS_BIOS;
using DCS_BIOS.Json;
using Cursors = System.Windows.Input.Cursors;
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

                var arduinoStrings = DCSBIOSArduinoInformation.GetInformation(_dcsbiosControl);
                
                foreach (var str in arduinoStrings)
                {
                    
                    var textBlock = new TextBlockSelectable(str);
                    textBlock.MouseEnter += TextBlock_OnMouseEnter;
                    textBlock.MouseLeave += TextBlock_OnMouseLeave;
                    SetContextMenu(textBlock);

                    textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                    textBlock.Width = Double.NaN;
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



        private void TextBlock_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.IBeam;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void TextBlock_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }


        private void SetContextMenu(TextBlockSelectable textBlock)
        {
            try
            {
                //_contextMenu.Opened += TextBlockContextMenuOpened;
                ContextMenu contextMenu = new();
                contextMenu.Opened += TextBlock_ContextMenuOpened;
                contextMenu.Tag = textBlock;
                var menuItemCopy = new MenuItem();
                menuItemCopy.Tag = textBlock;
                menuItemCopy.Header = "Copy";
                menuItemCopy.Click += MenuItemCopy_OnClick;
                contextMenu.Items.Add(menuItemCopy);
                textBlock.ContextMenu = contextMenu;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }
        
        private void TextBlock_ContextMenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                var contextMenu = (ContextMenu) sender;
                var textBlock = (TextBlockSelectable)contextMenu.Tag;
                ((MenuItem)contextMenu.Items[0]).IsEnabled = !string.IsNullOrEmpty(textBlock.SelectedText);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBlock = ((MenuItem)sender).Tag;
                CopyToClipboard((TextBlockSelectable)textBlock);
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void CopyToClipboard(TextBlockSelectable textBlock)
        {
            if(string.IsNullOrEmpty(textBlock.SelectedText)) textBlock.SelectAll();

            Clipboard.SetText(textBlock.SelectedText ?? "");
            SystemSounds.Asterisk.Play();
        }
    }
}
