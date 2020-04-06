using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals;
using NonVisuals.Interfaces;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for OSCommandWindow.xaml
    /// </summary>
    public partial class OSCommandWindow : Window, IIsDirty
    {
        private bool _isLoaded = false;
        private OSCommand _osCommand;
        private bool _isDirty;

        public OSCommandWindow()
        {
            InitializeComponent();
        }

        public OSCommandWindow(OSCommand osCommand)
        {
            InitializeComponent();
            _osCommand = osCommand;
            TextBoxName.Text = _osCommand.Name;
            TextBoxCommand.Text = _osCommand.Command;
            TextBoxArguments.Text = _osCommand.Arguments;
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
                Common.ShowErrorMessageBox(23060, ex);
            }
        }

        private void SetFormState()
        {
            try
            {
                ButtonOk.IsEnabled = TextBoxCommand.Text.Length > 6;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(23060, ex);
            }
        }

        private void TextBoxCommand_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                SetFormState();
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(23060, ex);
            }
        }

        private void ButtonTest_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var osCommand = new OSCommand(TextBoxCommand.Text, TextBoxArguments.Text, "");
                TextBoxResult.Text = osCommand.Execute(new CancellationToken());
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(23060, ex);
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
                Common.ShowErrorMessageBox(23060, ex);
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
                Common.ShowErrorMessageBox(23060, ex);
            }
        }

        public OSCommand OSCommandObject
        {
            get => _osCommand;
            set => _osCommand = value;
        }

        private void TextBoxArguments_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                SetFormState();
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(23060, ex);
            }
        }

        private void TextBoxName_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                SetFormState();
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(23060, ex);
            }
        }

        public bool IsDirty => _isDirty;

        public void SetIsDirty()
        {
            _isDirty = true;
        }
    }
}
