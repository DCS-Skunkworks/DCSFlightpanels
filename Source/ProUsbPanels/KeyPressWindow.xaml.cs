using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NonVisuals;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for KeyPressWindow.xaml
    /// </summary>
    public partial class KeyPressWindow : Window
    {
        private bool _isDirty;

        public KeyPressWindow()
        {
            InitializeComponent();
            SetFormState();
            TextBoxKeyPress.Focus();
            ComboBoxBreak.SelectedIndex = 0;
            ComboBoxKeyPressTime.SelectedIndex = 0;
        }

        public KeyPressWindow(KeyPressInfo keyPressInfo)
        {
            InitializeComponent();
            //comboBoxBreak.ItemsSource = Enum.GetValues(typeof(KeyPressLength));
            ComboBoxBreak.SelectedItem = keyPressInfo.LengthOfBreak;
            TextBoxKeyPress.Text = keyPressInfo.VirtualKeyCodesAsString;
            ComboBoxKeyPressTime.SelectedItem = keyPressInfo.LengthOfKeyPress;
            SetFormState();
            TextBoxKeyPress.Focus();
        }
        
        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);
                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }
                var keyPressed = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(e.Key);
                e.Handled = true;

                var hashSetOfKeysPressed = new HashSet<string>();
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyPressed));

                var modifiers = Common.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
                var result = "";
                foreach (var str in hashSetOfKeysPressed)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result = str + " + " + result;
                    }
                    else
                    {
                        result = str + " " + result;
                    }
                }
                textBox.Text = result;
                _isDirty = true;
                SetFormState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    ((TextBox)sender).Text = "";
                    ((TextBox)sender).Tag = null;
                    _isDirty = true;
                    SetFormState();
                }
                else if (e.ChangedButton == MouseButton.Left)
                {
                    //((TextBox) sender).ContextMenu.sh
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetFormState()
        {
            ButtonOk.IsEnabled = !string.IsNullOrEmpty(TextBoxKeyPress.Text);
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _isDirty = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
