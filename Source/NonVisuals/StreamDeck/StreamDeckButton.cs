using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{



    public class StreamDeckButton
    {
        private StreamDeckButtonNames _streamDeckButtonName;
        private IStreamDeckButtonFace _buttonFaceForPress = null;
        private IStreamDeckButtonFace _buttonFaceForRelease = null;
        private IStreamDeckButtonAction _buttonActionForPress = null;
        private IStreamDeckButtonAction _buttonActionForRelease = null;
        public int ExecutionDelay { get; set; } = 1000;

        [JsonIgnore]
        private Thread _delayedExecutionThread;
        [JsonIgnore]
        private CancellationTokenSource _cancellationTokenSource;

        public StreamDeckButton(StreamDeckButtonNames streamDeckButton)
        {
            _streamDeckButtonName = streamDeckButton;
        }

        ~StreamDeckButton()
        {
            _delayedExecutionThread?.Abort();
        }

        public void Press(StreamDeckPanel streamDeckPanel)
        {
            ActionForPress?.Execute(new CancellationToken());
            FaceForPress?.Execute(streamDeckPanel);

            if (_buttonFaceForRelease != null)
            {
                _cancellationTokenSource?.Cancel();

                _cancellationTokenSource = new CancellationTokenSource();
                _delayedExecutionThread = new Thread(() => DelayedRelease(streamDeckPanel, ActionForRelease, _buttonFaceForRelease));
                _delayedExecutionThread.Start();
            }
        }

        private void DelayedRelease(StreamDeckPanel streamDeckPanel, IStreamDeckButtonAction streamDeckButtonAction, IStreamDeckButtonFace streamDeckButtonFace)
        {
            try
            {
                Thread.Sleep(ExecutionDelay);
                streamDeckButtonAction?.Execute(new CancellationToken());
                streamDeckButtonFace?.Execute(streamDeckPanel);
            }
            catch (Exception e)
            {
                Common.ShowErrorMessageBox(e);
            }
        }

        public StreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public IStreamDeckButtonFace FaceForPress
        {
            get => _buttonFaceForPress;
            set => _buttonFaceForPress = value;
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

            FaceForPress = streamDeckButton.FaceForPress;
            FaceForRelease = streamDeckButton.FaceForRelease;
        }

        public IStreamDeckButtonFace FaceForRelease
        {
            get => _buttonFaceForRelease;
            set => _buttonFaceForRelease = value;
        }

        public IStreamDeckButtonAction ActionForRelease
        {
            get => _buttonActionForRelease;
            set => _buttonActionForRelease = value;
        }

        public bool HasConfig =>
            _buttonFaceForPress != null ||
            _buttonFaceForRelease != null ||
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
                if (FaceForPress != null)
                {
                    result = FaceForPress.FaceType;
                }
                else if (FaceForRelease != null)
                {
                    result = FaceForRelease.FaceType;
                }

                return result;
            }
        }

        public static HashSet<StreamDeckButton> GetAllStreamDeckButtonNames()
        {
            var result = new HashSet<StreamDeckButton>();

            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON1));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON2));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON3));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON4));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON5));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON6));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON7));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON8));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON9));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON10));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON11));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON12));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON13));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON14));
            result.Add(new StreamDeckButton(StreamDeckButtonNames.BUTTON15));
            return result;
        }


    }


    public enum StreamDeckButtonNames
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
        BUTTON15
    }

    public static class StreamDeckFunction
    {
        public static int ButtonNumber(StreamDeckButtonNames streamDeckButtonName)
        {
            if (streamDeckButtonName == StreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return 0;
            }

            return int.Parse(streamDeckButtonName.ToString().Replace("BUTTON", ""));
        }

        public static StreamDeckButtonNames ButtonName(int streamDeckButtonNumber)
        {
            if (streamDeckButtonNumber == 0)
            {
                return StreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }

        public static StreamDeckButtonNames ButtonName(string streamDeckButtonNumber)
        {
            if (string.IsNullOrEmpty(streamDeckButtonNumber) || streamDeckButtonNumber == "0")
            {
                return StreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }
    }

}
