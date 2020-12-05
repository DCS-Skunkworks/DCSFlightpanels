using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.Saitek.Panels;
using NonVisuals.StreamDeck.Events;
using OpenMacroBoard.SDK;
using StreamDeckSharp;
using Theraot.Core;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckPanel : GamingPanel, INvStreamDeckListener, IStreamDeckConfigListener, IDisposable
    {
        private IStreamDeckBoard _streamDeckBoard;
        private int _lcdKnobSensitivity;
        private readonly StreamDeckLayerHandler _streamDeckLayerHandler;
        private readonly object _lcdLockObject = new object();
        private readonly object _lcdDataVariablesLockObject = new object();

        private static readonly List<StreamDeckPanel> StreamDeckPanels = new List<StreamDeckPanel>();

        private int _buttonCount = 0;

        private object _updateStreamDeckOledLockObject = new object();



        public StreamDeckPanel(GamingPanelEnum panelType, HIDSkeleton hidSkeleton) : base(panelType, hidSkeleton)
        {
            switch (panelType)
            {
                case GamingPanelEnum.StreamDeckMini:
                    {
                        _buttonCount = 6;
                        break;
                    }
                case GamingPanelEnum.StreamDeck:
                    {
                        _buttonCount = 15;
                        break;
                    }
                case GamingPanelEnum.StreamDeckXL:
                    {
                        _buttonCount = 32;
                        break;
                    }
            }
            Startup();
            _streamDeckBoard = StreamDeckSharp.StreamDeck.OpenDevice(hidSkeleton.InstanceId, false);
            _streamDeckBoard.KeyStateChanged += StreamDeckKeyListener;
            _streamDeckLayerHandler = new StreamDeckLayerHandler(this);
            EventHandlers.AttachStreamDeckListener(this);
            EventHandlers.AttachStreamDeckConfigListener(this);
            StreamDeckPanels.Add(this);
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                StreamDeckButton.DisposeAll();
                _streamDeckBoard.KeyStateChanged -= StreamDeckKeyListener;
                _streamDeckBoard?.Dispose();
                StreamDeckPanels.Remove(this);
                EventHandlers.DetachStreamDeckListener(this);
                EventHandlers.DetachStreamDeckConfigListener(this);
                Closed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StreamDeckPanel()
        {
            Dispose(false);
        }

        public static StreamDeckPanel GetInstance(string bindingHash)
        {
            foreach (var streamDeckPanel in StreamDeckPanels)
            {
                if (streamDeckPanel.BindingHash == bindingHash)
                {
                    return streamDeckPanel;
                }
            }

            return null;
        }

        public static StreamDeckPanel GetInstance(GamingPanel gamingPanel)
        {
            foreach (var streamDeckPanel in StreamDeckPanels)
            {
                if (streamDeckPanel.BindingHash == gamingPanel.BindingHash)
                {
                    return streamDeckPanel;
                }
            }

            return null;
        }


        public override void Identify()
        {
            try
            {
                var thread = new Thread(ShowIdentifyingValue);
                thread.Start();
            }
            catch (Exception e)
            {
            }
        }

        private readonly Color[] _colors = new Color[]
        {
            Color.White, Color.Aqua, Color.Black, Color.Blue, Color.BurlyWood, Color.Chartreuse, Color.DarkOrange, Color.Lavender, Color.Silver, Color.Red,
            Color.Yellow, Color.Violet, Color.Thistle, Color.Teal, Color.Salmon, Color.SeaShell, Color.PowderBlue, Color.PaleGreen, Color.Olive, Color.LawnGreen
        };

        private void ShowIdentifyingValue()
        {
            try
            {
                var spins = 40;
                var random = new Random();
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
                for (var i = 0; i < ButtonCount; i++)
                {
                    SetImage(i, blackBitmap);
                }
            }
            catch (Exception e)
            {
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
                StartListeningForPanelChanges();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
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

        public override void SelectedProfile(object sender, AirframeEventArgs e)
        {
            _streamDeckLayerHandler.ClearSettings();
        }

        private void StreamDeckKeyListener(object sender, KeyEventArgs e)
        {
            if (!(sender is IMacroBoard))
            {
                return;
            }

            if (!ForwardPanelEvent)
            {
                return;
            }

            var streamDeckButton = _streamDeckLayerHandler.GetSelectedLayerButton(e.Key + 1);

            if (e.IsDown)
            {
                streamDeckButton.DoPress();
            }
            else
            {
                streamDeckButton.DoRelease(CancellationToken.None);
            }
        }

        protected override void StartListeningForPanelChanges()
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
            if (Closed)
            {
                return null;
            }
            return new List<string>();
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
            SettingsApplied();
        }

        private string ExportJSONSettings()
        {
            if (Closed)
            {
                return null;
            }

            var str = _streamDeckLayerHandler.ExportJSONSettings();
            return str;
        }

        public string GetKeyPressForLoggingPurposes(StreamDeckButton streamDeckButton)
        {
            var result = "";
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
            e.ProfileHandlerEA.RegisterJSONProfileData(this, ExportJSONSettings());
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
            //Set _selectedMode and LCD button statuses
            //and performs the actual actions for key presses
            // ADD METHOD ?
        }


        private void StreamDeckButtonChanged(IEnumerable<object> hashSet)
        {
            if (!ForwardPanelEvent)
            {
                return;
            }
        }

        private void DeviceAttachedHandler()
        {
            Startup();
            //IsAttached = true;
        }

        private void DeviceRemovedHandler()
        {
            Dispose();
            //IsAttached = false;
        }

        public DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public int LCDKnobSensitivity
        {
            get => _lcdKnobSensitivity;
            set => _lcdKnobSensitivity = value;
        }


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

        public StreamDeckLayer HomeLayer
        {
            get => _streamDeckLayerHandler.HomeLayer;
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
            return "StreamDeckLayerHandler Instance ID = ".Append(_streamDeckLayerHandler.InstanceId.ToString()).Append(" Counter = ").Append(StreamDeckLayerHandler.InstanceIdCounter.ToString());
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

        public StreamDeckLayer SelectedLayer
        {
            get => _streamDeckLayerHandler.SelectedLayer;
            set => _streamDeckLayerHandler.SelectedLayer = value;
        }

        public bool HasLayers
        {
            get { return _streamDeckLayerHandler.HasLayers; }
        }

        public void ShowHomeLayer()
        {
            _streamDeckLayerHandler.ShowHomeLayer();
        }

        public void ShowPreviousLayer()
        {
            _streamDeckLayerHandler.ShowPreviousLayer();
        }

        public IStreamDeckBoard StreamDeckBoard => _streamDeckBoard;

        public int ButtonCount => _buttonCount;

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

        public static Bitmap Validate(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                return new Bitmap(imagePath);
            }

            return FileNotFoundBitmap();
        }

        private static Bitmap _fileNotFoundBitMap = null;

        public static Bitmap FileNotFoundBitmap()
        {
            if (_fileNotFoundBitMap != null)
            {
                return _fileNotFoundBitMap;
            }
            var assembly = Assembly.GetExecutingAssembly();

            var tmpBitMapImage = new BitmapImage();
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
                Common.LogError(ex);
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
                Common.LogError(ex);
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
                Common.LogError(ex);
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
                Common.LogError(ex);
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
                Common.LogError(ex);
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
                Common.LogError(ex);
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
                Common.LogError(ex);
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
                Common.LogError(ex);
            }
        }
    }

}
