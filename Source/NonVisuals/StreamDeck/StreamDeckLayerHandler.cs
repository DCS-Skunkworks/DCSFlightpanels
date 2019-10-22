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

        HÄR!
            also some form of JSON verification?
        public string ExportJSONSettings()
        {
            return JsonConvert.SerializeObject(_layerList, Formatting.Indented);
        }

        public void ImportJSONSettings(string jsonText)
        {
            _layerList = JsonConvert.DeserializeObject<List<StreamDeckLayer>>(jsonText);
        }

        private void Add(bool isActive, bool isHomeLayer, string layerName)
        {
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
                _layerList.Add(layer);
            }
        }

        public void AddLayer(string layerName)
        {
            if (layerName.Contains(SEPARATOR_CHARS))
            {
                //Setting loaded, includes HID Instance and may contain many layers separated by |
                var layers = layerName.Split(new[] { SEPARATOR_CHARS }, StringSplitOptions.RemoveEmptyEntries)[0];
                layers = layers.Replace("Layers{", "").Replace("}", "");
                var layerArray = layers.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in layerArray)
                {
                    if (s.Contains(HOME_LAYER_ID))
                    {
                        Add(false, true, s.Replace(HOME_LAYER_ID, ""));
                    }
                    else
                    {
                        Add(false, false, s);
                    }
                }
            }
            else
            {
                Add(false, false, layerName);
            }
        }

        public void AddLayer(StreamDeckLayer streamDeckLayer)
        {
            if (!LayerList.Contains(streamDeckLayer) && streamDeckLayer != null)
            {
                LayerList.Add(streamDeckLayer);
            }
        }

        public void DeleteLayer(string layerName)
        {
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

            var streamDeckButton = new StreamDeckButton(false, streamDeckButtonName);
            GetStreamDeckLayer(layerName).AddButton(streamDeckButton);
            return streamDeckButton;
        }

        public StreamDeckButton GetCurrentLayerStreamDeckButton(StreamDeckButtonNames streamDeckButtonName)
        {
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
                throw new Exception("StreamDeckHandler : No layer is currently marked as active.");
            }
            set
            {
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

        private void DoLayerChangeUpdate()
        {

        }
    }
}
