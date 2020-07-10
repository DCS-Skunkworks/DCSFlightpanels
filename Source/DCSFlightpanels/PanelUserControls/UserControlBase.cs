using System;
using System.Windows.Controls;
using ClassLibraryCommon;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Windows;

namespace DCSFlightpanels.PanelUserControls
{
    public class UserControlBase : UserControl, IDisposable
    {

        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            Dispose(true);
        }

        
    }
}
