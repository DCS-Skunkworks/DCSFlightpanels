namespace DCSFlightpanels.Windows
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using NonVisuals;
    using NonVisuals.Interfaces;

    /// <summary>
    /// Interaction logic for OSCommandWindow.xaml
    /// </summary>
    public partial class OSCommandWindow : Window, IIsDirty
    {
        private bool _isLoaded = false;
        private OSCommand _operatingSystemCommand;
        private bool _isDirty;

        public OSCommandWindow()
        {
            InitializeComponent();
        }

        public OSCommandWindow(OSCommand operatingSystemCommand)
        {
            InitializeComponent();
            _operatingSystemCommand = operatingSystemCommand;
            TextBoxName.Text = _operatingSystemCommand.Name;
            TextBoxCommand.Text = _operatingSystemCommand.Command;
            TextBoxArguments.Text = _operatingSystemCommand.Arguments;
        }

        private void OSCommandWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                SetFormState();
                _isLoaded = true;
                TextBoxName.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void SetFormState()
        {
            try
            {
                ButtonOk.IsEnabled = IsDirty && !string.IsNullOrEmpty(TextBoxCommand.Text);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }
        
        private void ButtonTest_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var operatingSystemCommand = new OSCommand(TextBoxCommand.Text, TextBoxArguments.Text, string.Empty);
                TextBoxResult.Text = operatingSystemCommand.Execute(new CancellationToken());
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OSCommandObject = new OSCommand(TextBoxCommand.Text, TextBoxArguments.Text, TextBoxName.Text);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public OSCommand OSCommandObject
        {
            get => _operatingSystemCommand;
            set => _operatingSystemCommand = value;
        }

        private void TextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded || e.Key == Key.Escape)
                {
                    return;
                }
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public bool IsDirty => _isDirty;

        public void SetIsDirty()
        {
            _isDirty = true;
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        private void OSCommandWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }
    }
}
