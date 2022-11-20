namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    using ClassLibraryCommon;

    using Interfaces;
    using Properties;
    using Windows;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Panels.Saitek.Panels;
    using NonVisuals.Panels.Saitek;
    using NonVisuals.Panels;
    using NonVisuals.HID;

    /// <summary>
    /// Interaction logic for BackLitPanelUserControl.xaml
    /// </summary>
    public partial class BackLitPanelUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, ILedLightPanelListener
    {
        private readonly BacklitPanelBIP _backlitPanelBIP;

        private readonly List<Image> _colorImages = new();
        private readonly List<Image> _configurationIndicatorImages = new();
        private readonly BitmapImage _blackImage = new(new Uri("pack://application:,,,/dcsfp;component/Images/black.png"));
        private readonly BitmapImage _redImage = new(new Uri("pack://application:,,,/dcsfp;component/Images/red.png"));
        private readonly BitmapImage _greenImage = new(new Uri("pack://application:,,,/dcsfp;component/Images/green.png"));
        private readonly BitmapImage _yellowImage = new(new Uri("pack://application:,,,/dcsfp;component/Images/yellow1.png"));
        private PanelLEDColor _lastToggleColor = PanelLEDColor.DARK;


        public BackLitPanelUserControl(HIDSkeleton hidSkeleton)
        {
            InitializeComponent();
            _backlitPanelBIP = new BacklitPanelBIP(Settings.Default.BIPLedStrength, hidSkeleton);
            
            AppEventHandler.AttachGamingPanelListener(this);
            AppEventHandler.AttachLEDLightListener(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _backlitPanelBIP.Dispose(); 
                    AppEventHandler.DetachGamingPanelListener(this);
                    AppEventHandler.DetachLEDLightListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        

        private void BackLitPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            Init();
            UserControlLoaded = true;
            SetContextMenuClickHandlers();
            SetAllBlack();
            ShowGraphicConfiguration();
        }
        
        public override GamingPanel GetGamingPanel()
        {
            return _backlitPanelBIP;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.BackLitPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }
        
        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public void ProfileEvent(object sender, ProfileEventArgs e){}

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e) { }
        
        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
            try
            {
                if (!UserControlLoaded || _backlitPanelBIP.HIDInstance.Equals(e.HIDInstance))
                {
                    return;
                }

                Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void SettingsApplied(object sender, PanelInfoArgs e)
        {
            try
            {
                if (!UserControlLoaded)
                {
                    return;
                }

                if (e.PanelType == GamingPanelEnum.PZ69RadioPanel &&
                    e.HidInstance.Equals(_backlitPanelBIP.HIDInstance))
                {
                    Dispatcher?.BeginInvoke((Action) (ShowGraphicConfiguration));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsModified(object sender, PanelInfoArgs e)
        {
            try
            {
                if (!UserControlLoaded)
                {
                    return;
                }

                if (_backlitPanelBIP.HIDInstance.Equals(e.HidInstance))
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void Init()
        {
            var imageList = Common.FindVisualChildren<Image>(this);
            foreach (var image in imageList)
            {
                if (image.Name.StartsWith("ImagePosition"))
                {
                    _colorImages.Add(image);
                }
                if (image.Name.StartsWith("ImageConfigFound"))
                {
                    _configurationIndicatorImages.Add(image);
                }
            }
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!UserControlLoaded)
                {
                    return;
                }
                HideAllConfigurationExistsImages();
                foreach (BIPLedPositionEnum position in Enum.GetValues(typeof(BIPLedPositionEnum)))
                {
                    SetLEDImage(position, _backlitPanelBIP.GetColor(position));
                    SetConfigExistsImageVisibility(_backlitPanelBIP.HasConfiguration(position), position);
                }

                TextBoxBrightnessControl.Text = _backlitPanelBIP.BrightnessBinding != null ? _backlitPanelBIP.BrightnessBinding.ControlId : string.Empty;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void SetAllBlack()
        {
            foreach (var image in _colorImages)
            {
                image.Source = _blackImage;
            }
        }

        private void MenuItem_ContextConfigureBIPLED_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (MenuItem)sender;
                var contextMenu = (ContextMenu)menuItem.Parent;
                var imageName = contextMenu.Tag.ToString();
                var position = BacklitPanelBIP.GetLedPosition(imageName);

                var ledConfigsWindow = new LEDConfigsWindow("Set configuration for LED : " + position, new SaitekPanelLEDPosition(position), _backlitPanelBIP.GetLedDcsBiosOutputs(position), _backlitPanelBIP);
                if (ledConfigsWindow.ShowDialog() == true)
                {
                    //must include position because if user has deleted all entries then there is nothing to go after regarding position
                    _backlitPanelBIP.SetLedDcsBiosOutput(position, ledConfigsWindow.ColorOutputBindings);
                }
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void SetContextMenuClickHandlers()
        {
            if (!Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
            {
                return;
            }
            foreach (var image in Common.FindVisualChildren<Image>(this))
            {
                if (image.ContextMenu == null && image.Name.StartsWith("ImagePosition"))
                {
                    image.ContextMenu = (ContextMenu)Resources["BIPLEDContextMenu"];
                    if (image.ContextMenu != null)
                    {
                        image.ContextMenu.Tag = image.Name;
                    }
                }
            }
        }

        private void ImagePosition_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var newColor = PanelLEDColor.DARK;
                var image = (Image)sender;
                var currentColor = (PanelLEDColor)(Enum.Parse(typeof(PanelLEDColor), ((string)image.Tag)));
                if (currentColor == PanelLEDColor.DARK)
                {
                    image.Source = _greenImage;
                    newColor = PanelLEDColor.GREEN;
                    image.Tag = "GREEN";
                }
                else if (currentColor == PanelLEDColor.GREEN)
                {
                    image.Source = _yellowImage;
                    newColor = PanelLEDColor.YELLOW;
                    image.Tag = "YELLOW";
                }
                else if (currentColor == PanelLEDColor.YELLOW)
                {
                    image.Source = _redImage;
                    newColor = PanelLEDColor.RED;
                    image.Tag = "RED";
                }
                else if (currentColor == PanelLEDColor.RED)
                {
                    image.Source = _blackImage;
                    newColor = PanelLEDColor.DARK;
                    image.Tag = "DARK";
                }

                SetPhysicalLED(image, newColor);
            }
        }

        private void SetLEDImage(BIPLedPositionEnum bipLedPositionEnum, PanelLEDColor newColor)
        {
            Image image = null;
            foreach (var tmpImage in _colorImages)
            {
                if (tmpImage.Name.Contains(bipLedPositionEnum.ToString()))
                {
                    image = tmpImage;
                    break;
                }
            }
            if (image != null)
            {
                SetLEDImage(image, newColor);
            }
        }

        private void SetLEDImage(Image image, PanelLEDColor newColor)
        {
            if (newColor == PanelLEDColor.DARK)
            {
                image.Source = _blackImage;
                image.Tag = "DARK";
            }
            else if (newColor == PanelLEDColor.GREEN)
            {
                image.Source = _greenImage;
                image.Tag = "GREEN";
            }
            else if (newColor == PanelLEDColor.YELLOW)
            {
                image.Source = _yellowImage;
                image.Tag = "YELLOW";
            }
            else if (newColor == PanelLEDColor.RED)
            {
                image.Source = _redImage;
                image.Tag = "RED";
            }
        }


        private void HideAllConfigurationExistsImages()
        {
            foreach (var configurationIndicatorImage in _configurationIndicatorImages)
            {
                configurationIndicatorImage.Visibility = Visibility.Collapsed;
            }
        }

        private void SetConfigExistsImageVisibility(bool show, BIPLedPositionEnum bipLedPositionEnum)
        {
            var posString = _backlitPanelBIP.GetPosString(bipLedPositionEnum);
            foreach (var configurationIndicatorImage in _configurationIndicatorImages)
            {
                if (configurationIndicatorImage.Name.Contains(posString))
                {
                    if (show)
                    {
                        configurationIndicatorImage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        configurationIndicatorImage.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void SetPhysicalLED(Image image, PanelLEDColor newColor)
        {
            var position = BacklitPanelBIP.GetLedPosition(image.Name);
            _backlitPanelBIP.SetLED(position, newColor);
        }

        private void ToggleAll_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (_lastToggleColor)
                {
                    case PanelLEDColor.DARK:
                        {
                            _lastToggleColor = PanelLEDColor.GREEN;
                            break;
                        }
                    case PanelLEDColor.GREEN:
                        {
                            _lastToggleColor = PanelLEDColor.YELLOW;
                            break;
                        }
                    case PanelLEDColor.YELLOW:
                        {
                            _lastToggleColor = PanelLEDColor.RED;
                            break;
                        }
                    case PanelLEDColor.RED:
                        {
                            _lastToggleColor = PanelLEDColor.DARK;
                            break;
                        }
                }
                foreach (var image in _colorImages)
                {
                    SetLEDImage(image, _lastToggleColor);
                    SetPhysicalLED(image, _lastToggleColor);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void LEDIncrease_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _backlitPanelBIP.LEDBrightnessIncrease();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LEDDecrease_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _backlitPanelBIP.LEDBrightnessDecrease();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_backlitPanelBIP != null)
                {
                    TextBoxLogBIP.Text = string.Empty;
                    TextBoxLogBIP.Text = _backlitPanelBIP.HIDInstance;
                    Clipboard.SetText(_backlitPanelBIP.HIDInstance);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonGetHash_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_backlitPanelBIP != null)
                {
                    Clipboard.SetText(Common.GetMd5Hash(_backlitPanelBIP.HIDInstance));
                    MessageBox.Show("The MD5 hash for the panel has been copied to the Clipboard.\nUse this value when you connect switches to B.I.P. lights.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonGetIdentify_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _backlitPanelBIP.Identify();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            if (clearAlsoProfile)
            {
                _backlitPanelBIP.ClearSettings(true);
            }

            ShowGraphicConfiguration();
        }

        private void ButtonClearSettings_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Clear all settings?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    ClearAll(true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonSelectBrightnessControl_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var outputControlsWindow = new DCSBiosOutputWindow("Brightness Control", false);
                outputControlsWindow.ShowDialog();
                if (outputControlsWindow.DialogResult.HasValue && outputControlsWindow.DialogResult.Value)
                {
                    _backlitPanelBIP.SetBrightnessBinding(outputControlsWindow.DCSBiosOutput);
                    TextBoxBrightnessControl.Text = outputControlsWindow.DCSBiosOutput.ControlId;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void BIPImageOnMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void BIPImage_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
