using System;
using ClassLibraryCommon;
using NLog;

namespace NonVisuals
{
    using System.Collections.Generic;

    using DCS_BIOS;

    public static class JaceExtendedFactory
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<int, JaceExtended> JaceEngines = new Dictionary<int, JaceExtended>();
        private static readonly object LockObject = new();
        public static JaceExtended Instance(ref int id)
        {
            try
            {
                lock (LockObject)
                {
                    if (!JaceEngines.ContainsKey(id) || id == 0)
                    {
                        var collision = false;
                        while (true)
                        {
                            if (id == 0 || collision == true)
                            {
                                id = RandomFactory.Get();
                            }
                            if (!JaceEngines.ContainsKey(id))
                            {
                                var jaceExtended = new JaceExtended();
                                JaceEngines.Add(id, jaceExtended);
                                return jaceExtended;
                            }

                            collision = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                Common.ShowErrorMessageBox(ex);
            }

            return JaceEngines[id];
        }
    }
}
