using System.Collections.Generic;
using System.Linq;

namespace DCS_BIOS
{
    /// <summary>
    /// Convenience class for DCSBIOSControlLocator.
    /// </summary>
    internal class DCSBIOSAircraftLoadStatus
    {
        private string Profile { get; }
        private bool Loaded { get; set; }
        private static readonly List<DCSBIOSAircraftLoadStatus> LoadStatusList = new();

        private DCSBIOSAircraftLoadStatus(string profile, bool loaded)
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
                LoadStatusList.Add(new DCSBIOSAircraftLoadStatus(profile, loaded));
                return;
            }

            foreach (var loadStatus in LoadStatusList.Where(loadStatus => loadStatus.Profile == profile))
            {
                loadStatus.Loaded = loaded;
            }
        }

        public static bool IsLoaded(string profile)
        {
            return LoadStatusList.Exists(loadStatus => loadStatus.Profile == profile && loadStatus.Loaded);
        }

        private static bool IsRegistered(string profile)
        {
            return LoadStatusList.Exists(loadStatus => loadStatus.Profile == profile);
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
