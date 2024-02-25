namespace NonVisuals.Panels.Saitek {
    using System;
    using ClassLibraryCommon;

    [SerializeCritical]
    public class SaitekPanelLEDPosition
    {
        private Enum _position;

        public SaitekPanelLEDPosition(Enum position)
        {
            _position = position;
        }

        public Enum GetPosition()
        {
            return _position;
        }

        public void SetPosition(Enum position)
        {
            _position = position;
        }

        public Enum Position
        {
            get => _position;
            set => _position = value;
        }
    }
}
