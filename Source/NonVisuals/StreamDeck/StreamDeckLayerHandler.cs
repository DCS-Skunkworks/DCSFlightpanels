using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayerHandler
    {
        private List<StreamDeckLayer> _layerList = new List<StreamDeckLayer>();
        private const string SEPARATOR_CHARS = "\\o/";
        private const string HOME_LAYER_ID = "*";
        private List<string> _layerHistory = new List<string>();
        private string _activeLayer = "";
        private string _homeLayer = "";
        private IStreamDeckBoard _streamDeckBoard;
        private StreamDeckRequisites _streamDeckRequisite = new StreamDeckRequisites();







        public StreamDeckLayerHandler(IStreamDeckBoard streamDeckBoard)
        {
            _streamDeckBoard = streamDeckBoard;
            _streamDeckRequisite.StreamDeckBoard = _streamDeckBoard;
        }

        public List<StreamDeckLayer> GetEmptyLayers()
        {
            var result = new List<StreamDeckLayer>();

            foreach (var layer in _layerList)
            {
                var found = false;
                foreach (var emptyLayer in result)
                {
                    if (layer.Name == emptyLayer.Name)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    result.Add(layer.GetEmptyLayer());
                }
            }
            return result;
        }

        public string ExportJSONSettings()
        {
            var indented = Formatting.Indented;
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            CleanLayers();

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

            CheckAndSetHomeStatus();
        }

        private void CheckAndSetHomeStatus()
        {
            var found = false;
            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.IsHomeLayer)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _homeLayer = "";
            }

            if (_layerList.Count > 0)
            {
                _homeLayer = _layerList[0].Name;
                _layerList[0].IsHomeLayer = true;
            }
        }

        public void SetHomeStatus(bool isHomeLayer, string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                throw new Exception("SetHomeLayer : No layer name specified.");
            }

            foreach (var deckLayer in _layerList)
            {
                deckLayer.IsHomeLayer = false;
                if (isHomeLayer && deckLayer.Name == layerName)
                {
                    deckLayer.IsHomeLayer = true;
                }
            }

            CheckAndSetHomeStatus();
        }
        

        public bool AddLayer(StreamDeckLayer streamDeckLayer)
        {
            if (streamDeckLayer == null || string.IsNullOrEmpty(streamDeckLayer.Name))
            {
                return false;
            }

            var result = streamDeckLayer.IsHomeLayer;

            if (!LayerList.Contains(streamDeckLayer))
            {
                if (LayerList.Count == 0)
                {
                    streamDeckLayer.IsHomeLayer = true;
                }
                LayerList.Add(streamDeckLayer);
            }

            SetActiveLayer(streamDeckLayer.Name);
            return result;
        }

        public void DeleteLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return;
            }

            _layerList.RemoveAll(x => x.Name == layerName);
            _layerHistory.RemoveAll(x => x == layerName);
            if (_layerList.Count > 0)
            {
                SetActiveLayer(_layerList[0].Name);
            }
            else
            {
                SetActiveLayer(null);
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
                foreach (var streamDeckLayer in _layerList)
                {
                    if (streamDeckLayer.IsHomeLayer)
                    {
                        return streamDeckLayer;
                    }
                }

                return null;
            }
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

        public StreamDeckLayer GetActiveStreamDeckLayer()
        {
            return GetStreamDeckLayer(false, _activeLayer);
        }

        public StreamDeckLayer GetStreamDeckLayer(bool activateThisLayer, string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return null;
            }

            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.Name == layerName)
                {
                    if (activateThisLayer)
                    {
                        SetActiveLayer(layerName);
                    }

                    return streamDeckLayer;
                }
            }

            throw new Exception("GetStreamDeckLayer : Failed to find layer [" + layerName + "].");
        }

        public StreamDeckButton GetStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName, string layerName, bool throwExceptionIfNotFound = true)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return null;
            }

            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.Name == layerName && streamDeckLayer.ContainStreamDeckButton(streamDeckButtonName))
                {
                    return streamDeckLayer.GetStreamDeckButtonName(streamDeckButtonName);
                }
            }

            if (throwExceptionIfNotFound)
            {
                throw new Exception("Button " + streamDeckButtonName + " cannot be found in layer " + layerName + ".");
            }

            var streamDeckButton = new StreamDeckButton(streamDeckButtonName);
            GetStreamDeckLayer(true, layerName).AddButton(streamDeckButton);
            return streamDeckButton;
        }

        public StreamDeckButton GetActiveLayerStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return GetStreamDeckButton(streamDeckButtonName, ActiveLayer, false);
        }

        public StreamDeckButton GetActiveLayerStreamDeckButton(int streamDeckButtonNumber)
        {
            var streamDeckButtonName = StreamDeckFunction.ButtonName(streamDeckButtonNumber);
            return GetStreamDeckButton(streamDeckButtonName, ActiveLayer, false);
        }

        public StreamDeckButton AddStreamDeckButtonToActiveLayer(StreamDeckButton streamDeckButton)
        {
            StreamDeckButton result = null;

            var activeLayer = GetActiveStreamDeckLayer();

            var found = false;

            foreach (var button in activeLayer.StreamDeckButtons)
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
                activeLayer.AddButton(streamDeckButton);
                result = streamDeckButton;
            }

            return result;
        }
        
        public string ActiveLayer
        {
            get => _activeLayer;
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
                foreach (var streamDeckLayer in LayerList)
                {
                    if (streamDeckLayer.Name == value)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    throw new Exception("StreamDeckLayerHandler : Failed to find layer " + value + " in order to mark it active.");
                }

                SetActiveLayer(value);
            }
        }
        
        public bool HasLayers
        {
            get { return _layerList.Count > 0; }
        }

        public void ClearAllFaces()
        {
            for (var i = 0; i < 15; i++)
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
                _activeLayer = _layerHistory.Last();
                _layerHistory.RemoveAt(_layerHistory.Count -1 );
                ShowLayer(_activeLayer);
            }
        }

        public void ShowHomeLayer()
        {
            ShowLayer(_homeLayer);
        }

        public void ShowActiveLayer()
        {
            ShowLayer(_activeLayer);
        }

        public void ShowLayer(string layerName)
        {
            ClearAllFaces();

            var layer = GetStreamDeckLayer(false, layerName);

            foreach (var streamDeckButton in layer.StreamDeckButtons)
            {
                streamDeckButton.Show(_streamDeckRequisite);
            }
        }

        private void SetActiveLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName) || _layerList.Count == 0)
            {
                _layerHistory.Clear();
                _activeLayer = "";
                ClearAllFaces();
                return;
            }
            _layerHistory.Add(_activeLayer);
            _activeLayer = layerName;
            ShowActiveLayer();
        }
    }
}
