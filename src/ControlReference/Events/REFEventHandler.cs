using ControlReference.Interfaces;

namespace ControlReference.Events
{
    internal static class REFEventHandler
    {
        public delegate void SetCategoryEventHandler(object sender, CategoryEventArgs e);
        public static event SetCategoryEventHandler OnChangeCategory;

        public static void AttachCategoryListener(ICategoryChange categoryChange)
        {
            OnChangeCategory += categoryChange.ChangeCategory;
        }

        public static void DetachCategoryListener(ICategoryChange categoryChange)
        {
            OnChangeCategory -= categoryChange.ChangeCategory;
        }

        public static void ChangeCategory(object sender, string category)
        {
            OnChangeCategory?.Invoke(sender, new CategoryEventArgs { Sender = sender, Category = category });
        }






        public delegate void SetNewDCSBIOSDataEventHandler(object sender, DCSBIOSDataCombinedEventArgs e);
        public static event SetNewDCSBIOSDataEventHandler OnNewDCSBIOSData;

        public static void AttachDCSBIOSDataListener(INewDCSBIOSData newDCSBIOSData)
        {
            OnNewDCSBIOSData += newDCSBIOSData.NewDCSBIOSData;
        }

        public static void DetachDCSBIOSDataListener(INewDCSBIOSData newDCSBIOSData)
        {
            OnNewDCSBIOSData -= newDCSBIOSData.NewDCSBIOSData;
        }

        public static void NewDCSBIOSUIntData(object sender, uint address, uint data)
        {
            OnNewDCSBIOSData?.Invoke(sender, new DCSBIOSDataCombinedEventArgs { Sender = sender, Address = address, IsUIntValue = true, Data = data});
        }

        public static void NewDCSBIOSStringData(object sender, uint address, string stringValue)
        {
            OnNewDCSBIOSData?.Invoke(sender, new DCSBIOSDataCombinedEventArgs { Sender = sender, Address = address, IsUIntValue = false, StringValue = stringValue });
        }
    }
}
