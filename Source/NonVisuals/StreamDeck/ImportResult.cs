namespace NonVisuals.StreamDeck
{
    public class ImportResult
    {
        public int FacesImported = 0;
        public int KeyPressActions = 0;
        public int KeyReleaseActions = 0;
        public int StreamDeckButtons = 0;

        public bool ChangesWereMade
        {
            get => FacesImported != 0 && KeyReleaseActions != 0 && KeyReleaseActions != 0 && StreamDeckButtons != 0;
        }


    }
}
