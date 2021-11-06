namespace NonVisuals
{
    using System.Collections.Generic;

    using DCS_BIOS;

    public static class JaceExtendedFactory
    {
        private static Dictionary<int, JaceExtended> _jaceEngines = new Dictionary<int, JaceExtended>();

        public static JaceExtended Instance(ref int id)
        {
            if (!_jaceEngines.ContainsKey(id) || id == 0)
            {
                if (id == 0)
                {
                    id = RandomFactory.Get();
                }

                var jaceExtended = new JaceExtended();
                _jaceEngines.Add(id, jaceExtended);
                return jaceExtended;
            }

            return _jaceEngines[id];
        }
    }
}
