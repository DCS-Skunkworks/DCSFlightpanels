using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using ClassLibraryCommon;
using DCS_BIOS;
using Jace.Operations;
using NonVisuals.Saitek;
using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckPanel : GamingPanel
    {
        private IStreamDeckBoard _streamDeckBoard;
        private int _lcdKnobSensitivity;
        private readonly StreamDeckLayerHandler _streamDeckLayerHandler;
        private readonly object _lcdLockObject = new object();
        private readonly object _lcdDataVariablesLockObject = new object();
        private StreamDeckRequisites _streamDeckRequisite = new StreamDeckRequisites();

        private long _doUpdatePanelLCD;



        public StreamDeckPanel(HIDSkeleton hidSkeleton):base(GamingPanelEnum.StreamDeck, hidSkeleton)
        {
            Startup();
            _streamDeckBoard = StreamDeckSharp.StreamDeck.OpenDevice(hidSkeleton.InstanceId, false);
            _streamDeckBoard.KeyStateChanged += StreamDeckKeyListener;
            _streamDeckLayerHandler =  new StreamDeckLayerHandler(_streamDeckBoard);
            _streamDeckRequisite = new StreamDeckRequisites{ StreamDeck = this };
        }

        public sealed override void Startup()
        {
            try
            {
                StartListeningForPanelChanges();
            }
            catch (Exception ex)
            {
                Common.DebugP("StreamDeck.StartUp() : " + ex.Message);
                Common.LogError(321654, ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Closed = true;
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public void AddStreamDeckButtonToActiveLayer(StreamDeckButton streamDeckButton)
        {
            _streamDeckLayerHandler.AddStreamDeckButtonToActiveLayer(streamDeckButton);
            SetIsDirty();
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

            if (e.IsDown)
            {
                var streamDeckButton = _streamDeckLayerHandler.GetActiveLayerStreamDeckButton(e.Key + 1);
                streamDeckButton.DoPress(_streamDeckRequisite);
            }
            else
            {
                var streamDeckButton = _streamDeckLayerHandler.GetActiveLayerStreamDeckButton(e.Key + 1);
                streamDeckButton.DoRelease(_streamDeckRequisite);
            }
        }

        protected override void StartListeningForPanelChanges()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Common.DebugP(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (SettingsLoading)
            {
                return;
            }
            UpdateCounter(e.Address, e.Data);
            
        }

        public override void ImportSettings(List<string> settings)
        {
            SettingsLoading = true;
            //Clear current bindings
            ClearSettings();
            if (settings == null || settings.Count == 0)
            {
                return;
            }

            var stringBuilder = new StringBuilder();

            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2 && setting.Contains(InstanceId))
                {
                    stringBuilder.Append(setting.Replace(Constants.SEPARATOR_SYMBOL, "").Replace(InstanceId, "") + Environment.NewLine);
                }
            }
            _streamDeckLayerHandler.ImportJSONSettings(stringBuilder.ToString());
            SettingsLoading = false;
            SettingsApplied();
        }

        public void SetImage(int streamDeckButtonNumber, Bitmap bitmap)
        {
            SetImage(StreamDeckFunction.ButtonName(streamDeckButtonNumber), bitmap);
        }

        public void SetImage(EnumStreamDeckButtonNames streamDeckButtonName, Bitmap bitmap)
        {
            if (streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return;
            }
            var keyBitmap = KeyBitmap.Create.FromBitmap(bitmap);
            _streamDeckBoard.SetKeyBitmap(StreamDeckFunction.ButtonNumber(streamDeckButtonName) - 1, keyBitmap);
        }

        public void ClearAllFaces()
        {
            _streamDeckLayerHandler.ClearAllFaces();
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }
            return new List<string>();
        }

        private string ExportJSONSettings()
        {
            if (Closed)
            {
                return null;
            }

            return _streamDeckLayerHandler.ExportJSONSettings();
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
        
        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e){        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerEA.RegisterJSONProfileData(this, ExportJSONSettings());
        }

        public override void ClearSettings()
        {
            /*_keyBindings.Clear();
            _osCommandBindings.Clear();
            _dcsBiosBindings.Clear();
            _dcsBiosLcdBindings.Clear();
            _bipLinks.Clear();*/
            _streamDeckLayerHandler.ClearSettings();
        }

        protected override void GamingPanelKnobChanged(IEnumerable<object> hashSet)
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

            foreach (var o in hashSet)
            {
                var streamDeckButton = (StreamDeckButton)o;

                var found = false;
                /*foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.OSKeyPress != null && keyBinding.EnumStreamDeckButtonName == enumStreamDeckButton.EnumStreamDeckButtonName && keyBinding.WhenTurnedOn == enumStreamDeckButton.IsPressed)
                    {
                        keyBinding.OSKeyPress.Execute();
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.EnumStreamDeckButtonName == enumStreamDeckButton.EnumStreamDeckButtonName && dcsBiosBinding.WhenTurnedOn == enumStreamDeckButton.IsPressed)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }
                foreach (var osCommand in _osCommandBindings)
                {
                    if (osCommand.OSCommandObject != null && osCommand.EnumStreamDeckButtonName == enumStreamDeckButton.EnumStreamDeckButtonName && osCommand.WhenTurnedOn == enumStreamDeckButton.IsPressed)
                    {
                        osCommand.OSCommandObject.Execute();
                        found = true;
                        break;
                    }
                }
                foreach (var bipLinkStreamDeck in _bipLinks)
                {
                    if (bipLinkStreamDeck.BIPLights.Count > 0 && bipLinkStreamDeck.EnumStreamDeckButtonName == enumStreamDeckButton.EnumStreamDeckButtonName && bipLinkStreamDeck.WhenTurnedOn == enumStreamDeckButton.IsPressed)
                    {
                        bipLinkStreamDeck.Execute();
                        break;
                    }
                }*/
            }
        }
        
        private void DeviceAttachedHandler()
        {
            Startup();
            //IsAttached = true;
        }

        private void DeviceRemovedHandler()
        {
            Shutdown();
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

        public bool HasActiveLayer
        {
            get => _streamDeckLayerHandler.HasLayers;
        }

        public string ActiveLayer
        {
            get => _streamDeckLayerHandler.ActiveLayer;
            set => _streamDeckLayerHandler.ActiveLayer = value;
        }

        public override string SettingsVersion()
        {
            return "2X";
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
        
        public StreamDeckLayer HomeLayer
        {
            get => _streamDeckLayerHandler.HomeLayer;
        }

        public void SetHomeStatus(bool isHomeLayer, string layerName)
        {
            _streamDeckLayerHandler.SetHomeStatus(isHomeLayer, layerName);
            SetIsDirty();
        }

        public void SetHomeStatus(bool isHomeLayer, StreamDeckLayer streamDeckLayer)
        {
            SetHomeStatus(isHomeLayer, streamDeckLayer.Name);
        }

        public List<string> GetStreamDeckLayerNames()
        {
            return _streamDeckLayerHandler.GetStreamDeckLayerNames();
        }

        public StreamDeckButton GetStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName, string layerName)
        {
            return _streamDeckLayerHandler.GetStreamDeckButton(streamDeckButtonName, layerName);
        }

        public StreamDeckButton GetActiveLayerStreamDeckButton(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return _streamDeckLayerHandler.GetActiveLayerStreamDeckButton(streamDeckButtonName);
        }

        public StreamDeckLayer GetLayer(string layerName)
        {
            return _streamDeckLayerHandler.GetStreamDeckLayer(layerName);
        }
        
        public StreamDeckLayer GetActiveLayer()
        {
            return _streamDeckLayerHandler.GetActiveStreamDeckLayer();
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
        /*
        public void ShowLayer(string layerName)
        {
            _streamDeckLayerHandler.ShowLayer(layerName);
        }*/
    }


    public enum ControlListStreamDeck : byte
    {
        ALL,
        DCSBIOS,
        KEYS,
        BIPS
    }
}
