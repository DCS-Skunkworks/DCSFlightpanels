using MEF;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NonVisuals.KeyEmulation
{
    internal static class SendKeys
    {
        /*
         * SendInput är den korrekta funktionen att använda idag (13.1.2014) då keybd_event inte längre ska användas.
         * DCS dock fungerar inte med SendInput, LCONTROL,RCONTROL,LSHIFT,LALT,RSHIFT,RALT tas inte emot på korrekt sätt.
         * 
         * 
        */
        public static void Press(KeyPressLength breakLength,
            VirtualKeyCode[] virtualKeyCodes,
            KeyPressLength keyPressLength,
            CancellationToken innerCancellationToken,
            CancellationToken outerCancellationToken,
            int sleepValue)
        {
            var keyPressLengthTimeConsumed = 0;
            var breakLengthConsumed = 0;
            while (breakLengthConsumed < (int)breakLength)
            {
                Thread.Sleep(sleepValue);
                breakLengthConsumed += sleepValue;
                if (innerCancellationToken.IsCancellationRequested | outerCancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }

            var inputs = new NativeMethods.INPUT[virtualKeyCodes.Length];
            
            while (keyPressLengthTimeConsumed < (int)keyPressLength)
            {
                var modifierCount = 0;
                foreach (var virtualKeyCode in virtualKeyCodes)
                {
                    if (CommonVirtualKey.IsModifierKey(virtualKeyCode))
                    {
                        modifierCount++;
                    }
                }

                // Add modifiers
                for (var i = 0; i < virtualKeyCodes.Length; i++)
                {
                    var virtualKeyCode = virtualKeyCodes[i];
                    if (CommonVirtualKey.IsModifierKey(virtualKeyCode))
                    {
                        inputs[i].type = NativeMethods.INPUT_KEYBOARD;
                        inputs[i].InputUnion.ki.time = 0;
                        inputs[i].InputUnion.ki.dwFlags = NativeMethods.KEYEVENTF_SCANCODE;
                        if (CommonVirtualKey.IsExtendedKey(virtualKeyCode))
                        {
                            inputs[i].InputUnion.ki.dwFlags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
                        }

                        inputs[i].InputUnion.ki.wVk = 0;
                        inputs[i].InputUnion.ki.wScan = (ushort)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0);
                        inputs[i].InputUnion.ki.dwExtraInfo = NativeMethods.GetMessageExtraInfo();
                    }
                }

                // [x][x] [] []
                // 0  1  2  3
                // 1  2  3  4
                // Add normal keys
                for (var i = modifierCount; i < virtualKeyCodes.Length; i++)
                {
                    var virtualKeyCode = virtualKeyCodes[i];
                    if (!CommonVirtualKey.IsModifierKey(virtualKeyCode) && virtualKeyCode != VirtualKeyCode.VK_NULL)
                    {
                        inputs[i].type = NativeMethods.INPUT_KEYBOARD;
                        inputs[i].InputUnion.ki.time = 0;
                        inputs[i].InputUnion.ki.dwFlags = NativeMethods.KEYEVENTF_SCANCODE;

                        inputs[i].InputUnion.ki.wVk = 0;
                        inputs[i].InputUnion.ki.wScan = (ushort)NativeMethods.MapVirtualKey((uint)virtualKeyCode, 0);
                        inputs[i].InputUnion.ki.dwExtraInfo = NativeMethods.GetMessageExtraInfo();
                    }
                }

                NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));

                if (keyPressLength != KeyPressLength.Indefinite)
                {
                    Thread.Sleep(sleepValue);
                    keyPressLengthTimeConsumed += sleepValue;
                }
                else
                {
                    Thread.Sleep(20);
                }

                if (innerCancellationToken.IsCancellationRequested | outerCancellationToken.IsCancellationRequested)
                {
                    // If we are to cancel the whole operation. Release pressed keys ASAP and exit.
                    break;
                }
            }

            for (var i = 0; i < inputs.Length; i++)
            {
                inputs[i].InputUnion.ki.dwFlags |= NativeMethods.KEYEVENTF_KEYUP;
            }

            Array.Reverse(inputs);

            // Release same keys
            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

    }
}
