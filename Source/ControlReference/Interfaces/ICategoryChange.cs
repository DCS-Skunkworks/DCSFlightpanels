using ControlReference.Events;

namespace ControlReference.Interfaces
{
    internal interface ICategoryChange
    {
        void ChangeCategory(object sender, CategoryEventArgs args);
    }
    internal interface INewDCSBIOSData
    {
        void NewDCSBIOSData(object sender, DCSBIOSDataCombinedEventArgs args);
    }
}
