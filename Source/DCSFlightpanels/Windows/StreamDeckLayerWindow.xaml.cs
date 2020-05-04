using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for StreamDeckLayerWindow.xaml
    /// </summary>
    public partial class StreamDeckLayerWindow : Window
    {

        private List<StreamDeckLayer> _existingLayers = new List<StreamDeckLayer>();
        private bool _loaded = false;
        private const int MINIMUM_LENGTH = 3;
        private StreamDeckLayer _newLayer = null;
        private string _panelHash;

        public StreamDeckLayerWindow(List<StreamDeckLayer> existingLayers, string panelHash)
        {
            InitializeComponent();
            if (existingLayers != null)
            {
                _existingLayers = existingLayers;
            }

            _panelHash = panelHash;
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
                TextBoxLayerName.Text = TextBoxLayerName.Text.Replace("|", "").Replace("{", "").Replace("}", "").Replace("*", "");
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

            _newLayer = new StreamDeckLayer();
            _newLayer.PanelHash = _panelHash;
            _newLayer.Name = TextBoxLayerName.Text;
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
