using System;
using System.Windows.Controls;
using ClassLibraryCommon;
using NonVisuals;
using NonVisuals.Interfaces;

namespace DCSFlightpanels.PanelUserControls
{
    public class UserControlBase : UserControl, IDisposable
    {
        private IGlobalHandler _globalHandler;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private bool _userControlLoaded;
        

        public UserControlBase()
        {}
        

        private bool _disposed;
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

        public string ParentTabItemHeader
        {
            get => _parentTabItemHeader;
            set => _parentTabItemHeader = value;
        }

        public bool UserControlLoaded
        {
            get => _userControlLoaded;
            set => _userControlLoaded = value;
        }


    }
}
