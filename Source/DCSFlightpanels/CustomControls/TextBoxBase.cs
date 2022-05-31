namespace DCSFlightpanels.CustomControls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using ClassLibraryCommon;

    using DCSFlightpanels.Bills;


    public abstract class TextBoxBaseInput : TextBox
    {
        public BillBaseInput Bill { get; set; }

        protected abstract BillBaseInput GetBill { get; }

        protected TextBoxBaseInput()
        {
            PreviewKeyDown += TextBox_PreviewKeyDown;
            MouseDoubleClick += TextBoxMouseDoubleClick;
            MouseDown += TextBox_OnMouseDown;
            GotFocus += TextBoxGotFocus;
            LostFocus += TextBoxLostFocus;
            TextChanged += TextBoxTextChanged;
        }
        
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = (TextBoxBaseInput)sender;
                if (textBox.ContextMenu == null)
                {
                    return;
                }

                if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    textBox.ContextMenu.IsOpen = true;
                    ((ContextMenuPanelTextBox)textBox.ContextMenu).SetVisibility(
                        textBox.GetBill.IsEmpty(),
                        textBox.GetBill.ContainsKeyStroke(),
                        textBox.GetBill.ContainsKeySequence(),
                        textBox.GetBill.ContainsDCSBIOS(),
                        textBox.GetBill.ContainsBIPLink(),
                        textBox.GetBill.ContainsOSCommand());
                    ((ContextMenuPanelTextBox)textBox.ContextMenu).OpenCopySubMenuItem();
                }
                else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    var dummy = 0;
                    var menuItemVisibilities = ((ContextMenuPanelTextBox)textBox.ContextMenu).GetVisibility(
                        ref dummy,
                        textBox.GetBill.IsEmpty(), 
                        textBox.GetBill.ContainsKeyStroke(), 
                        textBox.GetBill.ContainsKeySequence(), 
                        textBox.GetBill.ContainsDCSBIOS(), 
                        textBox.GetBill.ContainsBIPLink(), 
                        textBox.GetBill.ContainsOSCommand());
                    if (menuItemVisibilities.PasteVisible)
                    {
                        textBox.GetBill.Paste();
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBox = (TextBoxBaseInput)sender;

                if (textBox.GetBill == null)
                {
                    return;
                }

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (textBox.GetBill.IsEmpty() || textBox.GetBill.ContainsKeyStroke() || string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.GetBill.EditKeyStroke();
                    }
                    else if (textBox.GetBill.ContainsKeySequence())
                    {
                        textBox.GetBill.EditKeySequence();
                    }
                    else if (textBox.GetBill.ContainsDCSBIOS())
                    {
                        textBox.GetBill.EditDCSBIOS();
                    }
                    else if (textBox.GetBill.ContainsOSCommand())
                    {
                        textBox.GetBill.EditOSCommand();
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = (TextBoxBaseInput)sender;
                if (textBox.GetBill != null && textBox.GetBill.ContainsBIPLink())
                {
                    Background = Brushes.Bisque;
                }
                else
                {
                    Background = Brushes.White;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var textBox = (TextBoxBaseInput)sender;

                if (textBox.GetBill != null && textBox.GetBill.ContainsKeySequence())
                {
                    textBox.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    textBox.FontStyle = FontStyles.Normal;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}