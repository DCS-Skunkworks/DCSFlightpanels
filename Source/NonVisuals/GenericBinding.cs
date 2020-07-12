using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class GenericBinding
    {
        private GamingPanelEnum _panelType = GamingPanelEnum.Unknown;
        private string _hidInstance;
        private string _bindingHash;
        private string _settings;
        private bool _hardwareWasFound = false;
        private bool _hasBeenProcess = false;

        public GenericBinding()
        {}

        public GenericBinding(string hidInstance, string bindingHash)
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

        public string Settings
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
    }
}
