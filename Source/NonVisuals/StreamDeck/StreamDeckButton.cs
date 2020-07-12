using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{


    [Serializable]
    public class StreamDeckButton : IDisposable
    {
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private IStreamDeckButtonFace _buttonFace = null;
        private IStreamDeckButtonAction _buttonActionForPress = null;
        private IStreamDeckButtonAction _buttonActionForRelease = null;
        [NonSerialized] private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        [NonSerialized] private Thread _keyPressedThread;
        [NonSerialized]
        private StreamDeckPanel _streamDeckPanel;
        private volatile bool _isVisible = false;
        
        [NonSerialized] private static List<StreamDeckButton> _staticStreamDeckButtons = new List<StreamDeckButton>();








        public StreamDeckButton(EnumStreamDeckButtonNames enumStreamDeckButton, StreamDeckPanel streamDeckPanel)
        {
            _streamDeckButtonName = enumStreamDeckButton;
            _streamDeckPanel = streamDeckPanel;
            _staticStreamDeckButtons.Add(this);
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
                _cancellationTokenSource?.Dispose();
                _staticStreamDeckButtons.Remove(this);
                IsVisible = false;
                _buttonFace?.Dispose();
                _buttonActionForPress = null;
                _buttonActionForRelease = null;
                _staticStreamDeckButtons.Remove(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StreamDeckButton()
        {
            Dispose(false);
        }
        
        public static StreamDeckButton GetStatic(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return _staticStreamDeckButtons.Find(o => o.StreamDeckButtonName == streamDeckButtonName);
        }

        public static void DisposeAll()
        {
            for (var i = 0; i < _staticStreamDeckButtons.Count; i++)
            {
                var streamDeckButton= _staticStreamDeckButtons[i];
                streamDeckButton.Dispose();
            }
        }

        public static List<StreamDeckButton> GetStaticButtons(StreamDeckPanel streamDeckPanelInstance)
        {
            if (streamDeckPanelInstance == null)
            {
                return _staticStreamDeckButtons;
            }
            
            return _staticStreamDeckButtons.FindAll(o => o.StreamDeckPanelInstance.BindingHash == streamDeckPanelInstance.BindingHash).ToList();
        }

        public void DoPress()
        {
            if (ActionForPress == null)
            {
                return;
            }

            while (ActionForPress.IsRunning())
            {
                _cancellationTokenSource.Cancel();
            }

            if (ActionForPress.IsRepeatable())
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var threadCancellationToken = _cancellationTokenSource.Token;
                _keyPressedThread = new Thread(() => ThreadedPress(threadCancellationToken));
                _keyPressedThread.Start();
            }
            else
            {
                ActionForPress.Execute(CancellationToken.None);
            }
        }

        public void ClearConfiguration()
        {
            _buttonActionForPress = null;
            _buttonActionForRelease = null;
            _buttonFace?.Dispose();
            _buttonFace = null;
        }

        public void ClearFace()
        {
            _streamDeckPanel.ClearFace(_streamDeckButtonName);
        }

        private void ThreadedPress(CancellationToken threadCancellationToken)
        {
            var first = true;
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                if (!ActionForPress.IsRunning())
                {
                    ActionForPress?.Execute(threadCancellationToken);
                }


                if (first)
                {
                    Thread.Sleep(500);
                    first = false;
                }
                else
                {
                    Thread.Sleep(25);
                }
            }
        }

        public void DoRelease(CancellationToken threadCancellationToken)
        {
            _cancellationTokenSource.Cancel();

            if (ActionForRelease == null)
            {
                return;
            }

            if (!ActionForRelease.IsRunning())
            {
                ActionForRelease?.Execute(threadCancellationToken);
            }
        }

        public bool CheckIfWouldOverwrite(StreamDeckButton newStreamDeckButton)
        {
            var result = _buttonFace != null && newStreamDeckButton.Face != null || 
                         _buttonActionForPress != null && newStreamDeckButton.ActionForPress != null || 
                         _buttonActionForRelease != null && newStreamDeckButton.ActionForRelease != null;
            return result;
        }
        
        public void Consume(StreamDeckButton newStreamDeckButton)
        {
            if (!this.Equals(newStreamDeckButton))
            {
                ClearConfiguration();
            }
            ActionForPress = newStreamDeckButton.ActionForPress;
            ActionForRelease = newStreamDeckButton.ActionForRelease;
            Face = newStreamDeckButton.Face;
            if (Face != null)
            {
                Face.StreamDeckButtonName = _streamDeckButtonName;
            }
        }

        public bool Consume(bool overwrite, StreamDeckButton streamDeckButton)
        {
            var result = false;

            if (_buttonFace != null && streamDeckButton.Face != null)
            {
                if (overwrite)
                {
                    Face?.Dispose();
                    Face = streamDeckButton.Face;
                    Face.StreamDeckButtonName = _streamDeckButtonName;
                    result = true;
                }
            }
            else if (_buttonFace == null && streamDeckButton.Face != null)
            {
                Face = streamDeckButton.Face;
                Face.StreamDeckButtonName = _streamDeckButtonName;
                result = true;
            }

            if (_buttonActionForPress != null && streamDeckButton.ActionForPress != null)
            {
                if (overwrite)
                {
                    _buttonActionForPress = streamDeckButton.ActionForPress;
                    result = true;
                }
            }
            else if (_buttonActionForPress == null && streamDeckButton.ActionForPress != null)
            {
                _buttonActionForPress = streamDeckButton.ActionForPress;
                result = true;
            }


            if (_buttonActionForRelease != null && streamDeckButton.ActionForRelease != null)
            {
                if (overwrite)
                {
                    _buttonActionForRelease = streamDeckButton.ActionForRelease;
                    result = true;
                }
            }
            else if (_buttonActionForRelease == null && streamDeckButton.ActionForRelease != null)
            {
                _buttonActionForRelease = streamDeckButton.ActionForRelease;
                result = true;
            }

            return result;
        }

        [JsonIgnore]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                if (_buttonFace != null)
                {
                    _buttonFace.IsVisible = value;
                }
            }
        }

        public string Description
        {
            get
            {
                var stringBuilder = new StringBuilder();
                if (ActionForPress != null)
                {
                    stringBuilder.Append("ActionPress : ").Append(_buttonActionForPress.ActionDescription).Append(" ");
                }
                if (ActionForRelease != null)
                {
                    stringBuilder.Append("ActionRelease : ").Append(_buttonActionForRelease.ActionDescription).Append(" ");
                }
                if (Face != null)
                {
                    stringBuilder.Append(Face.FaceDescription).Append(" ");
                }

                return stringBuilder.ToString();
            }
        }

        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public IStreamDeckButtonFace Face
        {
            get => _buttonFace;
            set => _buttonFace = value;
        }

        public IStreamDeckButtonAction ActionForPress
        {
            get => _buttonActionForPress;
            set => _buttonActionForPress = value;
        }

        public IStreamDeckButtonAction ActionForRelease
        {
            get => _buttonActionForRelease;
            set => _buttonActionForRelease = value;
        }

        public bool HasConfig =>
            _buttonFace != null ||
            _buttonActionForPress != null ||
            _buttonActionForRelease != null;

        public EnumStreamDeckActionType ActionType
        {
            get
            {
                var result = EnumStreamDeckActionType.Unknown;
                if (ActionForPress != null)
                {
                    result = ActionForPress.ActionType;
                }
                else if (ActionForRelease != null)
                {
                    result = ActionForRelease.ActionType;
                }

                return result;
            }
        }

        public EnumStreamDeckFaceType FaceType
        {
            get
            {
                var result = EnumStreamDeckFaceType.Unknown;
                if (Face != null)
                {
                    result = Face.FaceType;
                }

                return result;
            }
        }
       
        [JsonIgnore]
        public StreamDeckPanel StreamDeckPanelInstance
        {
            get => _streamDeckPanel;
            set
            {
                _streamDeckPanel = value;
                if (_buttonFace != null)
                {
                    _buttonFace.StreamDeckPanelInstance = value;
                }
                if (_buttonActionForPress != null)
                {
                    _buttonActionForPress.StreamDeckPanelInstance = value;
                }
                if (_buttonActionForRelease != null)
                {
                    _buttonActionForRelease.StreamDeckPanelInstance = value;
                }
            }
        }

        public int GetHash()
        {
            unchecked
            {
                var result = _buttonFace?.GetHash() ?? 0;
                result = (result * 397) ^ (_buttonActionForPress?.GetHash() ?? 0);
                result = (result * 397) ^ (_buttonActionForRelease?.GetHash() ?? 0);
                result = (result * 397) ^ StreamDeckButtonName.GetHashCode();
                return result;
            }
        }
    }


    public enum EnumStreamDeckButtonNames
    {
        BUTTON0_NO_BUTTON,
        BUTTON1,
        BUTTON2,
        BUTTON3,
        BUTTON4,
        BUTTON5,
        BUTTON6,
        BUTTON7,
        BUTTON8,
        BUTTON9,
        BUTTON10,
        BUTTON11,
        BUTTON12,
        BUTTON13,
        BUTTON14,
        BUTTON15,
        BUTTON16,
        BUTTON17,
        BUTTON18,
        BUTTON19,
        BUTTON20,
        BUTTON21,
        BUTTON22,
        BUTTON23,
        BUTTON24,
        BUTTON25,
        BUTTON26,
        BUTTON27,
        BUTTON28,
        BUTTON29,
        BUTTON30,
        BUTTON31,
        BUTTON32
    }


}
