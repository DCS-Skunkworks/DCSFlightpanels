using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NonVisuals.StreamDeck.Events;
using OpenMacroBoard.SDK;
using StreamDeckSharp;


namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayerHandler
    {
        private StreamDeckPanel _streamDeckPanel = null;
        private volatile List<StreamDeckLayer> _layerList = new List<StreamDeckLayer>();
        private const string HOME_LAYER_ID = "*";
        private volatile List<string> _layerHistory = new List<string>();
        private volatile string _selectedLayer = StreamDeckConstants.HOME_LAYER_NAME;
        private readonly IStreamDeckBoard _streamDeckBoard;
        private EnumStreamDeckButtonNames _selectedButtonName = EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;





        public StreamDeckLayerHandler(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
            _streamDeckBoard = streamDeckPanel.StreamDeckBoard;
        }

        public List<string> GetLayerNameList()
        {
            var result = new List<string>();

            foreach (var layer in _layerList)
            {
                result.Add(layer.Name);
            }
            return result;
        }

        public string ExportJSONSettings()
        {
            const Formatting indented = Formatting.Indented;
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            CleanLayers();

            CheckHomeLayerStatus();

            return JsonConvert.SerializeObject(_layerList, indented, settings);
        }

        public void ImportJSONSettings(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return;
            }
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            _layerList = JsonConvert.DeserializeObject<List<StreamDeckLayer>>(jsonText, settings);

            CheckHomeLayerStatus();

            CheckSelectedLayer();
        }

        private void CheckHomeLayerStatus()
        {
            var found = false;

            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.Name == StreamDeckConstants.HOME_LAYER_NAME)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var streamDeckLayer = new StreamDeckLayer();
                streamDeckLayer.Name = StreamDeckConstants.HOME_LAYER_NAME;
                _layerList.Insert(0, streamDeckLayer);
            }

            CheckSelectedLayer();
        }

        public void CheckSelectedLayer()
        {
            if (!string.IsNullOrEmpty(_selectedLayer) && _layerList.FindAll(o => o.Name == _selectedLayer).Count == 1)
            {
                SetSelectedLayer(_selectedLayer);
            }
            else
            {
                _selectedLayer = StreamDeckConstants.HOME_LAYER_NAME;
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

        public List<StreamDeckLayer> LayerList
        {
            get => _layerList;
            set => _layerList = value;
        }

        public void ClearSettings()
        {
            _layerList.Clear();
        }

        public StreamDeckLayer HomeLayer
        {
            get
            {
                CheckHomeLayerStatus();
                return _layerList.Find(o => o.Name == StreamDeckConstants.HOME_LAYER_NAME);
            }
        }


        public StreamDeckButton AddButtonToSelectedLayer(StreamDeckButton streamDeckButton)
        {
            StreamDeckButton result = null;

            var selectedLayer = SelectedLayer;

            var found = false;

            foreach (var button in selectedLayer.StreamDeckButtons)
            {
                if (button.StreamDeckButtonName == streamDeckButton.StreamDeckButtonName)
                {
                    button.Consume(streamDeckButton);
                    result = button;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                selectedLayer.AddButton(streamDeckButton);
                result = streamDeckButton;
            }

            return result;
        }

        public StreamDeckLayer SelectedLayer
        {
            get
            {
                CheckHomeLayerStatus();
                return GetLayer(_selectedLayer);
            }
            set => SetSelectedLayer(value.Name);
        }

        public string SelectedLayerName
        {
            get
            {
                CheckHomeLayerStatus();
                return _selectedLayer;
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
                var found = false;
                foreach (var layer in LayerList)
                {
                    if (layer.Name == value)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    throw new Exception("StreamDeckLayerHandler : Failed to find layer " + value + " in order to mark it active.");
                }

                SetSelectedLayer(value);
            }
        }

        public bool HasLayers => _layerList.Count > 0;

        public void ClearAllFaces()
        {
            for (var i = 0; i < _streamDeckPanel.ButtonCount; i++)
            {
                _streamDeckBoard.ClearKey(i);
            }
        }

        private void CleanLayers()
        {
            foreach (var streamDeckLayer in _layerList)
            {
                streamDeckLayer.RemoveEmptyButtons();
            }
        }

        public void ShowPreviousLayer()
        {
            if (_layerHistory.Count > 0)
            {
                _selectedLayer = _layerHistory.Last();
                _layerHistory.RemoveAt(_layerHistory.Count - 1);
                SetSelectedLayer(_selectedLayer);
            }
        }

        public void ShowHomeLayer()
        {
            SetSelectedLayer(StreamDeckConstants.HOME_LAYER_NAME);
        }

        private void MarkAllButtonsHiddenAndClearFaces()
        {
            foreach (var streamDeckButton in StreamDeckButton.GetButtons())
            {
                streamDeckButton.IsVisible = false;
            }
            ClearAllFaces();
        }

        private void SetSelectedLayer(string layerName)
        {
            if (layerName == _selectedLayer)
            {
                return;
            }

            MarkAllButtonsHiddenAndClearFaces();

            /*
             * Something is wrong
             */
            if (string.IsNullOrEmpty(layerName))
            {
                throw new Exception("Internal Error : StreamDeckLayerHandler : Trying to set an empty or null layer active.");
            }

            /*
             * There are already layers, add last only if name differs
             */
            if (_selectedLayer != layerName)
            {
                _layerHistory.Add(_selectedLayer);
            }
            _selectedLayer = layerName;

            var selectedLayer = GetLayer(_selectedLayer);
            foreach (var streamDeckButtons in selectedLayer.StreamDeckButtons)
            {
                streamDeckButtons.IsVisible = true;
            }

            EventHandlers.LayerSwitched(this, new StreamDeckShowNewLayerArgs() { SelectedLayerName = _selectedLayer });
        }

        public int SelectedButtonNumber
        {
            get => _selectedButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON ? 999 : int.Parse(_selectedButtonName.ToString().Replace(StreamDeckConstants.NUMBER_BUTTON_PREFIX, ""));
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
                    EventHandlers.SelectedButtonChanged(this, SelectedButton);
                    return;
                }
                _selectedButtonName = value;
            }
        }

        public StreamDeckButton SelectedButton
        {
            get => SelectedLayer.GetStreamDeckButton(SelectedButtonName);
        }

        public void RemoveButton(StreamDeckButton streamDeckButton)
        {
            SelectedLayer.RemoveButton(streamDeckButton);
        }


        public List<string> GetStreamDeckLayerNames()
        {
            var result = new List<string>();

            foreach (var streamDeckLayer in _layerList)
            {
                result.Add(streamDeckLayer.Name);
            }

            return result;
        }

        public StreamDeckLayer GetLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return null;
            }

            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.Name == layerName)
                {
                    return streamDeckLayer;
                }
            }

            throw new Exception("GetStreamDeckLayer : Failed to find layer [" + layerName + "].");
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
                throw new Exception("Button " + buttonName + " cannot be found in layer " + layerName + ".");
            }

            var button = new StreamDeckButton(buttonName);
            GetLayer(layerName).AddButton(button);
            return button;
        }

        public StreamDeckButton GetSelectedLayerButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return GetButton(streamDeckButtonName, SelectedLayerName, false);
        }

        public StreamDeckButton GetSelectedLayerButton(int streamDeckButtonNumber)
        {
            var streamDeckButtonName = StreamDeckFunction.ButtonName(streamDeckButtonNumber);
            return GetButton(streamDeckButtonName, SelectedLayerName, false);
        }


    }
}
