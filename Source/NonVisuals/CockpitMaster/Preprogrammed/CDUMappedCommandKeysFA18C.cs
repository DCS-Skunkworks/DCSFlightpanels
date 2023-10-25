using System.Collections.Generic;
using NonVisuals.CockpitMaster.Switches;

namespace NonVisuals.CockpitMaster.PreProgrammed
{
    public class CDUMappedCommandKeyFA18C : CDUMappedCommandKey
    {
        public CDUMappedCommandKeyFA18C(int group, int mask, bool isOn, CDU737Keys _CDUKey, string commandOn = "", string commandOff = "")
            : base(group, mask, isOn, _CDUKey, commandOn, commandOff)
        {
        }

        public static HashSet<CDUMappedCommandKey> GetMappedPanelKeys()
        {
            var result = new HashSet<CDUMappedCommandKey>
            {
                // Group 1
                new CDUMappedCommandKey(1, 1 << 7, false, CDU737Keys.RSK1,"UFC_OS1 1" , "UFC_OS1 0"),
                new CDUMappedCommandKey(1, 1 << 6, false, CDU737Keys.RSK2,"UFC_OS2 1" , "UFC_OS2 0"),
                new CDUMappedCommandKey(1, 1 << 5, false, CDU737Keys.RSK3,"UFC_OS3 1" , "UFC_OS3 0"),
                new CDUMappedCommandKey(1, 1 << 4, false, CDU737Keys.RSK4,"UFC_OS4 1" , "UFC_OS4 0"),
                new CDUMappedCommandKey(1, 1 << 3, false, CDU737Keys.RSK5,"UFC_OS5 1" , "UFC_OS5 0"),
                new CDUMappedCommandKey(1, 1 << 2, false, CDU737Keys.RSK6),

                // Group 2
                new CDUMappedCommandKey(2, 1 << 7, false, CDU737Keys.LSK1,"UFC_IP 1" , "UFC_IP 0"),
                new CDUMappedCommandKey(2, 1 << 6, false, CDU737Keys.LSK2),
                new CDUMappedCommandKey(2, 1 << 5, false, CDU737Keys.LSK3),
                new CDUMappedCommandKey(2, 1 << 4, false, CDU737Keys.LSK4),
                new CDUMappedCommandKey(2, 1 << 3, false, CDU737Keys.LSK5),
                new CDUMappedCommandKey(2, 1 << 2, false, CDU737Keys.LSK6),

                // Group 3
                new CDUMappedCommandKey(3, 1 << 6, false, CDU737Keys.INITREF, "UFC_AP 1" , "UFC_AP 0"),
                new CDUMappedCommandKey(3, 1 << 5, false, CDU737Keys.RTE, "UFC_IFF 1" , "UFC_IFF 0"),
                new CDUMappedCommandKey(3, 1 << 4, false, CDU737Keys.CLB, "UFC_TCN 1" , "UFC_TCN 0"),
                new CDUMappedCommandKey(3, 1 << 3, false, CDU737Keys.CRZ, "UFC_ILS 1" , "UFC_ILS 0"),
                new CDUMappedCommandKey(3, 1 << 2, false, CDU737Keys.DES,"UFC_DL 1" , "UFC_DL 0"),
                new CDUMappedCommandKey(3, 1 << 1, false, CDU737Keys.BRT_MINUS),
                new CDUMappedCommandKey(3, 1 << 0, false, CDU737Keys.BRT_PLUS),

                // Group 4
                
                new CDUMappedCommandKey(4, 1 << 5, false, CDU737Keys.MENU),
                new CDUMappedCommandKey(4, 1 << 4, false, CDU737Keys.LEGS),
                new CDUMappedCommandKey(4, 1 << 3, false, CDU737Keys.DEP_ARR),
                new CDUMappedCommandKey(4, 1 << 2, false, CDU737Keys.HOLD),
                new CDUMappedCommandKey(4, 1 << 1, false, CDU737Keys.PROG,"UFC_BCN 1" , "UFC_BCN 0"),
                new CDUMappedCommandKey(4, 1 << 0, false, CDU737Keys.EXEC , "UFC_ONOFF 1" , "UFC_ONOFF 0"),

                // Group 5
                new CDUMappedCommandKey(5, 1 << 6, false, CDU737Keys.N1LIMIT),
                new CDUMappedCommandKey(5, 1 << 5, false, CDU737Keys.FIX),
                new CDUMappedCommandKey(5, 1 << 4, false, CDU737Keys.A ),
                new CDUMappedCommandKey(5, 1 << 3, false, CDU737Keys.B),
                new CDUMappedCommandKey(5, 1 << 2, false, CDU737Keys.C),
                new CDUMappedCommandKey(5, 1 << 1, false, CDU737Keys.D),
                new CDUMappedCommandKey(5, 1 << 0, false, CDU737Keys.E),
                // Group 6
                new CDUMappedCommandKey(6, 1 << 6, false, CDU737Keys.PREV_PAGE),
                new CDUMappedCommandKey(6, 1 << 5, false, CDU737Keys.NEXT_PAGE),
                new CDUMappedCommandKey(6, 1 << 4, false, CDU737Keys.F),
                new CDUMappedCommandKey(6, 1 << 3, false, CDU737Keys.G),
                new CDUMappedCommandKey(6, 1 << 2, false, CDU737Keys.H),
                new CDUMappedCommandKey(6, 1 << 1, false, CDU737Keys.I),
                new CDUMappedCommandKey(6, 1 << 0, false, CDU737Keys.J),

                // Group 7
                new CDUMappedCommandKey(7, 1 << 7, false, CDU737Keys.K1, "UFC_1 1" , "UFC_1 0"),
                new CDUMappedCommandKey(7, 1 << 6, false, CDU737Keys.K2, "UFC_2 1" , "UFC_2 0"),
                new CDUMappedCommandKey(7, 1 << 5, false, CDU737Keys.K3, "UFC_3 1" , "UFC_3 0"),
                new CDUMappedCommandKey(7, 1 << 4, false, CDU737Keys.K),
                new CDUMappedCommandKey(7, 1 << 3, false, CDU737Keys.L),
                new CDUMappedCommandKey(7, 1 << 2, false, CDU737Keys.M),
                new CDUMappedCommandKey(7, 1 << 1, false, CDU737Keys.N),
                new CDUMappedCommandKey(7, 1 << 0, false, CDU737Keys.O),

                // Group 8
                new CDUMappedCommandKey(8, 1 << 7, false, CDU737Keys.K4, "UFC_4 1" , "UFC_4 0"),
                new CDUMappedCommandKey(8, 1 << 6, false, CDU737Keys.K5, "UFC_5 1" , "UFC_5 0"),
                new CDUMappedCommandKey(8, 1 << 5, false, CDU737Keys.K6, "UFC_6 1" , "UFC_6 0"),
                new CDUMappedCommandKey(8, 1 << 4, false, CDU737Keys.P),
                new CDUMappedCommandKey(8, 1 << 3, false, CDU737Keys.Q),
                new CDUMappedCommandKey(8, 1 << 2, false, CDU737Keys.R),
                new CDUMappedCommandKey(8, 1 << 1, false, CDU737Keys.S),
                new CDUMappedCommandKey(8, 1 << 0, false, CDU737Keys.T),

                // Group 9
                new CDUMappedCommandKey(9, 1 << 7, false, CDU737Keys.K7, "UFC_7 1" , "UFC_7 0"),
                new CDUMappedCommandKey(9, 1 << 6, false, CDU737Keys.K8, "UFC_8 1" , "UFC_8 0"),
                new CDUMappedCommandKey(9, 1 << 5, false, CDU737Keys.K9, "UFC_9 1" , "UFC_9 0"),
                new CDUMappedCommandKey(9, 1 << 4, false, CDU737Keys.U),
                new CDUMappedCommandKey(9, 1 << 3, false, CDU737Keys.V),
                new CDUMappedCommandKey(9, 1 << 2, false, CDU737Keys.W),
                new CDUMappedCommandKey(9, 1 << 1, false, CDU737Keys.X),
                new CDUMappedCommandKey(9, 1 << 0, false, CDU737Keys.Y),

                // Group 10
                new CDUMappedCommandKey(10, 1 << 7, false, CDU737Keys.KPT, "UFC_CLR 1" , "UFC_CLR 0"),
                new CDUMappedCommandKey(10, 1 << 6, false, CDU737Keys.K0, "UFC_0 1" , "UFC_0 0"),
                new CDUMappedCommandKey(10, 1 << 5, false, CDU737Keys.KPM,"UFC_ENT 1" , "UFC_ENT 0"),
                new CDUMappedCommandKey(10, 1 << 4, false, CDU737Keys.Z),
                new CDUMappedCommandKey(10, 1 << 3, false, CDU737Keys.SP),
                new CDUMappedCommandKey(10, 1 << 2, false, CDU737Keys.DEL),
                new CDUMappedCommandKey(10, 1 << 1, false, CDU737Keys.SLASH),
                new CDUMappedCommandKey(10, 1 << 0, false, CDU737Keys.CLR, "UFC_CLR 1" , "UFC_CLR 0"),
                };

            return result;
        }


    }
}
