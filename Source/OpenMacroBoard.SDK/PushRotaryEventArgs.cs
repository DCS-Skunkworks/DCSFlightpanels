using System;

namespace OpenMacroBoard.SDK
{
    public class PushRotaryEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyEventArgs"/> class.
        /// </summary>
        /// <param name="key">The index of the key that was pressed or released.</param>
        /// <param name="isDown">A flag that determines if the key was pressed or released.</param>
        /// <param name="ccw">A flag that determines if the rotary was rotated counterclockwise.</param>
        /// <param name="cw">A flag that determines if the rotary was rotated clockwise.</param>
        public PushRotaryEventArgs(int key, bool isDown, bool ccw, bool cw)
        {
            Key = key;
            IsDown = isDown;
            Ccw = ccw;
            Cw = cw;
        }

        /// <summary>
        /// The index of the key that was pressed or released.
        /// </summary>
        public int Key { get; }

        /// <summary>
        /// A flag that determines if the key was pressed or released.
        /// </summary>
        /// 
        public bool IsDown { get; }
        
        /// <summary>
        /// A flag that determines if the rotary was rotated counterclockwise.</param>
        /// </summary>
        public bool Ccw { get; }

        /// <summary>
        /// A flag that determines if the rotary was rotated clockwise.</param>
        /// </summary>
        public bool Cw { get; }
    }
}
