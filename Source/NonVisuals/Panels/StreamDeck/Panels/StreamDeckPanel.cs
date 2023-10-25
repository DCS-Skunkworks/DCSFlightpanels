using NonVisuals.Images;

namespace NonVisuals.Panels.StreamDeck.Panels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using ClassLibraryCommon;
    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using OpenMacroBoard.SDK;
    using MEF;
    using EventArgs;
    using Interfaces;
    using Saitek;
    using Events;
    
    using NonVisuals;
    using StreamDeck;
    using NonVisuals.Panels.Saitek.Panels;
    using NonVisuals.Panels;
    using HID;

    public class StreamDeckPanel : GamingPanel, INvStreamDeckListener, IStreamDeckConfigListener, IDisposable
    {
        private readonly bool _unitTesting;

        private readonly Color[] _colors =
            {
                Color.White, Color.Aqua, Color.Black, Color.Blue, Color.BurlyWood, Color.Chartreuse, Color.DarkOrange, Color.Lavender, Color.Silver, Color.Red,
                Color.Yellow, Color.Violet, Color.Thistle, Color.Teal, Color.Salmon, Color.SeaShell, Color.PowderBlue, Color.PaleGreen, Color.Olive, Color.LawnGreen
            };

        private readonly object _updateStreamDeckOledLockObject = new();
        private StreamDeckLayerHandler _streamDeckLayerHandler;
        private readonly int _buttonCount;

        private static readonly object LockObjectStreamDeckPanels = new();
        private static readonly List<StreamDeckPanel> StreamDeckPanels = new();

        private IMacroBoard _streamDeckBoard;
        private readonly IKeyBitmapFactory _keyBitmapFactory = new KeyBitmapFactory();
        private bool _layerSwitched = false; // Must ignore the release of the button used to switch layer

        public IMacroBoard StreamDeckBoard => _streamDeckBoard;
        public int ButtonCount => _buttonCount;

        public string SelectedLayerName { get => _streamDeckLayerHandler.SelectedLayerName; }
        public void SwitchToLayer(string layerName, bool switchedByUser, bool remotelySwitched) { _streamDeckLayerHandler.SwitchToLayer(layerName, switchedByUser, remotelySwitched); }
        public List<string> LayerNameList { get => _streamDeckLayerHandler.GetLayerNameList(); }
        public List<StreamDeckLayer> LayerList { get => _streamDeckLayerHandler.LayerList; }
        public StreamDeckLayer HomeLayer { get => _streamDeckLayerHandler.HomeLayer; }
        public StreamDeckLayer SelectedLayer { get => _streamDeckLayerHandler.SelectedLayer; }
        public bool HasLayers { get { return _streamDeckLayerHandler.HasLayers; } }
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

        public StreamDeckPanel(GamingPanelEnum panelType, HIDSkeleton hidSkeleton, bool unitTesting = false) : base(panelType, hidSkeleton)
        {
            _unitTesting = unitTesting;
            _buttonCount = panelType switch
            {
                GamingPanelEnum.StreamDeckMini => 6,
                GamingPanelEnum.StreamDeckMiniV2 => 6,

                GamingPanelEnum.StreamDeck or
                    GamingPanelEnum.StreamDeckV2 or
                    GamingPanelEnum.StreamDeckMK2 => 15,

                GamingPanelEnum.StreamDeckXL or GamingPanelEnum.StreamDeckXLRev2 => 32,

                GamingPanelEnum.StreamDeckPlus => 8,

                _ => throw new Exception("Failed to determine Stream Deck model")
            };
        }

        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_streamDeckBoard != null) // Null when unit testing
                    {
                        _streamDeckBoard.KeyStateChanged -= StreamDeckKeyListener;
                    }
                    SDEventHandler.DetachStreamDeckListener(this);
                    SDEventHandler.DetachStreamDeckConfigListener(this);

                    _streamDeckLayerHandler.Dispose();
                    _streamDeckBoard?.Dispose();
                    lock (LockObjectStreamDeckPanels)
                    {
                        StreamDeckPanels.Remove(this);
                    }

                    _disposed = true;
                }
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public override void InitPanel()
        {
            try
            {

                if (!_unitTesting)
                {
                    _streamDeckBoard = StreamDeckSharp.StreamDeck.OpenDevice(HIDSkeletonBase.HIDInstance, false);
                    _streamDeckBoard.KeyStateChanged += StreamDeckKeyListener;
                }
                SDEventHandler.AttachStreamDeckListener(this);
                SDEventHandler.AttachStreamDeckConfigListener(this);

                _streamDeckLayerHandler = new StreamDeckLayerHandler(this);
                lock (LockObjectStreamDeckPanels)
                {
                    StreamDeckPanels.Add(this);
                }

                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static StreamDeckPanel GetInstance(string bindingHash)
        {
            lock (LockObjectStreamDeckPanels)
            {
                return StreamDeckPanels.FirstOrDefault(x => x.BindingHash == bindingHash);
            }
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
            lock (LockObjectStreamDeckPanels)
            {
                return StreamDeckPanels;
            }
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
                if (_layerSwitched)
                {
                    // When pressing the button to switch to a new layer we must ignore the next
                    // button release as it would execute on the next layer. Releasing the button
                    // when switching layers should do nothing. 12.10.2023 JDA
                    _layerSwitched = false;
                    return;
                }
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

            var keyBitmap = _keyBitmapFactory.FromStream(bitmap.GetMemoryStream());
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

        public static Bitmap GetBitmapFromPath(string imagePath)
        {
            return BitMapCreator.BitmapOrFileNotFound(imagePath);
        }

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (BindingHash == e.BindingHash && !e.RemotelySwitched && e.SwitchedByUser)
                {
                    _layerSwitched = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (e.RemoteBindingHash == BindingHash && _streamDeckLayerHandler.LayerExists(e.SelectedLayerName))
                {
                    _streamDeckLayerHandler.SwitchToLayer(e.SelectedLayerName, true, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
            }
        }

        public List<StreamDeckButton> GetButtons()
        {
            var result = new List<StreamDeckButton>();

            foreach (var streamDeckLayer in _streamDeckLayerHandler.LayerList)
            {
                foreach (var streamDeckButton in streamDeckLayer.StreamDeckButtons)
                {
                    result.Add(streamDeckButton);
                }
            }

            return result;
        }
    }
}
