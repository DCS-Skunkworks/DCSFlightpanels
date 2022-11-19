﻿namespace NonVisuals
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using ClassLibraryCommon;
    using HID;

    public class GenericPanelBinding
    {
        private GamingPanelEnum _panelType = GamingPanelEnum.Unknown;
        private string _hidInstance;
        private string _bindingHash;
        private List<string> _settings = new(50);
        private string _jsonString = string.Empty;
        private bool _hasBeenDeleted;


        public GenericPanelBinding()
        { }

        public GenericPanelBinding(string hidInstance, string bindingHash, GamingPanelEnum panelType)
        {
            _hidInstance = hidInstance;
            _bindingHash = bindingHash;
            _panelType = panelType;
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

        public bool Match(HIDSkeleton hidSkeleton)
        {
            return HIDInstance.Equals(hidSkeleton.HIDInstance) && PanelType == hidSkeleton.GamingPanelType;
        }

        public bool Match(GenericPanelBinding genericPanelBinding)
        {
            return HIDInstance.Equals(genericPanelBinding.HIDInstance) && PanelType == genericPanelBinding.PanelType;
        }

        public void JSONAddLine(string jsonLine)
        {
            if (string.IsNullOrEmpty(_jsonString))
            {
                _jsonString = jsonLine;
            }
            else
            {
                _jsonString = _jsonString + Environment.NewLine + jsonLine;
            }
        }
        
        public GamingPanelEnum PanelType
        {
            get => _panelType;
            set => _panelType = value;
        }

        public bool HasBeenDeleted
        {
            get => _hasBeenDeleted;
            set => _hasBeenDeleted = value;
        }

        public string JSONString
        {
            get => _jsonString;
            set => _jsonString = value;
        }

        /// <summary>
        /// If a panel has been found with matching HID and type this will be true
        /// </summary>
        public bool InUse { get; set; }

        public string SettingsString
        {
            get
            {
                if (IsJSON())
                {
                    return _jsonString;
                }

                var stringBuilder = new StringBuilder(500);
                foreach (var setting in _settings)
                {
                    stringBuilder.AppendLine(setting);
                }

                return stringBuilder.ToString();
            }
        }

        public bool IsJSON()
        {
            return (PanelType == GamingPanelEnum.StreamDeckMini || PanelType == GamingPanelEnum.StreamDeck || PanelType == GamingPanelEnum.StreamDeckXL || PanelType == GamingPanelEnum.StreamDeckV2 || PanelType == GamingPanelEnum.StreamDeckMK2);
        }

        public void ClearSettings()
        {
            _settings.Clear();
        }

        public string ExportBinding()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PanelType=" + _panelType);
            stringBuilder.AppendLine("PanelInstanceID=" + _hidInstance);
            stringBuilder.AppendLine("BindingHash=" + _bindingHash);

            if (IsJSON())
            {
                stringBuilder.AppendLine("BeginPanelJSON");
            }
            else
            {
                stringBuilder.AppendLine("BeginPanel");
            }

            if (IsJSON())
            {
                stringBuilder.Append(_jsonString);
            }
            else
            {
                foreach (var str in _settings)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        stringBuilder.AppendLine("\t" + str);
                    }
                }
            }

            if (IsJSON())
            {
                stringBuilder.AppendLine(Environment.NewLine + "EndPanelJSON");
            }
            else
            {
                stringBuilder.AppendLine("EndPanel");
            }

            return stringBuilder.ToString();
        }
    }

    public enum GenericBindingStateEnum
    {
        Unknown,
        New,
        Modified,
        Deleted
    }

    public class ModifiedGenericBinding
    {
        private readonly GenericBindingStateEnum _state = GenericBindingStateEnum.Unknown;
        private readonly GenericPanelBinding _genericPanelBinding;

        public ModifiedGenericBinding(GenericBindingStateEnum state, GenericPanelBinding genericPanelBinding)
        {
            _state = state;
            _genericPanelBinding = genericPanelBinding;
        }

        public GenericBindingStateEnum State => _state;

        public GenericPanelBinding GenericPanelBinding => _genericPanelBinding;
    }
}
