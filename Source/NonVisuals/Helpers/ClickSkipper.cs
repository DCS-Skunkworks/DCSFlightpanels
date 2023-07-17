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
        /// Returns true if should not skip.
        /// </summary>
        /// <returns></returns>
        public bool ClickAndCheck()
        {
            _clicksDetected++;
            if (_clicksDetected <= _clicksToSkip)
            {
                return false; // skip
            }

            _clicksDetected = 0;
            return true;
        }
        
    }
}
