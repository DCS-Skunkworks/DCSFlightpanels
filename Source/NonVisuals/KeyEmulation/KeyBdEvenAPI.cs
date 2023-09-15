using MEF;
using System.Threading;

namespace NonVisuals.KeyEmulation
{
    internal static class KeyBdEvenAPI
    {
        internal static void Press(KeyPressLength breakLength,
            VirtualKeyCode[] virtualKeyCodes,
            KeyPressLength keyPressLength,
            CancellationToken innerCancellationToken,
            CancellationToken outerCancellationToken,
            int sleepValue)
        {
            var keyPressLengthTimeConsumed = 0;
            var breakLengthConsumed = 0;

            /*
                //keybd_event
                http://msdn.microsoft.com/en-us/library/windows/desktop/ms646304%28v=vs.85%29.aspx
            */
            while (breakLengthConsumed < (int)breakLength)
            {
                Thread.Sleep(sleepValue);
                breakLengthConsumed += sleepValue;
                if (innerCancellationToken.IsCancellationRequested | outerCancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }

            while (keyPressLengthTimeConsumed < (int)keyPressLength)
            {
                // Debug.WriteLine("VK = " + virtualKeyCodes[1] + " length = " + keyPressLength);
                // Press modifiers
                for (var i = 0; i < virtualKeyCodes.Length; i++)
                {
                    var virtualKeyCode = virtualKeyCodes[i];
                    if (CommonVirtualKey.IsModifierKey(virtualKeyCode))
                    {
                        if (CommonVirtualKey.IsExtendedKey(virtualKeyCode))
                        {
                            NativeMethods.keybd_event((byte)virtualKeyCode,
                                (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0),
                                (int)NativeMethods.KEYEVENTF_EXTENDEDKEY | 0, 0);
                            // keybd_event(VK_LCONTROL, 0, KEYEVENTF_EXTENDEDKEY, 0);
                        }
                        else
                        {
                            NativeMethods.keybd_event((byte)virtualKeyCode,
                                (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), 0, 0);
                        }
                    }
                }

                // Delay between modifiers and normal keys
                // Added 2021-11-28 to fix problem with combination of certain keypresses like lShift + G in IL-2
                Thread.Sleep(millisecondsTimeout: 32);

                // Press normal keys
                foreach (var virtualKeyCode in virtualKeyCodes)
                {
                    if (!CommonVirtualKey.IsModifierKey(virtualKeyCode) && virtualKeyCode != VirtualKeyCode.VK_NULL)
                    {
                        NativeMethods.keybd_event((byte)virtualKeyCode,
                            (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0), 0, 0);
                    }
                }

                if (keyPressLength != KeyPressLength.Indefinite)
                {
                    Thread.Sleep(sleepValue);
                    keyPressLengthTimeConsumed += sleepValue;
                }
                else
                {
                    Thread.Sleep(20);
                }

                if (keyPressLength != KeyPressLength.Indefinite)
                {
                    ReleaseKeys(virtualKeyCodes);
                }

                if (innerCancellationToken.IsCancellationRequested | outerCancellationToken.IsCancellationRequested)
                {
                    // If we are to cancel the whole operation. Release pressed keys ASAP and exit.
                    break;
                }
            }

            if (keyPressLength == KeyPressLength.Indefinite)
            {
                ReleaseKeys(virtualKeyCodes);
            }
        }

        private static void ReleaseKeys(VirtualKeyCode[] virtualKeyCodes)
        {
            // Release normal keys
            for (var i = 0; i < virtualKeyCodes.Length; i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (!CommonVirtualKey.IsModifierKey(virtualKeyCode))
                {
                    NativeMethods.keybd_event((byte)virtualKeyCode,
                        (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0),
                        (int)NativeMethods.KEYEVENTF_KEYUP, 0);
                }
            }

            // Release modifiers
            for (var i = 0; i < virtualKeyCodes.Length; i++)
            {
                var virtualKeyCode = virtualKeyCodes[i];
                if (CommonVirtualKey.IsModifierKey(virtualKeyCode))
                {
                    if (CommonVirtualKey.IsExtendedKey(virtualKeyCode))
                    {
                        NativeMethods.keybd_event((byte)virtualKeyCode,
                            (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0),
                            (int)(NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP), 0);
                    }
                    else
                    {
                        NativeMethods.keybd_event((byte)virtualKeyCode,
                            (byte)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0),
                            (int)NativeMethods.KEYEVENTF_KEYUP, 0);
                    }
                }
            }
        }
    }
}
