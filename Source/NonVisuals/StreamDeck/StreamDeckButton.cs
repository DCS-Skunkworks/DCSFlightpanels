using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibraryCommon;
using Newtonsoft.Json;
using NonVisuals.Interfaces;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{



    public class StreamDeckButton
    {
        private EnumStreamDeckButtonNames _enumStreamDeckButtonName;
        private IStreamDeckButtonFace _buttonFace = null;
        private IStreamDeckButtonAction _buttonActionForPress = null;
        private IStreamDeckButtonAction _buttonActionForRelease = null;
        public int ExecutionDelay { get; set; } = 1000;

        [JsonIgnore]
        private Thread _delayedExecutionThread;
        [JsonIgnore]
        private CancellationTokenSource _cancellationTokenSource;

        public StreamDeckButton(EnumStreamDeckButtonNames enumStreamDeckButton)
        {
            _enumStreamDeckButtonName = enumStreamDeckButton;
        }

        ~StreamDeckButton()
        {
            _delayedExecutionThread?.Abort();
        }

        public void Press(StreamDeckRequisites streamDeckRequisites)
        {
            ActionForPress?.Execute(streamDeckRequisites);

            if (_buttonActionForRelease != null)
            {
                _cancellationTokenSource?.Cancel();

                _cancellationTokenSource = new CancellationTokenSource();
                _delayedExecutionThread = new Thread(() => DelayedRelease(streamDeckRequisites, ActionForRelease));
                _delayedExecutionThread.Start();
            }
        }

        public void Show(StreamDeckRequisites streamDeckRequisites)
        {
            if (Face != null)
            {
                Face?.Show(streamDeckRequisites);
            }
        }

        private void DelayedRelease(StreamDeckRequisites streamDeckRequisites, IStreamDeckButtonAction streamDeckButtonAction)
        {
            try
            {
                Thread.Sleep(ExecutionDelay);
                streamDeckButtonAction?.Execute(streamDeckRequisites);
            }
            catch (Exception e)
            {
                Common.ShowErrorMessageBox(e);
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
            return result;
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
        BUTTON15
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
