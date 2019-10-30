using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayerHandler
    {
        private List<StreamDeckLayer> _layerList = new List<StreamDeckLayer>();
        private const string SEPARATOR_CHARS = "\\o/";
        private const string HOME_LAYER_ID = "*";

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
        }

        public void SetHomeLayerStatus(bool isHomeLayer, string layerName)
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
        }

        public void SetHomeLayerStatus(bool isHomeLayer, StreamDeckLayer streamDeckLayer)
        {
            SetHomeLayerStatus(isHomeLayer, streamDeckLayer.Name);
        }

        private bool Add(bool isActive, bool isHomeLayer, string layerName)
        {
            var result = isHomeLayer;

            if (string.IsNullOrEmpty(layerName))
            {
                return result;
            }

            var found = false;

            foreach (var layer in _layerList)
            {
                if (layer.Name == layerName)
                {
                    found = true;
                }
            }

            if (!found)
            {
                var layer = new StreamDeckLayer();
                layer.Name = layerName;
                layer.IsHomeLayer = isHomeLayer;
                layer.IsActive = isActive;
                if (_layerList.Count == 0)
                {
                    layer.IsHomeLayer = true;
                    result = true;
                }
                _layerList.Add(layer);
            }

            return result;
        }

        public bool AddLayer(StreamDeckLayer streamDeckLayer)
        {
            var result = streamDeckLayer.IsHomeLayer;

            if (!LayerList.Contains(streamDeckLayer) && streamDeckLayer != null)
            {
                if (LayerList.Count == 0)
                {
                    streamDeckLayer.IsHomeLayer = true;
                }
                LayerList.Add(streamDeckLayer);
            }

            return result;
        }

        public void DeleteLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return;
            }

            _layerList.RemoveAll(x => x.Name == layerName);
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

            throw new Exception("GetStreamDeckLayer : Failed to find layer " + layerName + ".");
        }

        public StreamDeckButton GetStreamDeckButton(StreamDeckButtonNames streamDeckButtonName, string layerName, bool throwExceptionIfNotFound = true)
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

        public StreamDeckButton GetCurrentLayerStreamDeckButton(StreamDeckButtonNames streamDeckButtonName)
        {
            return GetStreamDeckButton(streamDeckButtonName, CurrentLayerName, false);
        }

        public StreamDeckButton GetCurrentLayerStreamDeckButton(int streamDeckButtonNumber)
        {
            var streamDeckButtonName = StreamDeckFunction.ButtonName(streamDeckButtonNumber);
            return GetStreamDeckButton(streamDeckButtonName, CurrentLayerName, false);
        }

        public void AddStreamDeckButtonToCurrentLayer(StreamDeckButton streamDeckButton)
        {
            foreach (var streamDeckLayer in LayerList)
            {
                if (streamDeckLayer.IsActive)
                {
                    foreach (var button in streamDeckLayer.StreamDeckButtons)
                    {
                        if (button.StreamDeckButtonName == streamDeckButton.StreamDeckButtonName)
                        {
                            button.Consume(streamDeckButton);
                            break;
                        }
                    }
                }
            }
        }
        
        public string CurrentLayerName
        {
            get
            {
                foreach (var streamDeckLayer in LayerList)
                {
                    if (streamDeckLayer.IsActive)
                    {
                        return streamDeckLayer.Name;
                    }
                }

                return "";
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
                foreach (var streamDeckLayer in LayerList)
                {
                    streamDeckLayer.IsActive = false;
                    if (streamDeckLayer.Name == value)
                    {
                        streamDeckLayer.IsActive = true;
                        found = true;
                    }
                }

                if (!found)
                {
                    throw new Exception("StreamDeckLayerHandler : Failed to find layer " + value + " in order to mark it active.");
                }
                DoLayerChangeUpdate();
            }
        }

        public bool NoLayerIsActive
        {
            get
            {
                var notFound = true;
                foreach (var streamDeckLayer in LayerList)
                {
                    if (streamDeckLayer.IsActive)
                    {
                        notFound = false;
                        break;
                    }
                }

                return notFound;
            }
        }

        public bool HasLayers
        {
            get { return _layerList.Count > 0; }
        }

        private void CleanLayers()
        {
            foreach (var streamDeckLayer in _layerList)
            {
                streamDeckLayer.RemoveEmptyButtons();
            }
        }

        private void DoLayerChangeUpdate()
        {

        }
    }
}
