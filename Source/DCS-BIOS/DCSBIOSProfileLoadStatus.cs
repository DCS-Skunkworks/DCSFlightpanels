using System.Collections.Generic;
using System.Linq;

namespace DCS_BIOS
{
    internal class DCSBIOSProfileLoadStatus
    {
        private string Profile { get; set; }
        private bool Loaded { get; set; }
        private static List<DCSBIOSProfileLoadStatus> _loadStatusList = new List<DCSBIOSProfileLoadStatus>();

        private DCSBIOSProfileLoadStatus(string profile, bool loaded)
        {
            Profile = profile;
            Loaded = loaded;
        }

        public static void Clear()
        {
            _loadStatusList.Clear();
        }

        public static void SetLoaded(string profile, bool loaded)
        {
            if (!IsRegistered(profile))
            {
                _loadStatusList.Add(new DCSBIOSProfileLoadStatus(profile, loaded));
                return;
            }

            foreach (var loadStatus in _loadStatusList)
            {
                if (loadStatus.Profile == profile)
                {
                    loadStatus.Loaded = loaded;
                }
            }
        }

        public static bool IsLoaded(string profile)
        {
            foreach (var loadStatus in _loadStatusList)
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
            foreach (var loadStatus in _loadStatusList)
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
            var itemToRemove = _loadStatusList.SingleOrDefault(r => r.Profile == profile);
            if (itemToRemove != null)
            {
                _loadStatusList.Remove(itemToRemove);
            }
        }
    }
}
