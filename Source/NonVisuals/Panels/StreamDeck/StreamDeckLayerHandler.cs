namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Reflection;
    using System.Text;

    using MEF;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog;
    using Events;

    using OpenMacroBoard.SDK;

    using StreamDeckSharp;
    using Panels;

    public class StreamDeckLayerHandler : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly StreamDeckPanel _streamDeckPanel;
        private volatile List<StreamDeckLayer> _layerList = new();
        private volatile List<string> _layerHistory = new();
        private volatile string _selectedLayerName = string.Empty;
        private readonly IStreamDeckBoard _streamDeckBoard;
        private EnumStreamDeckButtonNames _selectedButtonName = EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;

        private bool _jsonImported;

        private static int _hidInstanceCounter;
        private readonly int _hidInstance;


        private readonly UnicodeEncoding _uniCodeEncoding = new();
        private const Formatting INDENTED_FORMATTING = Formatting.Indented;
        private readonly JsonSerializerSettings _jsonSettings = new()
        {
            ContractResolver = new ExcludeObsoletePropertiesResolver(),
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) =>
                {
                    Logger.Error($"JSON Error.{args.ErrorContext.Error.Message}");
                }
        };

        public static int HIDInstanceCounter => _hidInstanceCounter;
        public int HIDInstance => _hidInstance;
        public bool HasLayers => _layerList.Count > 0;

        public List<StreamDeckLayer> LayerList
        {
            get => _layerList;
            set => _layerList = value;
        }

        public StreamDeckLayer HomeLayer
        {
            get
            {
                CheckHomeLayerExists();
                return _layerList.Find(o => o.Name == StreamDeckConstants.HOME_LAYER_NAME);
            }
        }

        public StreamDeckLayer SelectedLayer
        {
            get
            {
                CheckHomeLayerExists();
                return GetLayer(_selectedLayerName);
            }

            set => SetSelectedLayer(value.Name);
        }






        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamDeckPanel"></param>
        public StreamDeckLayerHandler(StreamDeckPanel streamDeckPanel)
        {
            _hidInstance = _hidInstanceCounter++;
            _streamDeckPanel = streamDeckPanel;
            _streamDeckBoard = streamDeckPanel.StreamDeckBoard;
        }


        public void Dispose()
        {
            foreach (var streamDeckLayer in _layerList)
            {
                streamDeckLayer.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        public StreamDeckButton SelectedButton
        {
            get => SelectedLayer.GetStreamDeckButton(SelectedButtonName);
        }

        public List<string> GetLayerNameList()
        {
            List<string> result = new();
            _layerList.ForEach(x => result.Add(x.Name));
            return result;
        }

        public string ExportJSONSettings()
        {
            CleanLayers();

            CheckHomeLayerExists();

            return JsonConvert.SerializeObject(_layerList, INDENTED_FORMATTING, _jsonSettings);
        }

        public void ImportJSONSettings(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return;
            }

            jsonText = JSONFixer.Fix(jsonText);
            _jsonSettings.MissingMemberHandling = MissingMemberHandling.Error;

            _layerList = JsonConvert.DeserializeObject<List<StreamDeckLayer>>(jsonText, _jsonSettings);
            _layerList.ForEach(layer => layer.StreamDeckPanelInstance = _streamDeckPanel);
            _jsonImported = true;
            SetStreamDeckPanelInstance(_streamDeckPanel);
            CheckHomeLayerExists();
        }

        public void SetStreamDeckPanelInstance(StreamDeckPanel streamDeckPanel)
        {
            foreach (var streamDeckLayer in LayerList)
            {
                streamDeckLayer.StreamDeckPanelInstance = streamDeckPanel;
            }
        }

        private void CheckHomeLayerExists()
        {
            if (!_layerList.Exists(x => x.Name == StreamDeckConstants.HOME_LAYER_NAME))
            {
                var streamDeckLayer = new StreamDeckLayer(_streamDeckPanel)
                {
                    Name = StreamDeckConstants.HOME_LAYER_NAME
                };
                _layerList.Insert(0, streamDeckLayer);
            }

            CheckSelectedLayer();
        }

        public void CheckSelectedLayer()
        {
            if (!string.IsNullOrEmpty(_selectedLayerName) && _layerList.FindAll(o => o.Name == _selectedLayerName).Count == 1)
            {
                SetSelectedLayer(_selectedLayerName);
            }
            else
            {
                SetSelectedLayer(StreamDeckConstants.HOME_LAYER_NAME);
            }
        }

        public bool AddLayer(StreamDeckLayer streamDeckLayer)
        {
            if (streamDeckLayer == null || string.IsNullOrEmpty(streamDeckLayer.Name))
            {
                return false;
            }

            if (!LayerList.Contains(streamDeckLayer))
            {
                LayerList.Add(streamDeckLayer);
            }

            SetSelectedLayer(streamDeckLayer.Name);
            return true;
        }

        public void DeleteLayer(string layerName)
        {
            if (layerName == StreamDeckConstants.HOME_LAYER_NAME)
            {
                throw new Exception("Home layer can not be deleted.");
            }

            if (string.IsNullOrEmpty(layerName))
            {
                return;
            }

            GetLayer(layerName).IsVisible = false;
            GetLayer(layerName).RemoveButtons(true);
            _layerList.RemoveAll(x => x.Name == layerName);
            _layerHistory.RemoveAll(x => x == layerName);

            if (_layerHistory.Count > 0)
            {
                ShowPreviousLayer();
            }
            else if (_layerList.Count > 0)
            {
                SetSelectedLayer(_layerList[0].Name);
            }
            else
            {
                SetSelectedLayer(null);
            }
        }

        public void EraseLayerButtons(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return;
            }

            GetLayer(layerName).RemoveButtons(true);
        }

        public void ClearSettings()
        {
            foreach (var streamDeckLayer in _layerList)
            {
                streamDeckLayer.IsVisible = false;
                streamDeckLayer.RemoveButtons(false);
            }

            _layerList.Clear();
            ClearAllFaces();
        }

        public string SelectedLayerName
        {
            get
            {
                CheckHomeLayerExists();
                return _selectedLayerName;
            }

            set
            {
                if (LayerList.Count == 0)
                {
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                bool found = _layerList.Exists(x => x.Name == value);

                if (!found)
                {
                    throw new Exception($"StreamDeckLayerHandler : Failed to find layer with name {value} in order to mark it selected.");
                }

                SetSelectedLayer(value);
            }
        }

        public void ClearAllFaces()
        {
            for (var i = 0; i < _streamDeckPanel.ButtonCount; i++)
            {
                _streamDeckBoard.ClearKey(i);
            }
        }

        public void ClearFace(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            if (streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }

            _streamDeckBoard.ClearKey(StreamDeckCommon.ButtonNumber(streamDeckButtonName) - 1);
        }

        private void CleanLayers()
        {
            _layerList.ForEach(layer => layer.RemoveEmptyButtons());
        }

        public string GetConfigurationInformation()
        {
            var stringBuilder = new StringBuilder(500);
            stringBuilder.Append('\n');

            stringBuilder.Append($"Layer count : {_layerList.Count}, button count = {_streamDeckPanel.GetButtons().Count}\n");
            stringBuilder.Append("Existing layers:\n");
            foreach (var streamDeckLayer in _layerList)
            {
                stringBuilder.Append($"\t{streamDeckLayer.Name} ({streamDeckLayer.StreamDeckButtons.Count})\n");
            }

            stringBuilder.Append('\n');

            return stringBuilder.ToString();
        }

        public void ShowPreviousLayer()
        {
            if (_layerHistory.Count > 0)
            {
                var tmpLayer = _layerHistory.Last();
                _layerHistory.RemoveAt(_layerHistory.Count - 1);
                SetSelectedLayer(tmpLayer);
            }
        }

        public void ShowHomeLayer()
        {
            SetSelectedLayer(StreamDeckConstants.HOME_LAYER_NAME);
        }

        private void MarkAllButtonsHiddenAndClearFaces()
        {
            if (_streamDeckPanel.GetButtons() != null)
            {
                _streamDeckPanel.GetButtons().ForEach(button => button.IsVisible = false);
                ClearAllFaces();
            }
        }

        private void SetSelectedLayer(string layerName)
        {
            if (layerName == _selectedLayerName && _jsonImported == false)
            {
                return;
            }

            // We want one update after having read the json
            _jsonImported = false;

            MarkAllButtonsHiddenAndClearFaces();

            /*
             * Something is wrong
             */
            if (string.IsNullOrEmpty(layerName))
            {
                throw new Exception("Internal Error : StreamDeckLayerHandler : Trying to select an empty or null layer.");
            }

            /*
             * There are already layers, add last only if name differs
             */
            if (_selectedLayerName != layerName)
            {
                _layerHistory.Add(_selectedLayerName);
            }

            _selectedLayerName = layerName;

            var selectedLayer = GetLayer(_selectedLayerName);

            selectedLayer.IsVisible = true;

            SDEventHandler.LayerSwitched(this, _streamDeckPanel.BindingHash, _selectedLayerName);
        }

        public int SelectedButtonNumber
        {
            get => _selectedButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON ? 999 : int.Parse(_selectedButtonName.ToString().Replace(StreamDeckConstants.NUMBER_BUTTON_PREFIX, string.Empty));
            set
            {
                if (SelectedButtonNumber != value)
                {
                    var selectedButtonName = (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), StreamDeckConstants.NUMBER_BUTTON_PREFIX + value, true);
                    SelectedButtonName = selectedButtonName;
                }
            }
        }

        public EnumStreamDeckButtonNames SelectedButtonName
        {
            get => _selectedButtonName;
            set
            {
                if (_selectedButtonName != value)
                {
                    _selectedButtonName = value;
                    SDEventHandler.SelectedButtonChanged(this, SelectedButton, _streamDeckPanel.BindingHash);
                    return;
                }

                _selectedButtonName = value;
            }
        }

        public List<string> GetStreamDeckLayerNames()
        {
            List<string> result = new();
            _layerList.ForEach(layer => result.Add(layer.Name));
            return result;
        }

        public StreamDeckLayer GetLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return null;
            }

            if (!_layerList.Exists(layer => layer.Name == layerName))
            {
                throw new Exception($"GetStreamDeckLayer : Failed to find layer with name [{layerName}].");
            }

            return _layerList.First(x => x.Name == layerName);
        }

        public StreamDeckButton GetButton(EnumStreamDeckButtonNames buttonName, string layerName, bool throwExceptionIfNotFound = true)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return null;
            }

            foreach (var layer in _layerList)
            {
                if (layer.Name == layerName && layer.ContainStreamDeckButton(buttonName))
                {
                    return layer.GetStreamDeckButtonName(buttonName);
                }
            }

            if (throwExceptionIfNotFound)
            {
                throw new Exception($"Button [{buttonName}] cannot be found in layer [{layerName}].");
            }

            var button = new StreamDeckButton(buttonName, _streamDeckPanel);

            /*
             * Silently means there won't be any event of type "New Button added". This is an empty button
             * and unless it receives settings later it will just be here to serve the machinery instead of
             * having to do with streamdeck buttons being null.
             *
             * The reason, Layer A has layer navigation on button 15 => Layer B.
             * Layer B has nothing configured for button 15.
             * When user presses button (button still pressed) a switch to Layer B occurs.
             * When user releases the button Stream Deck Sharp reports an release event,
             * DCSFP then retrieves the streamdeck button for 15 but none exists, it is then
             * we end up here on the next line.
             */
            GetLayer(layerName).AddButton(button, true);
            return button;
        }

        public StreamDeckButton GetSelectedLayerButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return GetButton(streamDeckButtonName, SelectedLayerName, false);
        }

        public StreamDeckButton GetSelectedLayerButtonNumber(int streamDeckButtonNumber)
        {
            var streamDeckButtonName = StreamDeckCommon.ButtonName(streamDeckButtonNumber);
            return GetButton(streamDeckButtonName, SelectedLayerName, false);
        }

        public bool LayerExists(string layerName)
        {
            return _layerList.Exists(x => x.Name == layerName);
        }

    }

    public class ExcludeObsoletePropertiesResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            if (jsonProperty.AttributeProvider.GetAttributes(true).OfType<ObsoleteAttribute>().Any())
            {
                jsonProperty.ShouldSerialize = obj => false;
            }

            return jsonProperty;
        }
    }
}
