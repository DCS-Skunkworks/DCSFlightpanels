using System.Collections.Generic;
using System.Linq;

namespace DCS_BIOS
{
    internal class DCSBIOSProfileLoadStatus
    {
        private string Profile { get; set; }
        private bool Loaded { get; set; }
        private static readonly List<DCSBIOSProfileLoadStatus> LoadStatusList = new List<DCSBIOSProfileLoadStatus>();

        private DCSBIOSProfileLoadStatus(string profile, bool loaded)
        {
            Profile = profile;
            Loaded = loaded;
        }

        public static void Clear()
        {
            LoadStatusList.Clear();
        }

        public static void SetLoaded(string profile, bool loaded)
        {
            if (!IsRegistered(profile))
            {
                LoadStatusList.Add(new DCSBIOSProfileLoadStatus(profile, loaded));
                return;
            }

            foreach (var loadStatus in LoadStatusList)
            {
                if (loadStatus.Profile == profile)
                {
                    loadStatus.Loaded = loaded;
                }
            }
        }

        public static bool IsLoaded(string profile)
        {
            foreach (var loadStatus in LoadStatusList)
            {
                if (loadStatus.Profile == profile && loadStatus.Loaded)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsRegistered(string profile)
        {
            foreach (var loadStatus in LoadStatusList)
            {
                if (loadStatus.Profile == profile)
                {
                    return true;
                }
            }
            return false;
        }

        public static void Remove(string profile)
        {
            var itemToRemove = LoadStatusList.SingleOrDefault(r => r.Profile == profile);
            if (itemToRemove != null)
            {
                LoadStatusList.Remove(itemToRemove);
            }
        }
    }
}
