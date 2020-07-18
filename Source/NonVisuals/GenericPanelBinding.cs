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
        private List<string> _settings = new List<string>(50);
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

        public List<string> Settings
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

        public string SettingsString
        {
            get
            {
                var stringBuilder = new StringBuilder(500);
                foreach (var setting in _settings)
                {
                    stringBuilder.AppendLine(setting);
                }
                return stringBuilder.ToString();
            }
        }

        public void ClearSettings()
        {
            _settings.Clear();
        }

        public string ExportBinding()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PanelType=" + PanelType);
            stringBuilder.AppendLine("PanelInstanceID=" + HIDInstance);
            stringBuilder.AppendLine("BindingHash=" + BindingHash);

            if (PanelType == GamingPanelEnum.StreamDeckMini || PanelType == GamingPanelEnum.StreamDeck || PanelType == GamingPanelEnum.StreamDeckXL)
            {
                stringBuilder.AppendLine("BeginPanelJSON");
            }
            else
            {
                stringBuilder.AppendLine("BeginPanel");
            }

            foreach (var str in _settings)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    stringBuilder.AppendLine("\t" + str);
                }
            }

            if (PanelType == GamingPanelEnum.StreamDeckMini || PanelType == GamingPanelEnum.StreamDeck || PanelType == GamingPanelEnum.StreamDeckXL)
            {
                stringBuilder.AppendLine("EndPanelJSON");
            }
            else
            {
                stringBuilder.AppendLine("EndPanel");
            }

            return stringBuilder.ToString();
        }
    }
}
