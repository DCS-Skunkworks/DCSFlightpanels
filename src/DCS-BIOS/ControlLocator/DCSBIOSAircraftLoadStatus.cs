using System.Collections.Generic;
using System.Linq;

namespace DCS_BIOS.ControlLocator
{
    /// <summary>
    /// Convenience class for DCSBIOSControlLocator.
    /// </summary>
    internal class DCSBIOSAircraftLoadStatus
    {
        private string Filename { get; }
        private bool Loaded { get; set; }
        private static readonly List<DCSBIOSAircraftLoadStatus> LoadStatusList = new();

        private DCSBIOSAircraftLoadStatus(string filename, bool loaded)
        {
            Filename = filename;
            Loaded = loaded;
        }

        public static void Clear()
        {
            LoadStatusList.Clear();
        }

        public static void SetLoaded(string filename, bool loaded)
        {
            if (!IsRegistered(filename))
            {
                LoadStatusList.Add(new DCSBIOSAircraftLoadStatus(filename, loaded)); // <-- loaded set here
                return;
            }

            foreach (var loadStatus in LoadStatusList.Where(loadStatus => loadStatus.Filename == filename))
            {
                loadStatus.Loaded = loaded;
            }
        }

        public static bool IsLoaded(string filename)
        {
            return LoadStatusList.Exists(loadStatus => loadStatus.Filename == filename && loadStatus.Loaded);
        }

        private static bool IsRegistered(string filename)
        {
            return LoadStatusList.Exists(loadStatus => loadStatus.Filename == filename);
        }

        public static void Remove(string filename)
        {
            var itemToRemove = LoadStatusList.SingleOrDefault(r => r.Filename == filename);
            if (itemToRemove != null)
            {
                LoadStatusList.Remove(itemToRemove);
            }
        }
    }
}
