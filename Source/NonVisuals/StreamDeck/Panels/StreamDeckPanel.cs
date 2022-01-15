namespace NonVisuals.StreamDeck.Panels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Media.Imaging;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;

    using MEF;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Saitek;
    using NonVisuals.Saitek.Panels;
    using NonVisuals.StreamDeck.Events;

    using OpenMacroBoard.SDK;

    using StreamDeckSharp;

    public class StreamDeckPanel : GamingPanel, INvStreamDeckListener, IStreamDeckConfigListener, IDisposable
    {
        private readonly Color[] _colors =
            {
                Color.White, Color.Aqua, Color.Black, Color.Blue, Color.BurlyWood, Color.Chartreuse, Color.DarkOrange, Color.Lavender, Color.Silver, Color.Red,
                Color.Yellow, Color.Violet, Color.Thistle, Color.Teal, Color.Salmon, Color.SeaShell, Color.PowderBlue, Color.PaleGreen, Color.Olive, Color.LawnGreen
            };

        private readonly object _updateStreamDeckOledLockObject = new();
        private readonly StreamDeckLayerHandler _streamDeckLayerHandler;
        private readonly int _buttonCount;
        private static readonly List<StreamDeckPanel> StreamDeckPanels = new();
        private readonly IStreamDeckBoard _streamDeckBoard;
        private static Bitmap _fileNotFoundBitMap;
        public IStreamDeckBoard StreamDeckBoard => _streamDeckBoard;
        public int ButtonCount => _buttonCount;

        public string SelectedLayerName
        {
            get => _streamDeckLayerHandler.SelectedLayerName;
            set => _streamDeckLayerHandler.SelectedLayerName = value;
        }

        public List<string> LayerNameList
        {
            get => _streamDeckLayerHandler.GetLayerNameList();
        }

        public List<StreamDeckLayer> LayerList
        {
            get => _streamDeckLayerHandler.LayerList;
        }

        public StreamDeckLayer HomeLayer
        {
            get => _streamDeckLayerHandler.HomeLayer;
        }

        public StreamDeckLayer SelectedLayer
        {
            get => _streamDeckLayerHandler.SelectedLayer;
            set => _streamDeckLayerHandler.SelectedLayer = value;
        }

        public bool HasLayers
        {
            get { return _streamDeckLayerHandler.HasLayers; }
        }

        public int SelectedButtonNumber
        {
            get => _streamDeckLayerHandler.SelectedButtonNumber;
            set => _streamDeckLayerHandler.SelectedButtonNumber = value;
        }

        public EnumStreamDeckButtonNames SelectedButtonName
        {
            get => _streamDeckLayerHandler.SelectedButtonName;
            set => _streamDeckLayerHandler.SelectedButtonName = value;
        }

        public StreamDeckButton SelectedButton
        {
            get => SelectedLayer.GetStreamDeckButton(SelectedButtonName);
        }

        public StreamDeckPanel(GamingPanelEnum panelType, HIDSkeleton hidSkeleton) : base(panelType, hidSkeleton)
        {

            _buttonCount = panelType switch
            {
                GamingPanelEnum.StreamDeckMini => 6,
                
                GamingPanelEnum.StreamDeck or 
                GamingPanelEnum.StreamDeckV2 or 
                GamingPanelEnum.StreamDeckMK2 => 15,

                GamingPanelEnum.StreamDeckXL => 32,
                _ => throw new Exception("Failed to determine Stream Deck model")
            };

            Startup();
            _streamDeckBoard = StreamDeck.OpenDevice(hidSkeleton.HIDInstance, false);
            _streamDeckBoard.KeyStateChanged += StreamDeckKeyListener;
            _streamDeckLayerHandler = new StreamDeckLayerHandler(this);
            SDEventHandler.AttachStreamDeckListener(this);
            SDEventHandler.AttachStreamDeckConfigListener(this);
            StreamDeckPanels.Add(this);
        }
        
        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StreamDeckButton.DisposeAll();
                    _streamDeckBoard.KeyStateChanged -= StreamDeckKeyListener;
                    _streamDeckBoard?.Dispose();
                    StreamDeckPanels.Remove(this);
                    SDEventHandler.DetachStreamDeckListener(this);
                    SDEventHandler.DetachStreamDeckConfigListener(this);
                    _disposed = true;
                }
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public static StreamDeckPanel GetInstance(string bindingHash)
        {
            return StreamDeckPanels.FirstOrDefault(x => x.BindingHash == bindingHash);
        }

        public static StreamDeckPanel GetInstance(GamingPanel gamingPanel)
        {
            return GetInstance(gamingPanel.BindingHash);
        }

        public override void Identify()
        {
            try
            {
                Thread thread = new(ShowIdentifyingValue);
                thread.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ShowIdentifyingValue()
        {
            try
            {
                int spins = 40;
                Random random = new();
                var ledPositionArray = Enum.GetValues(typeof(SwitchPanelPZ55LEDPosition));
                var panelColorArray = Enum.GetValues(typeof(PanelLEDColor));

                while (spins > 0)
                {
                    var bitmap = BitMapCreator.CreateEmptyStreamDeckBitmap(_colors[random.Next(0, 20)]);
                    SetImage(random.Next(0, ButtonCount - 1), bitmap);

                    Thread.Sleep(50);
                    spins--;
                }

                var blackBitmap = BitMapCreator.CreateEmptyStreamDeckBitmap(Color.Black);
                for (int i = 0; i < ButtonCount; i++)
                {
                    SetImage(i, blackBitmap);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static List<StreamDeckPanel> GetStreamDeckPanels()
        {
            return StreamDeckPanels;
        }

        public sealed override void Startup()
        {
            try
            {
                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void ImportButtons(EnumButtonImportMode importMode, List<ButtonExport> buttonExports)
        {
            _streamDeckLayerHandler.ImportButtons(importMode, buttonExports);
        }

        public List<ButtonExport> GetButtonExports()
        {
            return _streamDeckLayerHandler.GetButtonExports();
        }

        public void Export(string compressedFilenameAndPath, List<ButtonExport> buttonExports)
        {
            _streamDeckLayerHandler.Export(compressedFilenameAndPath, buttonExports);
        }
        
        private void StreamDeckKeyListener(object sender, KeyEventArgs e)
        {
            if (sender is not IMacroBoard)
            {
                return;
            }

            if (!ForwardPanelEvent)
            {
                return;
            }

            var streamDeckButton = _streamDeckLayerHandler.GetSelectedLayerButtonNumber(e.Key + 1);

            if (e.IsDown)
            {
                streamDeckButton.DoPress();
            }
            else
            {
                streamDeckButton.DoRelease();
            }
        }

        protected override void StartListeningForHidPanelChanges()
        { }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (SettingsLoading)
            {
                return;
            }

            UpdateCounter(e.Address, e.Data);
        }

        public void SetImage(int streamDeckButtonNumber, Bitmap bitmap)
        {
            SetImage(StreamDeckCommon.ButtonName(streamDeckButtonNumber), bitmap);
        }

        public void SetImage(EnumStreamDeckButtonNames streamDeckButtonName, Bitmap bitmap)
        {
            if (streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }

            var keyBitmap = KeyBitmap.Create.FromBitmap(bitmap);

            lock (_updateStreamDeckOledLockObject)
            {
                _streamDeckBoard.SetKeyBitmap(StreamDeckCommon.ButtonNumber(streamDeckButtonName) - 1, keyBitmap);
            }
        }

        public void SetImage(EnumStreamDeckButtonNames streamDeckButtonName, BitmapImage bitmapImage)
        {
            if (streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }

            var keyBitmap = KeyBitmap.Create.FromBitmap(BitMapCreator.BitmapImage2Bitmap(bitmapImage));
            lock (_updateStreamDeckOledLockObject)
            {
                _streamDeckBoard.SetKeyBitmap(StreamDeckCommon.ButtonNumber(streamDeckButtonName) - 1, keyBitmap);
            }
        }

        public void ClearAllFaces()
        {
            _streamDeckLayerHandler.ClearAllFaces();
        }

        public void ClearFace(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            _streamDeckLayerHandler.ClearFace(streamDeckButtonName);
        }

        public override List<string> ExportSettings()
        {
            return Closed ? null : new List<string>();
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            SettingsLoading = true;

            if (!string.IsNullOrEmpty(genericPanelBinding.JSONString))
            {
                _streamDeckLayerHandler.ImportJSONSettings(genericPanelBinding.JSONString);
            }

            SettingsLoading = false;
            AppEventHandler.SettingsApplied(this, HIDSkeletonBase.HIDInstance, TypeOfPanel);
        }

        private string ExportJSONSettings()
        {
            return Closed ? null : _streamDeckLayerHandler.ExportJSONSettings();
        }

        public string GetKeyPressForLoggingPurposes(StreamDeckButton streamDeckButton)
        {
            var result = string.Empty;

            /*foreach (var keyBinding in _keyBindings)
                        {
                            if (keyBinding.OSKeyPress != null && keyBinding.EnumStreamDeckButtonName == enumStreamDeckButton.EnumStreamDeckButtonName && keyBinding.WhenTurnedOn == enumStreamDeckButton.IsPressed)
                            {
                                result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                            }
                        }*/
            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e) { }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerCaller.RegisterJSONProfileData(this, ExportJSONSettings());
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
            _streamDeckLayerHandler.ClearSettings();

            if (setIsDirty)
            {
                SetIsDirty();
            }
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            // Set _selectedMode and LCD button statuses
            // and performs the actual actions for key presses
            // ADD METHOD ?
        }
        
        public DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public bool AddLayer(StreamDeckLayer streamDeckLayer)
        {
            SetIsDirty();
            return _streamDeckLayerHandler.AddLayer(streamDeckLayer);
        }

        public void DeleteLayer(string streamDeckLayerName)
        {
            _streamDeckLayerHandler.DeleteLayer(streamDeckLayerName);
            SetIsDirty();
        }

        public void EraseLayerButtons(string streamDeckLayerName)
        {
            _streamDeckLayerHandler.EraseLayerButtons(streamDeckLayerName);
            SetIsDirty();
        }

        public List<string> GetStreamDeckLayerNames()
        {
            return _streamDeckLayerHandler.GetStreamDeckLayerNames();
        }

        public string GetConfigurationInformation()
        {
            return _streamDeckLayerHandler.GetConfigurationInformation();
        }

        public string GetLayerHandlerInformation()
        {
            return $"StreamDeckLayerHandler Instance ID = [{_streamDeckLayerHandler.HIDInstance}] Counter = [{StreamDeckLayerHandler.HIDInstanceCounter}]";
        }

        public StreamDeckButton GetStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName, string layerName)
        {
            return _streamDeckLayerHandler.GetButton(streamDeckButtonName, layerName);
        }

        public StreamDeckButton GetCurrentLayerButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return _streamDeckLayerHandler.GetSelectedLayerButton(streamDeckButtonName);
        }

        public StreamDeckLayer GetLayer(string layerName)
        {
            return _streamDeckLayerHandler.GetLayer(layerName);
        }

        public void ShowHomeLayer()
        {
            _streamDeckLayerHandler.ShowHomeLayer();
        }

        public void ShowPreviousLayer()
        {
            _streamDeckLayerHandler.ShowPreviousLayer();
        }

        public static Bitmap Validate(string imagePath)
        {
            return File.Exists(imagePath) ? new Bitmap(imagePath) : FileNotFoundBitmap();
        }

        public static Bitmap FileNotFoundBitmap()
        {
            if (_fileNotFoundBitMap != null)
            {
                return _fileNotFoundBitMap;
            }

            var assembly = Assembly.GetExecutingAssembly();

            BitmapImage tmpBitMapImage = new();
            using (var stream = assembly.GetManifestResourceStream(@"NonVisuals.Images.filenotfound.png"))
            {
                tmpBitMapImage.BeginInit();
                tmpBitMapImage.StreamSource = stream;
                tmpBitMapImage.CacheOption = BitmapCacheOption.OnLoad;
                tmpBitMapImage.EndInit();
            }

            _fileNotFoundBitMap = BitMapCreator.BitmapImage2Bitmap(tmpBitMapImage);
            return _fileNotFoundBitMap;
        }

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (BindingHash == e.BindingHash)
                { }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (e.RemoteBindingHash == BindingHash && _streamDeckLayerHandler.LayerExists(e.SelectedLayerName))
                {
                    _streamDeckLayerHandler.SelectedLayerName = e.SelectedLayerName;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                if (BindingHash == e.BindingHash)
                { }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e)
        {
            try
            {
                if (BindingHash == e.BindingHash)
                { }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e)
        {
            try
            {
                if (BindingHash == e.BindingHash)
                { }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void ClearSettings(object sender, StreamDeckClearSettingsArgs e)
        {
            try
            {
                if (BindingHash == e.BindingHash)
                { }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }


        public void SyncConfiguration(object sender, StreamDeckSyncConfigurationArgs e)
        {
            try
            {
                if (BindingHash == e.BindingHash)
                { }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void ConfigurationChanged(object sender, StreamDeckConfigurationChangedArgs e)
        {
            try
            {
                if (e.BindingHash == BindingHash)
                {
                    SetIsDirty();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
