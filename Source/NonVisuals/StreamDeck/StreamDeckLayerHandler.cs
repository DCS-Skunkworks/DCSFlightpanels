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
        private volatile List<StreamDeckLayer> _layerList = new List<StreamDeckLayer>();
        private const string HOME_LAYER_ID = "*";
        private volatile List<string> _layerHistory = new List<string>();
        private volatile string _activeLayer = "";
        private IStreamDeckBoard _streamDeckBoard;
        private StreamDeckRequisites _streamDeckRequisite = new StreamDeckRequisites();
        






        public StreamDeckLayerHandler(IStreamDeckBoard streamDeckBoard)
        {
            _streamDeckBoard = streamDeckBoard;
            _streamDeckRequisite.StreamDeckBoard = _streamDeckBoard;
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

            CheckActiveLayer();
        }

        private void CheckHomeLayerStatus()
        {
            var found = false;

            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.Name == CommonStreamDeck.HOME_LAYER_NAME)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var streamDeckLayer = new StreamDeckLayer();
                streamDeckLayer.Name = CommonStreamDeck.HOME_LAYER_NAME;
                _layerList.Insert(0, streamDeckLayer);
            }

            CheckActiveLayer();
        }

        public void CheckActiveLayer()
        {
            if (!string.IsNullOrEmpty(_activeLayer) && _layerList.FindAll(o => o.Name == _activeLayer).Count == 1)
            {
                SetActiveLayer(_activeLayer);
            }
            else
            {
                SetActiveLayer(CommonStreamDeck.HOME_LAYER_NAME);
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

            SetActiveLayer(streamDeckLayer.Name);
            return true;
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
                CheckHomeLayerStatus();
                return _layerList.Find(o => o.Name == CommonStreamDeck.HOME_LAYER_NAME);
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
            CheckHomeLayerStatus();
            return GetStreamDeckLayer(_activeLayer);
        }

        public StreamDeckLayer GetStreamDeckLayer(string layerName)
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
            GetStreamDeckLayer(layerName).AddButton(streamDeckButton);
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
        
        public bool HasLayers => _layerList.Count > 0;

        public bool HasActiveLayer => _layerList.Count > 0 && !string.IsNullOrEmpty(_activeLayer);

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
            ShowLayer(CommonStreamDeck.HOME_LAYER_NAME);
        }

        private void ShowLayer(string layerName)
        {
            ClearAllFaces();

            var layer = GetStreamDeckLayer(layerName);

            foreach (var streamDeckButton in layer.StreamDeckButtons)
            {
                streamDeckButton.Show(_streamDeckRequisite);
            }
        }

        private void SetActiveLayer(string layerName)
        {
            ClearAllFaces();

            /*
             * Something is wrong
             */
            if (string.IsNullOrEmpty(layerName))
            {
                
                throw  new Exception("StreamDeckLayerHandler : Trying to set an empty or null layer active.");
            }

            /*
             * There are already layers, add last only if name differs
             */
            if (_activeLayer != layerName)
            {
                _layerHistory.Add(_activeLayer);
            }
            _activeLayer = layerName;
            ShowLayer(_activeLayer);
        }
    }
}
