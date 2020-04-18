using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{



    public class StreamDeckButton
    {
        private EnumStreamDeckButtonNames _enumStreamDeckButtonName;
        private IStreamDeckButtonFace _buttonFace = null;
        private IStreamDeckButtonAction _buttonActionForPress = null;
        private IStreamDeckButtonAction _buttonActionForRelease = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Thread _keyPressedThread;
        private string _streamDeckInstanceId;
        private static List<StreamDeckButton> streamDeckButtons = new List<StreamDeckButton>();
        private bool _isVisible = false;

        public StreamDeckButton(EnumStreamDeckButtonNames enumStreamDeckButton)
        {
            _enumStreamDeckButtonName = enumStreamDeckButton;
            streamDeckButtons.Add(this);
        }

        ~StreamDeckButton()
        {
            streamDeckButtons.Remove(this);
        }

        public static StreamDeckButton Get(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return streamDeckButtons.Find(o => o.StreamDeckButtonName == streamDeckButtonName);
        }

        public static List<StreamDeckButton> GetButtons()
        {
            return streamDeckButtons;
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

        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _enumStreamDeckButtonName;
            set => _enumStreamDeckButtonName = value;
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

        public void Consume(StreamDeckButton streamDeckButton)
        {
            StreamDeckButtonName = streamDeckButton.StreamDeckButtonName;
            ActionForPress = streamDeckButton.ActionForPress;
            ActionForRelease = streamDeckButton.ActionForRelease;

            Face = streamDeckButton.Face;
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

        public static HashSet<StreamDeckButton> GetAllStreamDeckButtonNames()
        {
            var result = new HashSet<StreamDeckButton>();

            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON1));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON2));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON3));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON4));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON5));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON6));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON7));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON8));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON9));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON10));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON11));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON12));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON13));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON14));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON15));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON16));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON17));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON18));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON19));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON20));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON21));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON22));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON23));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON24));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON25));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON26));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON27));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON28));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON29));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON30));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON31));
            result.Add(new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON32));
            return result;
        }

        public string StreamDeckInstanceId
        {
            get => _streamDeckInstanceId;
            set => _streamDeckInstanceId = value;
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

    public static class StreamDeckFunction
    {
        public static int ButtonNumber(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            if (streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return 0;
            }

            return int.Parse(streamDeckButtonName.ToString().Replace("BUTTON", ""));
        }

        public static EnumStreamDeckButtonNames ButtonName(int streamDeckButtonNumber)
        {
            if (streamDeckButtonNumber == 0)
            {
                return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }

        public static EnumStreamDeckButtonNames ButtonName(string streamDeckButtonNumber)
        {
            if (string.IsNullOrEmpty(streamDeckButtonNumber) || streamDeckButtonNumber == "0")
            {
                return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }
    }

}
