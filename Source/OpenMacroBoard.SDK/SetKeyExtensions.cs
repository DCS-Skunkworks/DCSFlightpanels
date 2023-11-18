using System;

namespace OpenMacroBoard.SDK
{
    /// <summary>
    /// A bunch of extensions to clear all keys, or set a single <see cref="KeyBitmap"/> to all keys.
    /// </summary>
    public static class SetKeyExtensions
    {
        /// <summary>
        /// Sets a background image for all keys
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided board is null.</exception>
        public static void SetKeyBitmap(this IMacroBoard board, KeyBitmap bitmap)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            for (var i = 0; i < board.Keys.Count; i++)
            {
                board.SetKeyBitmap(i, bitmap);
            }
        }

        /// <summary>
        /// Sets background to black for a given key
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided board is null.</exception>
        public static void ClearKey(this IMacroBoard board, int keyId)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            board.SetKeyBitmap(keyId, KeyBitmap.Black);
        }

        /// <summary>
        /// Sets background to black for all given keys
        /// </summary>
        public static void ClearKeys(this IMacroBoard board)
        {
            board.SetKeyBitmap(KeyBitmap.Black);
        }
    }
}
