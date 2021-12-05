namespace NonVisuals
{
    using System.Collections.Generic;

    using DCS_BIOS;

    public static class JaceExtendedFactory
    {
        private static readonly Dictionary<int, JaceExtended> JaceEngines = new Dictionary<int, JaceExtended>();

        public static JaceExtended Instance(ref int id)
        {
            if (!JaceEngines.ContainsKey(id) || id == 0)
            {
                if (id == 0)
                {
                    id = RandomFactory.Get();
                }

                var jaceExtended = new JaceExtended();
                JaceEngines.Add(id, jaceExtended);
                return jaceExtended;
            }

            return JaceEngines[id];
        }
    }
}
