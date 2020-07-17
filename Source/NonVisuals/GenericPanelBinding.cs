using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class GenericPanelBinding
    {
        private GamingPanelEnum _panelType = GamingPanelEnum.Unknown;
        private string _hidInstance;
        private string _bindingHash;
        private StringBuilder _settings = new StringBuilder();
        private bool _hardwareWasFound = false;
        private bool _hasBeenProcess = false;

        public GenericPanelBinding()
        {}

        public GenericPanelBinding(string hidInstance, string bindingHash)
        {
            _hidInstance = hidInstance;
            _bindingHash = bindingHash;
        }

        public string HIDInstance
        {
            get => _hidInstance;
            set => _hidInstance = value;
        }

        public string BindingHash
        {
            get => _bindingHash;
            set => _bindingHash = value;
        }

        public StringBuilder Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public bool HardwareWasFound
        {
            get => _hardwareWasFound;
            set => _hardwareWasFound = value;
        }

        public GamingPanelEnum PanelType
        {
            get => _panelType;
            set => _panelType = value;
        }

        public bool HasBeenProcess
        {
            get => _hasBeenProcess;
            set => _hasBeenProcess = value;
        }

        public string ExportBinding()
        {
            var stringBuilder = new StringBuilder();
            PanelType = PZ55SwitchPanel
            PanelInstanceID =\\?\hid#vid_06a3&pid_0d67#8&b2dc743&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                PanelSettingsVersion = 0X
            BeginPanel
                BeginPanelJSON
        }
    }
}
