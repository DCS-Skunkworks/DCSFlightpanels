namespace NonVisuals.Radios.SRS
{
    public static class SRSRadioFactory
    {
        private static SRSRadio _srsRadio;
        private static string _srsSendToIPUdp = "127.0.0.1";
        private static int _srsReceivePortUdp = 7082;
        private static int _srsSendPortUdp = 9040;

        public static void SetParams(int portFrom, string ipAddressTo, int portTo)
        {
            _srsSendToIPUdp = ipAddressTo;
            _srsReceivePortUdp = portFrom;
            _srsSendPortUdp = portTo;
        }

        public static void Shutdown()
        {
            _srsRadio?.Shutdown();
            _srsRadio = null;
        }

        public static void ReStart()
        {
            Shutdown();
            if (_srsRadio == null)
            {
                _srsRadio = new SRSRadio(_srsReceivePortUdp, _srsSendToIPUdp, _srsSendPortUdp);
            }
        }

        public static SRSRadio GetSRSRadio()
        {
            if (_srsRadio == null)
            {
                _srsRadio = new SRSRadio(_srsReceivePortUdp, _srsSendToIPUdp, _srsSendPortUdp);
            }

            return _srsRadio;
        }
    }
}
