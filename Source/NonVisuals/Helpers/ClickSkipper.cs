using DCS_BIOS;

namespace NonVisuals.Helpers
{
    /// <summary>
    /// Used for especially radio dials so that when the user turns the dial the
    /// value doesn't change too fast.
    /// </summary>
    public class ClickSkipper
    {
        private readonly int _clicksToSkip;
        private int _clicksDetected;

        public ClickSkipper(int clicksToSkip)
        {
            _clicksToSkip = clicksToSkip;
        }
        
        /// <summary>
        /// Returns true if should skip.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSkip()
        {
            _clicksDetected++;
            if (_clicksDetected <= _clicksToSkip)
            {
                return true; // skip
            }

            _clicksDetected = 0;
            return false;
        }

        /// <summary>
        /// Returns true if should skip.
        /// Executes DCS-BIOS command if not skipping.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSkip(string dcsBIOSCommand)
        {
            _clicksDetected++;
            if (_clicksDetected <= _clicksToSkip)
            {
                return true; // skip
            }

            _clicksDetected = 0;
            DCSBIOS.Send(dcsBIOSCommand);
            return false;
        }
    }
}
