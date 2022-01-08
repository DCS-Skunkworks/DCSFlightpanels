using System;
using System.Windows.Controls;
using ClassLibraryCommon;
using NonVisuals;


namespace DCSFlightpanels.PanelUserControls
{
    public class UserControlBase : UserControl, IDisposable
    {
        private TabItem _parentTabItem;
        private bool _disposed;

        public string ParentTabItemHeader { get; set; }  //unused except in 6 commented lines and I think those lines could be deleted, not very clear in my head.
        public bool UserControlLoaded { get; set; }

        public TabItem ParentTabItem
        {
            get => _parentTabItem;
            set
            {
                _parentTabItem = value;
                if (_parentTabItem != null && _parentTabItem.Header != null)
                {
                    ParentTabItemHeader = ParentTabItem.Header.ToString();
                }
            }
        }
        
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
        }

        public void DeviceRemovedHandler()
        {
            Dispose();
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
