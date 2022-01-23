namespace NonVisuals
{
    using System.Collections.Generic;
    using System.Windows.Input;

    using MEF;

    public static class CommonVirtualKey
    {

        private static readonly HashSet<VirtualKeyCode> Modifiers = new HashSet<VirtualKeyCode>();

        public static bool IsModifierKeyDown()
        {
            foreach (var modifier in Modifiers)
            {
                if (Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)modifier)))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsModifierKey(VirtualKeyCode virtualKeyCode)
        {
            if (Modifiers.Count == 0)
            {
                PopulateModifiersHashSet();
            }

            foreach (var modifier in Modifiers)
            {
                if (virtualKeyCode == modifier)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsExtendedKey(VirtualKeyCode virtualKeyCode)
        {
            if (Modifiers.Count == 0)
            {
                PopulateModifiersHashSet();
            }

            /*Extended-Key Flag
                The extended-key flag indicates whether the keystroke message originated from one of the additional keys on the 
             * enhanced keyboard. 
             * The extended keys consist of the 
             * ALT and CTRL keys on the RIGHT HAND side of the keyboard; 
             * The INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; 
             * the NUM LOCK key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in the numeric
             * keypad. The extended-key flag is set if the key is an extended key.*/
            if (IsModifierKey(virtualKeyCode) && (virtualKeyCode == VirtualKeyCode.RMENU || virtualKeyCode == VirtualKeyCode.RCONTROL || virtualKeyCode == VirtualKeyCode.INSERT
                                                  || virtualKeyCode == VirtualKeyCode.DELETE || virtualKeyCode == VirtualKeyCode.HOME || virtualKeyCode == VirtualKeyCode.END
                                                  || virtualKeyCode == VirtualKeyCode.PRIOR || virtualKeyCode == VirtualKeyCode.NEXT || virtualKeyCode == VirtualKeyCode.LEFT
                                                  || virtualKeyCode == VirtualKeyCode.UP || virtualKeyCode == VirtualKeyCode.RIGHT || virtualKeyCode == VirtualKeyCode.DOWN
                                                  || virtualKeyCode == VirtualKeyCode.NUMLOCK || virtualKeyCode == VirtualKeyCode.PRINT || virtualKeyCode == VirtualKeyCode.DIVIDE
                                                  || virtualKeyCode == VirtualKeyCode.MULTIPLY || virtualKeyCode == VirtualKeyCode.RETURN))
            {
                return true;
            }

            return false;

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

        public static HashSet<VirtualKeyCode> ModifierList()
        {
            if (Modifiers.Count == 0)
            {
                PopulateModifiersHashSet();
            }

            return Modifiers;
        }

        private static void PopulateModifiersHashSet()
        {
            Modifiers.Add(VirtualKeyCode.LSHIFT);
            Modifiers.Add(VirtualKeyCode.RSHIFT);
            Modifiers.Add(VirtualKeyCode.LCONTROL);
            Modifiers.Add(VirtualKeyCode.RCONTROL);
            Modifiers.Add(VirtualKeyCode.LWIN);
            Modifiers.Add(VirtualKeyCode.RWIN);
            Modifiers.Add(VirtualKeyCode.END);
            Modifiers.Add(VirtualKeyCode.DELETE);
            Modifiers.Add(VirtualKeyCode.INSERT);
            Modifiers.Add(VirtualKeyCode.HOME);
            Modifiers.Add(VirtualKeyCode.LEFT);
            Modifiers.Add(VirtualKeyCode.RIGHT);
            Modifiers.Add(VirtualKeyCode.UP);
            Modifiers.Add(VirtualKeyCode.DOWN);
            Modifiers.Add(VirtualKeyCode.DIVIDE);
            Modifiers.Add(VirtualKeyCode.MULTIPLY);
            Modifiers.Add(VirtualKeyCode.SUBTRACT);
            Modifiers.Add(VirtualKeyCode.ADD);
            Modifiers.Add(VirtualKeyCode.RETURN);
            Modifiers.Add(VirtualKeyCode.NUMLOCK);
            Modifiers.Add(VirtualKeyCode.LMENU);
            Modifiers.Add(VirtualKeyCode.RMENU);
        }

    }
}
