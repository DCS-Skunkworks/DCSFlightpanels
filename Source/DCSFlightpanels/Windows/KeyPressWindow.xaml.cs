namespace DCSFlightpanels.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using ClassLibraryCommon;

    using MEF;

    using NonVisuals;
    using NonVisuals.Interfaces;

    /// <summary>
    /// Interaction logic for KeyPressWindow.xaml
    /// </summary>
    public partial class KeyPressWindow : Window, IIsDirty
    {
        private bool _isDirty;
        private bool _supportIndefinite;

        public KeyPressWindow(bool supportIndefinite = true)
        {
            InitializeComponent();
            SetFormState();
            TextBoxKeyPress.Focus();
            ComboBoxBreak.SelectedIndex = 0;
            ComboBoxKeyPressTime.SelectedIndex = 0;
            _supportIndefinite = supportIndefinite;
        }

        public KeyPressWindow(KeyPressInfo keyPressInfo, bool supportIndefinite = true)
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
                if (((KeyPressLength)ComboBoxKeyPressTime.SelectedItem) == KeyPressLength.Indefinite && !_supportIndefinite)
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

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);
                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.ThirtyTwoMilliSec;
                }

                var keyPressed = KeyInterop.VirtualKeyFromKey(e.SystemKey == Key.F10 ? Key.F10 : e.Key);
                e.Handled = true;

                var hashSetOfKeysPressed = new HashSet<string>();
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyPressed));

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
                SetIsDirty();
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
    }
}
