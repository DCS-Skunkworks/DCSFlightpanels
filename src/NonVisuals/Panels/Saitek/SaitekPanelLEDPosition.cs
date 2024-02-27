namespace NonVisuals.Panels.Saitek {
    using ClassLibraryCommon;

    [SerializeCritical]
    public class SaitekPanelLEDPosition
    {
        private int _position;

        public SaitekPanelLEDPosition(int position)
        {
            _position = position;
        }

        public int GetPosition()
        {
            return _position;
        }

        public void SetPosition(int position)
        {
            _position = position;
        }

        public int Position
        {
            get => _position;
            set => _position = value;
        }
    }
}
