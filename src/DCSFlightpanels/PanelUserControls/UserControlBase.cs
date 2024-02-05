using System;
using System.Windows.Controls;
using ClassLibraryCommon;
using NonVisuals.Panels;

namespace DCSFlightpanels.PanelUserControls
{
    public abstract class UserControlBase : UserControl, IDisposable
    {
        private bool _disposed;

        protected bool UserControlLoaded { get; set; }
        protected bool TextBoxEnvironmentSet { get; set; }

        public abstract void Init();

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        
        public virtual GamingPanel GetGamingPanel()
        {
            return null;
        }

        public virtual GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.Unknown;
        }
    }
}
