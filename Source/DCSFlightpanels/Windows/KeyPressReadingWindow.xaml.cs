using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using NonVisuals;

namespace DCSFlightpanels.Windows
{
    using MEF;

    /// <summary>
    /// Interaction logic for KeyPressReadingWindow.xaml
    /// </summary>
    public partial class KeyPressReadingWindow : Window
    {
        private bool _isDirty = false;
        private bool _loaded = false;
        private KeyPressLength _keyPressLength;
        private bool _supportIndefinite;

        public KeyPressReadingWindow(bool supportIndefinite = true)
        {
            InitializeComponent();
            ComboBoxPressTimes.SelectedItem = KeyPressLength.ThirtyTwoMilliSec;
            _keyPressLength = KeyPressLength.ThirtyTwoMilliSec;
            _supportIndefinite = supportIndefinite;
        }

        public KeyPressReadingWindow(KeyPressLength keyPressLength, string keyPress, bool supportIndefinite = true)
        {
            InitializeComponent();
            TextBoxKeyPress.Text = keyPress;
            ComboBoxPressTimes.SelectedItem = keyPressLength;
            _keyPressLength = keyPressLength;
            _supportIndefinite = supportIndefinite;
        }

        private void KeyPressReadingWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    return;
                }

                TextBoxKeyPress.Focus();
                _loaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetFormState()
        {
            ButtonDelete.IsEnabled = !string.IsNullOrEmpty(TextBoxKeyPress.Text);
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.Yellow;
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
                ((TextBox)sender).Background = Brushes.White;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;

                var hashSetOfKeysPressed = new HashSet<string>();

                var keyCode = KeyInterop.VirtualKeyFromKey(e.RealKey());

                e.Handled = true;

                if (keyCode > 0)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyCode));
                }
                var modifiers = CommonVK.GetPressedVirtualKeyCodesThatAreModifiers();
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

                result = Common.RemoveRControl(result);

                textBox.Text = result;
                SetFormState();
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
                _isDirty = true;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxKeyPress.Text = string.Empty;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if(_keyPressLength == KeyPressLength.Indefinite && !_supportIndefinite)
                {
                    MessageBox.Show("Indefinite is not supported for this device.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public bool IsDirty => _isDirty;

        private void ComboBoxPressTimes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_loaded)
                {
                    return;
                }
                var tmpKeyPressLength = (KeyPressLength)ComboBoxPressTimes.SelectedItem;
                if (tmpKeyPressLength != _keyPressLength)
                {
                    _keyPressLength = tmpKeyPressLength;
                    _isDirty = true;
                }


                SetFormState();

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public string VirtualKeyCodesAsString
        {
            get => TextBoxKeyPress.Text;
        }

        public KeyPressLength LengthOfKeyPress
        {
            get => _keyPressLength;
        }

        private void KeyPressReadingWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    DialogResult = false;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
