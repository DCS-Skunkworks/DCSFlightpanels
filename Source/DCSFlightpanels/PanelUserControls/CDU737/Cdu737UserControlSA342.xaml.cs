
using System;
using System.Windows;
using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using NonVisuals.CockpitMaster.PreProgrammed;
using NonVisuals.EventArgs;
using NonVisuals.HID;
using NonVisuals.Interfaces;
using NonVisuals.Panels;

namespace DCSFlightpanels.PanelUserControls.CDU737
{
    /// <summary>
    /// Logique d'interaction pour Cdu737UserControlAH64D.xaml
    /// </summary>
    /// 
    public partial class Cdu737UserControlSA342 : IGamingPanelListener, IGamingPanelUserControl
    {
        private readonly CDU737PanelSA342 _cdu737PanelSA342;

        public Cdu737UserControlSA342(HIDSkeleton hidSkeleton)
        {
            InitializeComponent();

            _cdu737PanelSA342 = new CDU737PanelSA342(hidSkeleton);
            
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
                    _cdu737PanelSA342.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);

                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        public override void Init()
        {
            try
            {
                _cdu737PanelSA342.InitPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void CDU737UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            UserControlLoaded = true;
        }

        public override GamingPanel GetGamingPanel()
        {
            return _cdu737PanelSA342;
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
            string[] lines = _cdu737PanelSA342.CDULines;
            Dispatcher?.BeginInvoke(
            (Action)
            (() => {
                CDU737UserControl.DisplayLines(lines, 14);
            }
            ));

        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            string[] lines = _cdu737PanelSA342.CDULines;
            Dispatcher?.BeginInvoke(
            (Action)
            (() => {
                CDU737UserControl.DisplayLines(lines,14);
            }
            ));

        }
        private static void HideAllImages()
        {
        }

        public string GetName()
        {
            return GetType().Name;
        }
    }
}

