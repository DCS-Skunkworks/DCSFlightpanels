using System.Collections.Generic;
using WindowsInput;

namespace ProUsbPanels
{
    public class VirtualKeyCodeHolder
    {
         
        private VirtualKeyCode _virtualKeyCode;
        private bool _isModifier;
        private bool _isExtendedKey;

        public VirtualKeyCodeHolder(VirtualKeyCode virtualKeyCode)
        {
            _virtualKeyCode = virtualKeyCode;
            UpdateModifierExtendedMembers();
        }
        
        public bool IsModifier
        {
            get { return _isModifier; }
        }

        public bool IsExtended
        {
            get { return _isExtendedKey; }
        }

        public VirtualKeyCode VirtualKeyCode
        {
            get { return _virtualKeyCode; }
            set
            {
                _virtualKeyCode = value;
                UpdateModifierExtendedMembers();
            }
        }

        private void UpdateModifierExtendedMembers()
        {
            _isModifier = IsModifierKey(_virtualKeyCode);
            _isExtendedKey = IsExtendedKey(_virtualKeyCode);
        }



        public static HashSet<VirtualKeyCodeHolder> GetPressedVirtualKeyCodeHoldersThatAreModifiers()
        {
            var virtualKeyCodeHolders = new HashSet<VirtualKeyCodeHolder>();

            if (InputSimulator.IsKeyDown(VirtualKeyCode.LSHIFT))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.LSHIFT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RSHIFT))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.RSHIFT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LCONTROL))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.LCONTROL));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RCONTROL))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.RCONTROL));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LWIN))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.LWIN));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RWIN))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.RWIN));
            }
            /*
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F1))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F1));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F2))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F2));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F3))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F3));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F4))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F4));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F5))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F5));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F6))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F6));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F7))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F7));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F8))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F8));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F9))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F9));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F10))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F10));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F11))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F11));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F12))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.F12));
            }
             */
            if (InputSimulator.IsKeyDown(VirtualKeyCode.END))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.END));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.DELETE))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.DELETE));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.INSERT))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.INSERT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.HOME))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.HOME));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LEFT))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.LEFT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RIGHT))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.RIGHT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.UP))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.UP));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.DOWN))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.DOWN));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.DIVIDE))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.DIVIDE));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.MULTIPLY))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.MULTIPLY));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.SUBTRACT))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.SUBTRACT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.ADD))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.ADD));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RETURN))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.RETURN));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMLOCK))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMLOCK));
            }
            /*
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD0))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD0));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD1))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD1));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD2))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD2));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD3))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD3));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD4))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD4));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD5))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD5));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD6))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD6));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD7))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD7));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD8))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD8));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD9))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.NUMPAD9));
            }*/
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LMENU))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.LMENU));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RMENU))
            {
                virtualKeyCodeHolders.Add(new VirtualKeyCodeHolder(VirtualKeyCode.RMENU));
            }
            return virtualKeyCodeHolders;
        }



        /*if (InputSimulator.IsKeyDown(VirtualKeyCode.LSHIFT))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.LSHIFT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RSHIFT))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.RSHIFT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LCONTROL))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.LCONTROL));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RCONTROL))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.RCONTROL));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LWIN))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.LWIN));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RWIN))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.RWIN));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F1))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F1));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F2))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F2));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F3))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F3));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F4))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F4));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F5))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F5));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F6))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F6));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F7))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F7));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F8))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F8));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F9))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F9));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F10))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F10));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F11))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F11));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.F12))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.F12));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.END))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.END));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.DELETE))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.DELETE));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.INSERT))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.INSERT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.HOME))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.HOME));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LEFT))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.LEFT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RIGHT))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.RIGHT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.UP))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.UP));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.DOWN))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.DOWN));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.DIVIDE))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.DIVIDE));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.MULTIPLY))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.MULTIPLY));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.SUBTRACT))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.SUBTRACT));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.ADD))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.ADD));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RETURN))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.RETURN));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMLOCK))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMLOCK));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD0))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD0));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD1))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD1));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD2))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD2));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD3))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD3));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD4))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD4));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD5))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD5));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD6))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD6));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD7))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD7));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD8))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD8));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.NUMPAD9))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.NUMPAD9));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.LMENU))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.LMENU));
            }
            if (InputSimulator.IsKeyDown(VirtualKeyCode.RMENU))
            {
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.RMENU));
            }*/
    }
}
