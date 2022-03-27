namespace NonVisuals
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using MEF;

    public static class CommonVirtualKey
    {
        private static readonly HashSet<VirtualKeyCode> _modifiers = new()
        {
            VirtualKeyCode.LSHIFT,
            VirtualKeyCode.RSHIFT,
            VirtualKeyCode.LCONTROL,
            VirtualKeyCode.RCONTROL,
            VirtualKeyCode.LWIN,
            VirtualKeyCode.RWIN,
            VirtualKeyCode.END,
            VirtualKeyCode.DELETE,
            VirtualKeyCode.INSERT,
            VirtualKeyCode.HOME,
            VirtualKeyCode.LEFT,
            VirtualKeyCode.RIGHT,
            VirtualKeyCode.UP,
            VirtualKeyCode.DOWN,
            VirtualKeyCode.DIVIDE,
            VirtualKeyCode.MULTIPLY,
            VirtualKeyCode.SUBTRACT,
            VirtualKeyCode.ADD,
            VirtualKeyCode.RETURN,
            VirtualKeyCode.NUMLOCK,
            VirtualKeyCode.LMENU,
            VirtualKeyCode.RMENU,
        };

        private static readonly HashSet<VirtualKeyCode> _extended = new()
        {
            VirtualKeyCode.RCONTROL,
            VirtualKeyCode.END,
            VirtualKeyCode.DELETE,
            VirtualKeyCode.INSERT,
            VirtualKeyCode.HOME,
            VirtualKeyCode.LEFT,
            VirtualKeyCode.RIGHT,
            VirtualKeyCode.UP,
            VirtualKeyCode.DOWN,
            VirtualKeyCode.DIVIDE,
            VirtualKeyCode.MULTIPLY,
            VirtualKeyCode.RETURN,
            VirtualKeyCode.NUMLOCK,
            VirtualKeyCode.RMENU,
        };

        public static bool IsModifierKey(VirtualKeyCode virtualKeyCode)
        {
            return _modifiers.Contains(virtualKeyCode);
        }

        public static bool IsExtendedKey(VirtualKeyCode virtualKeyCode)
        {
            return _extended.Contains(virtualKeyCode);

            /*Extended-Key Flag
                The extended-key flag indicates whether the keystroke message originated from one of the additional keys on the 
             * enhanced keyboard. 
             * The extended keys consist of the 
             * ALT and CTRL keys on the RIGHT HAND side of the keyboard; 
             * The INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; 
             * the NUM LOCK key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in the numeric
             * keypad. The extended-key flag is set if the key is an extended key.*/

            // All modifiers except LSHIFT / RSHIFT are extended keys.                    
            /* EXTENDED KEYS :
                        RIGHT ALT
                        RIGHT CTRL
                        INS
                        DEL
                        HOME
                        END
                        PAGE UP
                        PAGE DOWN
                        ARROW KEYS
                        NUM LOCK
                        BREAK (CTRL+PAUSE)
                        PRINT SCRN
                        DIVIDE
                        MULTIPLY
                        ENTER
                     */
        }

        public static HashSet<VirtualKeyCode> GetPressedVirtualKeyCodesThatAreModifiers()
        {
            var virtualKeyCodeHolders = new HashSet<VirtualKeyCode>();

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LSHIFT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LSHIFT);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RSHIFT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RSHIFT);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LCONTROL)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LCONTROL);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RCONTROL)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RCONTROL);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LWIN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LWIN);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RWIN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RWIN);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.END)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.END);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.DELETE)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DELETE);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.INSERT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.INSERT);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.HOME)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.HOME);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LEFT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LEFT);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RIGHT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RIGHT);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.UP)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.UP);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.DOWN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DOWN);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.DIVIDE)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.DIVIDE);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.MULTIPLY)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.MULTIPLY);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.SUBTRACT)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.SUBTRACT);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.ADD)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.ADD);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RETURN)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RETURN);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.NUMLOCK)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.NUMLOCK);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.LMENU)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.LMENU);
            }

            if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)VirtualKeyCode.RMENU)))
            {
                virtualKeyCodeHolders.Add(VirtualKeyCode.RMENU);
            }

            return virtualKeyCodeHolders;
        }
    }
}
