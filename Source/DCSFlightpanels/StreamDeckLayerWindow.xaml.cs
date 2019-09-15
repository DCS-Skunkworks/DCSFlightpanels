using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ClassLibraryCommon;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for StreamDeckLayerWindow.xaml
    /// </summary>
    public partial class StreamDeckLayerWindow : Window
    {
        private List<string> _existingLayers = new List<string>();
        private bool _loaded = false;
        private string _layerName = "";
        private const int _minimumLength = 3;

        public StreamDeckLayerWindow(List<string> existingLayers)
        {
            InitializeComponent();
            if (existingLayers != null)
            {
                _existingLayers = existingLayers;
            }
        }

        private void StreamDeckLayerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _loaded = true;
                TextBoxLayerName.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(556, ex);
            }
        }

        private void SetFormState()
        {
            try
            {
                TextBoxLayerName.Text = TextBoxLayerName.Text.Replace("|", "").Replace("{", "").Replace("}", "");
                ButtonOk.IsEnabled = TextBoxLayerName.Text.Length >= _minimumLength;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(558, ex);
            }
        }

        private void TextBoxLayerName_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(558, ex);
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(557, ex);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var existingLayer in _existingLayers)
                {
                    if (existingLayer == TextBoxLayerName.Text)
                    {
                        throw new Exception("Layer with name " + TextBoxLayerName.Text + " already exists.");
                    }
                }

                if (TextBoxLayerName.Text.Length < _minimumLength)
                {
                    throw new Exception("Layer name " + TextBoxLayerName.Text + " too short. Minimum length is " + _minimumLength + ".");
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(555, ex);
            }
        }

        public string LayerName => _layerName;
    }
}
