namespace DCSFlightpanels.PanelUserControls.PreProgrammed
{
    using System.Windows;
    using System.Windows.Controls;
    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;

    using NonVisuals.CockpitMaster.Preprogrammed;
    using DCSFlightpanels.Interfaces;
    using System;

    /// <summary>
    /// Logique d'interaction pour Cdu737UserControlA10C.xaml
    /// </summary>
    /// 
    public partial class Cdu737UserControlA10C : UserControlBase,
        IGamingPanelListener,
        IGamingPanelUserControl
    {
        private readonly CDU737PanelA10C _CDU737PanelA10C;

        public Cdu737UserControlA10C(HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            InitializeComponent();
            ParentTabItem = parentTabItem;

            _CDU737PanelA10C = new CDU737PanelA10C(hidSkeleton);
            //_HIDSkeleton = hidSkeleton;
            AppEventHandler.AttachGamingPanelListener(this);

            HideAllImages();

        }
        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _CDU737PanelA10C.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);

                }
                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        private bool _once = true;

        private void CDU737UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            UserControlLoaded = true;

            if (_once)
            {
                _once = false;
            }
        }

        public override GamingPanel GetGamingPanel()
        {
            return _CDU737PanelA10C;
        }


        public void ProfileEvent(object sender, ProfileEventArgs e)
        {

        }

        public void SettingsApplied(object sender, PanelInfoArgs e)
        {

        }

        public void SettingsModified(object sender, PanelInfoArgs e)
        {

        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            string[] lines = _CDU737PanelA10C.CDULines;
            Dispatcher?.BeginInvoke(
            (Action)
            (() =>
            {
                CDULines.Text = lines[0] + '\n';
                CDULines.Text += lines[1] + '\n';
                CDULines.Text += lines[2] + '\n';
                CDULines.Text += lines[3] + '\n';
                CDULines.Text += lines[4] + '\n';
                CDULines.Text += lines[5] + '\n';
                CDULines.Text += lines[6] + '\n';
                CDULines.Text += lines[7] + '\n';
                CDULines.Text += lines[8] + '\n';
                CDULines.Text += lines[9];
            }
            ));
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {

        }
        private void HideAllImages()
        {

        }

        public string GetName()
        {
            return GetType().Name;
        }

    }
}

