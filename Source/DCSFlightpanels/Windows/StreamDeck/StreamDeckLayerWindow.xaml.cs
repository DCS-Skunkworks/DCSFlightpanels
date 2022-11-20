using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals.Panels.StreamDeck;
using NonVisuals.Panels.StreamDeck.Panels;

namespace DCSFlightpanels.Windows.StreamDeck
{
    /// <summary>
    /// Interaction logic for StreamDeckLayerWindow.xaml
    /// </summary>
    public partial class StreamDeckLayerWindow
    {

        private readonly List<StreamDeckLayer> _existingLayers = new();
        private bool _loaded = false;
        private const int MINIMUM_LENGTH = 3;
        private StreamDeckLayer _newLayer = null;
        [NonSerialized]
        private readonly StreamDeckPanel _streamDeckPanel;

        public StreamDeckLayerWindow(List<StreamDeckLayer> existingLayers, StreamDeckPanel streamDeckPanel)
        {
            InitializeComponent();
            if (existingLayers != null)
            {
                _existingLayers = existingLayers;
            }

            _streamDeckPanel = streamDeckPanel;
        }

        private void StreamDeckLayerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_loaded)
                {
                    return;
                }
                TextBoxLayerName.Focus();
                SetFormState();

                _loaded = true;
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
                TextBoxLayerName.Text = TextBoxLayerName.Text.Replace("|", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Replace("*", string.Empty);
                ButtonOk.IsEnabled = TextBoxLayerName.Text.Length >= MINIMUM_LENGTH;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonOkChecks();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonOkChecks()
        {
            foreach (var existingLayer in _existingLayers)
            {
                if (existingLayer.Name == TextBoxLayerName.Text)
                {
                    throw new Exception("Layer with name " + TextBoxLayerName.Text + " already exists.");
                }
            }

            if (TextBoxLayerName.Text.Length < MINIMUM_LENGTH)
            {
                throw new Exception("Layer name " + TextBoxLayerName.Text + " too short. Minimum length is " + MINIMUM_LENGTH + ".");
            }

            _newLayer = new StreamDeckLayer(_streamDeckPanel)
            {
                Name = TextBoxLayerName.Text
            };
            DialogResult = true;
            Close();
        }

        public StreamDeckLayer NewLayer => _newLayer;

        private void StreamDeckLayerWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
                {
                    DialogResult = false;
                    e.Handled = true;
                    Close();
                }

                if (ButtonOk.IsEnabled && e.Key == Key.Enter)
                {
                    ButtonOkChecks();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }

        }
    }
}
