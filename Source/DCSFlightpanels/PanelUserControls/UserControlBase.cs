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
        
        public void DeviceRemovedHandler()
        {
            GlobalHandler.Detach(GetGamingPanel());
        }

        public virtual GamingPanel GetGamingPanel()
        {
            return null;
        }

        public virtual GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.Unknown;
        }

        public IGlobalHandler GlobalHandler
        {
            set => _globalHandler = value;
            get => _globalHandler;
        }

        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            Dispose(true);
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
