using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.StreamDeck.Events;
using OpenMacroBoard.SDK;
using StreamDeckSharp;


namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayerHandler
    {
        private readonly StreamDeckPanel _streamDeckPanel = null;
        private volatile List<StreamDeckLayer> _layerList = new List<StreamDeckLayer>();
        private const string HOME_LAYER_ID = "*";
        private volatile List<string> _layerHistory = new List<string>();
        private volatile string _selectedLayerName = "";
        private readonly IStreamDeckBoard _streamDeckBoard;
        private EnumStreamDeckButtonNames _selectedButtonName = EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;

        private bool _jsonImported = false;

        private static int _instanceIdCounter = 0;
        private readonly int _instanceId = 0;


        private readonly UnicodeEncoding _uniCodeEncoding = new UnicodeEncoding();
        private const Formatting INDENTED_FORMATTING = Formatting.Indented;
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) =>
            {
                Common.LogError("JSON Error.\n" + args.ErrorContext.Error.Message);
            }
        };


        public StreamDeckLayerHandler(StreamDeckPanel streamDeckPanel)
        {
            _instanceId = _instanceIdCounter++;
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

            _jsonSettings.MissingMemberHandling = MissingMemberHandling.Error;

            _layerList = JsonConvert.DeserializeObject<List<StreamDeckLayer>>(jsonText, _jsonSettings);
            
            _layerList.SetPanel(_streamDeckPanel);
            _jsonImported = true;
            SetStreamDeckPanelInstance(_streamDeckPanel);
            CheckHomeLayerExists();
        }

        public void ImportButtons(EnumButtonImportMode importMode, List<ButtonExport> buttonExports)
        {
            var importLayerNames = buttonExports.Select(o => o.LayerName).Distinct().ToList();

            foreach (var importLayerName in importLayerNames)
            {

                if (!LayerExists(importLayerName))
                {
                    var newLayer = new StreamDeckLayer(_streamDeckPanel) { Name = importLayerName};
                    AddLayer(newLayer);
                }

                var layer = GetLayer(importLayerName);
                layer.ImportButtons(importMode, buttonExports);
            }

            SetStreamDeckPanelInstance(_streamDeckPanel);
        }

        public List<ButtonExport> GetButtonExports()
        {
            var result = new List<ButtonExport>();

            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.HasConfig)
                {
                    var list = streamDeckLayer.GetButtonsWithConfig();
                    foreach (var streamDeckButton in list)
                    {
                        var clonedButton = streamDeckButton.DeepClone();
                        var exportButton = new ButtonExport(streamDeckLayer.Name, clonedButton);
                        result.Add(exportButton);
                    }
                }
            }

            return result;
        }
        
        public void Export(string compressedFilenameAndPath, List<ButtonExport> buttonExports)
        {
            var filesToCompressList = new List<string>(); //includes the json file and eventual image files

            StreamDeckCommon.CleanDCSFPTemporaryFolder();
            /*
             * Close because the list changes below. If not then subsequent operations by the user
             * will cause null exceptions since image path is reset.
             */
            var clonedButtonExports = buttonExports.DeepClone();

            foreach (var buttonExport in clonedButtonExports)
            {
                if (buttonExport.Button.Face != null)
                {
                    if (buttonExport.Button.Face.GetType() == typeof(DCSBIOSDecoder))
                    {
                        var decoder = ((DCSBIOSDecoder)buttonExport.Button.Face);

                        foreach (var imageFile in decoder.ImageFiles)
                        {
                            filesToCompressList.Add(imageFile);
                        }
                        /*
                         * We must remove any path imageFilePath.
                         * When importing a path will be added back following whatever folder
                         * the user wants the image files to reside in.
                         */
                        decoder.ResetImageFilePaths();
                    }
                    else if (buttonExport.Button.Face.GetType() == typeof(FaceTypeImage))
                    {
                        var faceTypeImage = ((FaceTypeImage)buttonExport.Button.Face);
                        filesToCompressList.Add(faceTypeImage.ImageFile);
                        faceTypeImage.ImageFile = Path.GetFileName(faceTypeImage.ImageFile);
                    }
                    else if (buttonExport.Button.Face.GetType() == typeof(FaceTypeDCSBIOSOverlay))
                    {
                        var faceTypeDCSBIOSOverlay = ((FaceTypeDCSBIOSOverlay)buttonExport.Button.Face);
                        filesToCompressList.Add(faceTypeDCSBIOSOverlay.BackgroundBitmapPath);
                        faceTypeDCSBIOSOverlay.BackgroundBitmapPath = Path.GetFileName(faceTypeDCSBIOSOverlay.BackgroundBitmapPath);
                    }
                }
            }

            var json = JsonConvert.SerializeObject(clonedButtonExports, INDENTED_FORMATTING, _jsonSettings);
            var chars = _uniCodeEncoding.GetChars(_uniCodeEncoding.GetBytes(json));

            var filename = StreamDeckCommon.GetDCSFPTemporaryFolder() + "\\" + StreamDeckConstants.BUTTON_EXPORT_FILENAME;
            filesToCompressList.Add(filename);

            using (var streamWriter = File.CreateText(filename))
            {
                streamWriter.Write(chars);
            }

            ZipArchiver.CreateZipFile(compressedFilenameAndPath, filesToCompressList.Distinct().ToList());

            SystemSounds.Asterisk.Play();
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
                var streamDeckLayer = new StreamDeckLayer(_streamDeckPanel);
                streamDeckLayer.Name = StreamDeckConstants.HOME_LAYER_NAME;
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

        public List<StreamDeckLayer> LayerList
        {
            get => _layerList;
            set => _layerList = value;
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
                    throw new Exception("StreamDeckLayerHandler : Failed to find layer " + value + " in order to mark it selected.");
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
            foreach (var streamDeckLayer in _layerList)
            {
                streamDeckLayer.RemoveEmptyButtons();
            }
        }

        public string GetConfigurationInformation()
        {
            var stringBuilder = new StringBuilder(500);
            stringBuilder.Append("\n");

            stringBuilder.Append("Layer count : " + _layerList.Count + ", button count = " + StreamDeckButton.GetStaticButtons(null).Count + "\n");
            stringBuilder.Append("Existing layers:\n");
            foreach (var streamDeckLayer in _layerList)
            {
                stringBuilder.Append("\t" + streamDeckLayer.Name + " (" + streamDeckLayer.LayerStreamDeckButtons.Count + ")\n");
            }
            stringBuilder.Append("\n");

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
            foreach (var streamDeckButton in StreamDeckButton.GetStaticButtons(_streamDeckPanel))
            {
                streamDeckButton.IsVisible = false;
            }
            ClearAllFaces();
        }

        private void SetSelectedLayer(string layerName)
        {
            if (layerName == _selectedLayerName && _jsonImported == false)
            {
                return;
            }

            //We want one update after having read the json
            _jsonImported = false;

            MarkAllButtonsHiddenAndClearFaces();

            /*
             * Something is wrong
             */
            if (string.IsNullOrEmpty(layerName))
            {
                throw new Exception("Internal Error : StreamDeckLayerHandler : Trying to set an empty or null layer selected.");
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

            EventHandlers.LayerSwitched(this, new StreamDeckShowNewLayerArgs() { SelectedLayerName = _selectedLayerName });
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

        public StreamDeckButton GetSelectedLayerButton(int streamDeckButtonNumber)
        {
            var streamDeckButtonName = StreamDeckCommon.ButtonName(streamDeckButtonNumber);
            return GetButton(streamDeckButtonName, SelectedLayerName, false);
        }

        public bool LayerExists(string layerName)
        {
            var result = false;

            foreach (var streamDeckLayer in _layerList)
            {
                if (streamDeckLayer.Name == layerName)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static int InstanceIdCounter => _instanceIdCounter;

        public int InstanceId => _instanceId;
    }
}
