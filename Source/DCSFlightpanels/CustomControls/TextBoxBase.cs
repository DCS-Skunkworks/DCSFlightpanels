using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public abstract class TextBoxBaseStreamDeckInput : TextBox
    {
        public BillBaseInput Bill { get; set; }
    }

    public abstract class TextBoxBaseInput : TextBox
    {
        public BillBaseInput Bill { get; set; }
        protected abstract BillBaseInput GetBill { get; }

        protected TextBoxBaseInput()
        {
            MouseDoubleClick += TextBoxMouseDoubleClick;
            MouseDown += TextBox_OnMouseDown;
            GotFocus += TextBoxGotFocus;
            LostFocus += TextBoxLostFocus;
            TextChanged += TextBoxTextChanged;
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
                    if (textBox.GetBill.IsEmpty() || textBox.GetBill.ContainsSingleKey() || string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.GetBill.EditSingleKeyPress();
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


namespace DCSFlightpanels.CustomControls
{
    public class TextBoxBaseOutput : TextBox
    {
        public BillBaseOutput Bill { get; set; }
    }
}