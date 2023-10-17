using System.Diagnostics;
using System.Linq;

namespace DCSFlightpanels.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using MEF;
    using NonVisuals.Interfaces;
    using NonVisuals.KeyEmulation;
    

    /// <summary>
    /// Interaction logic for KeyPressWindow.xaml
    /// </summary>
    public partial class KeyPressReadingBigWindow : IIsDirty
    {
        private bool _isDirty;
        private readonly bool _supportIndefinite;
        private bool _doOpenRecording;

        public KeyPressReadingBigWindow(bool supportIndefinite = true)
        {
            InitializeComponent();
            SetFormState();
            TextBoxKeyPress.Focus();
            ComboBoxBreak.SelectedIndex = 0;
            ComboBoxKeyPressTime.SelectedIndex = 0;
            _supportIndefinite = supportIndefinite;
        }

        public KeyPressReadingBigWindow(KeyPressInfo keyPressInfo, bool supportIndefinite = true)
        {
            InitializeComponent();
            //comboBoxBreak.ItemsSource = Enum.GetValues(typeof(KeyPressLength));
            ComboBoxBreak.SelectedItem = keyPressInfo.LengthOfBreak;
            TextBoxKeyPress.Text = keyPressInfo.VirtualKeyCodesAsString;
            ComboBoxKeyPressTime.SelectedItem = keyPressInfo.LengthOfKeyPress;
            _supportIndefinite = supportIndefinite;
            SetFormState();
            TextBoxKeyPress.Focus();
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((KeyPressLength)ComboBoxKeyPressTime.SelectedItem == KeyPressLength.Indefinite && !_supportIndefinite)
                {
                    MessageBox.Show("Indefinite is not supported for this device.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
        
        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    ((TextBox)sender).Text = string.Empty;
                    ((TextBox)sender).Tag = null;
                    SetIsDirty();
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
                SetIsDirty();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void KeyPressWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        private void ButtonAddNullKey_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxKeyPress.Text = "VK_NULL";
                SetFormState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = DarkMode.TextBoxSelectedBackgroundColor;
                SetFormState();
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
                ((TextBox)sender).Background = DarkMode.TextBoxUnselectedBackgroundColor;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelOpenRecording_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/DCSFlightpanels/DCSFlightpanels/wiki/Open-Recording-for-Key-Presses",
                UseShellExecute = true
            });
        }

        private void CheckBoxOpenRecording_OnChecked(object sender, RoutedEventArgs e)
        {
            _doOpenRecording = true;
            TextBoxKeyPress.Focus();
        }

        private void CheckBoxOpenRecording_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _doOpenRecording = false;
            TextBoxKeyPress.Focus();
        }
        
        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (_doOpenRecording)
                {
                    TextBoxOpenRecording(sender, e);
                    return;
                }

                var textBox = (TextBox)sender;

                var hashSetOfKeysPressed = new HashSet<string>();

                var keyCode = KeyInterop.VirtualKeyFromKey(e.RealKey());

                e.Handled = true;

                if (keyCode > 0)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyCode));
                }
                var modifiers = CommonVirtualKey.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
                var result = string.Empty;
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

                result = Common.RemoveLControl(result);

                textBox.Text = result;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxOpenRecording(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = true;
                var textBox = (TextBox)sender;

                var hashSetOfKeysPressed = new HashSet<string>();

                var modifiers = CommonVirtualKey.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }

                if (hashSetOfKeysPressed.Any())
                {
                    foreach (var virtualKeyCode in modifiers)
                    {
                        AddKey(textBox, Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                    }
                }
                else
                {
                    var keyCode = KeyInterop.VirtualKeyFromKey(e.RealKey());

                    if (keyCode > 0)
                    {
                        AddKey(textBox, Enum.GetName(typeof(VirtualKeyCode), keyCode));
                    }
                }

                textBox.Text = Common.RemoveLControl(textBox.Text);
                SetFormState();

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void AddKey(TextBox textBox, string key)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                Debug.WriteLine("Setting text");
                textBox.Text = key;
            }
            else
            {
                Debug.WriteLine($@"Appending text {textBox.Text}");
                textBox.Text += " + " + key;
            }
        }

        private void LabelOpenRecording_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void LabelOpenRecording_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElementDarkMode(this);
        }
    }
}
