using System.Threading.Tasks;
using DCS_BIOS;

namespace NonVisuals.Helpers
{
    /// <summary>
    /// Used for especially radio dials so that when the user turns the dial the
    /// value doesn't change too fast.
    /// </summary>
    public class ClickSkipper
    {
        public int ClicksToSkip { get; set; }
        private int _clicksDetected;

        public ClickSkipper(int clicksToSkip)
        {
            ClicksToSkip = clicksToSkip;
        }

        
        /// <summary>
        /// Returns true if should skip.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSkip()
        {
            _clicksDetected++;
            if (_clicksDetected <= ClicksToSkip)
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
        public async Task<bool> ClickAsync(string dcsBIOSCommand)
        {
            _clicksDetected++;
            if (_clicksDetected <= ClicksToSkip)
            {
                return true; // skip
            }

            _clicksDetected = 0;

            if (!string.IsNullOrEmpty(dcsBIOSCommand))
            {
                await DCSBIOS.SendAsync(dcsBIOSCommand);
            }

            return false;
        }
    }
}
