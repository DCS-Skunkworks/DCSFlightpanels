using NonVisuals.BindingClasses.Key;

namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    using ClassLibraryCommon;

    using MEF;

    using Newtonsoft.Json;

    using Interfaces;
    using Plugin;
    using Panels;
    using NLog;

    [Serializable]
    public class ActionTypeKey : KeyBindingBase, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.KeyPress;
        public bool IsRepeatable() => true;
        
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private EnumStreamDeckPushRotaryNames _streamDeckPushRotaryName;

        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;


        public ActionTypeKey(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }


        public virtual void Dispose(bool disposing) { }
        public virtual void Dispose() { }

        public new int GetHash()
        {
            unchecked
            {
                var result = _streamDeckButtonName.GetHashCode();
                result = result * 397 ^ base.GetHash();
                return result;
            }
        }

        [JsonIgnore]
        public string ActionDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("Key press");
                if (OSKeyPress != null)
                {
                    stringBuilder.Append(' ').Append(OSKeyPress.GetKeyPressInformation());
                }

                return stringBuilder.ToString();
            }
        }


        public bool IsRunning()
        {
            return OSKeyPress.IsRunning();
        }

        /*
         * We have several options here.
         * 1) There is only 1 command configured and not repeatable => execute it without thread.
         * 2) There are multiple commands.
         *   2a) If Sequenced Execution is configured execute each one in the sequence
         *       when user keeps pressing the button. No Thread.
         *   2b) The multiple commands can be seen as a batch and should be all executed
         *       regardless if the user releases the button. Use Thread.
         *
         * 3) Command is repeatable, for single commands. Just keep executing them, use thread.
         *
         * Commands that must be cancelled by the release of the button are :
         * 1) Repeatable single command.
         */
        public void Execute(CancellationToken threadCancellationToken, bool executeOnce = false)
        {
            Common.PlaySoundFile(SoundFile, Volume);

            if (!PluginManager.DisableKeyboardAPI)
            {
                if (OSKeyPress.HasSequence)
                {
                    OSKeyPress.Execute(threadCancellationToken);
                }
                else
                {
                    if (!executeOnce)
                    {
                        new Thread(() => SendKeysCommandsThread(threadCancellationToken)).Start();
                    }
                    else
                    {
                        OSKeyPress.Execute(threadCancellationToken, false);
                    }
                }
            }

            if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
            {
                PluginManager.DoEvent(
                    DCSAircraft.SelectedAircraft.Description,
                    StreamDeckPanelInstance.HIDInstance,
                    StreamDeckCommon.ConvertEnum(_streamDeckPanel.TypeOfPanel),
                    (int)StreamDeckButtonName,
                    true,
                    OSKeyPress.KeyPressSequence);
            }
        }

        private void SendKeysCommandsThread(CancellationToken threadCancellationToken)
        {
            try
            {
                while (!threadCancellationToken.IsCancellationRequested)
                {
                    OSKeyPress.Execute(threadCancellationToken, false);
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        [JsonProperty("StreamDeckButtonName", Required = Required.Default)]
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        [JsonProperty("StreamDeckPushRotaryName", Required = Required.Default)]
        public EnumStreamDeckPushRotaryNames StreamDeckPushRotaryName
        {
            get => _streamDeckPushRotaryName;
            set => _streamDeckPushRotaryName = value;
        }

        public static HashSet<ActionTypeKey> SetNegators(HashSet<ActionTypeKey> keyBindings)
        {
            return keyBindings;
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }

        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set => _streamDeckPanel = value;
        }

        [JsonProperty("SoundFile", Required = Required.Default)]
        public string SoundFile { get; set; }

        [JsonProperty("Volume", Required = Required.Default)]
        public double Volume { get; set; }

        [JsonProperty("Delay", Required = Required.Default)]
        public int Delay { get; set; }

        [Obsolete]
        [JsonProperty("HasSound", Required = Required.Default)]
        public bool HasSound => !string.IsNullOrEmpty(SoundFile) && File.Exists(SoundFile);


        public void PlaySound()
        {
            Common.PlaySoundFile(SoundFile, Volume);
        }
    }
}
