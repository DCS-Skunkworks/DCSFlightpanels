using System;
using System.Collections.Generic;
using System.Text;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayerHandler
    {
        private List<string> _layerList = new List<string>();
        private const string SEPARATOR_CHARS = "\\o/";
        private string _homeLayer = "";
        private const string HOME_LAYER_ID = "*home_layer";


        public string ExportLayers()
        {
            var stringBuilder = new StringBuilder();
            var layers = "Layers{";
            foreach (var layer in _layerList)
            {
                if (_homeLayer == layer)
                {
                    layers = layers + "|" + layer + "*home_layer";
                }
                else
                {
                    layers = layers + "|" + layer;
                }
            }
            layers += "}";
            return layers;
        }

        private void Add(string layerName)
        {
            var found = false;
            
            foreach (var layer in _layerList)
            {
                if (layer == layerName)
                {
                    found = true;
                }
            }

            if (!found)
            {
                _layerList.Add(layerName);
            }
        }

        public void AddLayer(string layer)
        {
            if (layer.Contains(SEPARATOR_CHARS))
            {
                //Setting loaded, includes HID Instance and may contain many layers separated by |
                var layers = layer.Split(new[] {SEPARATOR_CHARS}, StringSplitOptions.RemoveEmptyEntries)[0];
                layers = layers.Replace("Layers{", "").Replace("}", "");
                var layerArray = layers.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in layerArray)
                {
                    if (s.Contains(HOME_LAYER_ID))
                    {
                        Add(s.Replace(HOME_LAYER_ID,""));
                        _homeLayer = s;
                    }
                    else
                    {
                        Add(s);
                    }
                }
            }
            else
            {
                Add(layer);
            }
        }

        public void DeleteLayer(string layerName)
        {
            _layerList.Remove(layerName);
        }

        public List<string> LayerList
        {
            get => _layerList;
            set => _layerList = value;
        }

        public void ClearSettings()
        {
            _layerList.Clear();
        }


        public string HomeLayer
        {
            get => _homeLayer;
            set => _homeLayer = value;
        }
    }
}
