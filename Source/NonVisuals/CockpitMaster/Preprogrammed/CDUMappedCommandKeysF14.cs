using NonVisuals.CockpitMaster.Switches;
using NonVisuals.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    public class CDUMappedCommandKeyF14 : CDUMappedCommandKey
    {
        public CDUMappedCommandKeyF14(int group, int mask, bool isOn, CDU737Keys _CDUKey, string commandOn = "", string commandOff = "")
            : base(group, mask, isOn, _CDUKey, commandOn, commandOff)
        {
        }

        public static HashSet<CDUMappedCommandKey> GetMappedPanelKeys()
        {
            var result = new HashSet<CDUMappedCommandKey>
            {
                // Group 1
                new CDUMappedCommandKey(1, 1 << 7, false, CDU737Keys.RSK1, "RIO_CAP_BTN_6 INC","RIO_CAP_BTN_6 DEC"),
                new CDUMappedCommandKey(1, 1 << 6, false, CDU737Keys.RSK2, "RIO_CAP_BTN_7 INC","RIO_CAP_BTN_7 DEC"),
                new CDUMappedCommandKey(1, 1 << 5, false, CDU737Keys.RSK3, "RIO_CAP_BTN_8 INC","RIO_CAP_BTN_8 DEC"),
                new CDUMappedCommandKey(1, 1 << 4, false, CDU737Keys.RSK4, "RIO_CAP_BTN_9 INC","RIO_CAP_BTN_9 DEC"),
                new CDUMappedCommandKey(1, 1 << 3, false, CDU737Keys.RSK5, "RIO_CAP_BTN_10 INC","RIO_CAP_BTN_10 DEC"),
                new CDUMappedCommandKey(1, 1 << 2, false, CDU737Keys.RSK6),

                // Group 2
                new CDUMappedCommandKey(2, 1 << 7, false, CDU737Keys.LSK1, "RIO_CAP_BTN_1 INC","RIO_CAP_BTN_1 DEC"),
                new CDUMappedCommandKey(2, 1 << 6, false, CDU737Keys.LSK2, "RIO_CAP_BTN_2 INC","RIO_CAP_BTN_2 DEC"),
                new CDUMappedCommandKey(2, 1 << 5, false, CDU737Keys.LSK3, "RIO_CAP_BTN_3 INC","RIO_CAP_BTN_3 DEC"),
                new CDUMappedCommandKey(2, 1 << 4, false, CDU737Keys.LSK4, "RIO_CAP_BTN_4 INC","RIO_CAP_BTN_4 DEC"),
                new CDUMappedCommandKey(2, 1 << 3, false, CDU737Keys.LSK5, "RIO_CAP_BTN_5 INC","RIO_CAP_BTN_5 DEC"),
                new CDUMappedCommandKey(2, 1 << 2, false, CDU737Keys.LSK6),

                // Group 3
                new CDUMappedCommandKey(3, 1 << 6, false, CDU737Keys.INITREF, "RIO_HCU_TCS TOGGLE",""),
                new CDUMappedCommandKey(3, 1 << 5, false, CDU737Keys.RTE,"RIO_HCU_RADAR TOGGLE", ""),
                new CDUMappedCommandKey(3, 1 << 4, false, CDU737Keys.CLB,"RIO_HCU_DDD TOGGLE", ""),
                new CDUMappedCommandKey(3, 1 << 3, false, CDU737Keys.CRZ,"RIO_HCU_TID TOGGLE", ""),
                new CDUMappedCommandKey(3, 1 << 2, false, CDU737Keys.DES),
                new CDUMappedCommandKey(3, 1 << 1, false, CDU737Keys.BRT_MINUS),
                new CDUMappedCommandKey(3, 1 << 0, false, CDU737Keys.BRT_PLUS),

                // Group 4
                
                new CDUMappedCommandKey(4, 1 << 5, false, CDU737Keys.MENU),
                new CDUMappedCommandKey(4, 1 << 4, false, CDU737Keys.LEGS),
                new CDUMappedCommandKey(4, 1 << 3, false, CDU737Keys.DEP_ARR),
                new CDUMappedCommandKey(4, 1 << 2, false, CDU737Keys.HOLD),
                new CDUMappedCommandKey(4, 1 << 1, false, CDU737Keys.PROG ),
                new CDUMappedCommandKey(4, 1 << 0, false, CDU737Keys.EXEC, "RIO_CAP_ENTER INC","RIO_CAP_ENTER DEC"),

                // Group 5
                new CDUMappedCommandKey(5, 1 << 6, false, CDU737Keys.N1LIMIT, "RIO_CAP_BTN_TNG INC","RIO_CAP_BTN_TNG DEC"),
                new CDUMappedCommandKey(5, 1 << 5, false, CDU737Keys.FIX, "RIO_CAP_BTN_PGM_RESTRT INC","RIO_CAP_BTN_PGM_RESTRT DEC"),
                new CDUMappedCommandKey(5, 1 << 4, false, CDU737Keys.A),
                new CDUMappedCommandKey(5, 1 << 3, false, CDU737Keys.B),
                new CDUMappedCommandKey(5, 1 << 2, false, CDU737Keys.C),
                new CDUMappedCommandKey(5, 1 << 1, false, CDU737Keys.D),
                new CDUMappedCommandKey(5, 1 << 0, false, CDU737Keys.E ,"RIO_CAP_NE INC","RIO_CAP_NE DEC" ),
                // Group 6
                new CDUMappedCommandKey(6, 1 << 6, false, CDU737Keys.PREV_PAGE, "RIO_CAP_CATRGORY DEC" ),
                new CDUMappedCommandKey(6, 1 << 5, false, CDU737Keys.NEXT_PAGE , "RIO_CAP_CATRGORY INC"  ),
                new CDUMappedCommandKey(6, 1 << 4, false, CDU737Keys.F),
                new CDUMappedCommandKey(6, 1 << 3, false, CDU737Keys.G),
                new CDUMappedCommandKey(6, 1 << 2, false, CDU737Keys.H),
                new CDUMappedCommandKey(6, 1 << 1, false, CDU737Keys.I),
                new CDUMappedCommandKey(6, 1 << 0, false, CDU737Keys.J),

                // Group 7
                new CDUMappedCommandKey(7, 1 << 7, false, CDU737Keys.K1, "RIO_CAP_LAT_1 INC","RIO_CAP_LAT_1 DEC"),
                new CDUMappedCommandKey(7, 1 << 6, false, CDU737Keys.K2, "RIO_CAP_NBR_2 INC","RIO_CAP_NBR_2 DEC"),
                new CDUMappedCommandKey(7, 1 << 5, false, CDU737Keys.K3, "RIO_CAP_SPD_3 INC","RIO_CAP_SPD_3 DEC"),
                new CDUMappedCommandKey(7, 1 << 4, false, CDU737Keys.K),
                new CDUMappedCommandKey(7, 1 << 3, false, CDU737Keys.L),
                new CDUMappedCommandKey(7, 1 << 2, false, CDU737Keys.M),
                new CDUMappedCommandKey(7, 1 << 1, false, CDU737Keys.N ,"RIO_CAP_NE INC","RIO_CAP_NE DEC" ),
                new CDUMappedCommandKey(7, 1 << 0, false, CDU737Keys.O),

                // Group 8
                new CDUMappedCommandKey(8, 1 << 7, false, CDU737Keys.K4, "RIO_CAP_ALT_4 INC","RIO_CAP_ALT_4 DEC"),
                new CDUMappedCommandKey(8, 1 << 6, false, CDU737Keys.K5, "RIO_CAP_RNG_5 INC","RIO_CAP_RNG_5 DEC"),
                new CDUMappedCommandKey(8, 1 << 5, false, CDU737Keys.K6, "RIO_CAP_LONG_6 INC","RIO_CAP_LONG_6 DEC"),
                new CDUMappedCommandKey(8, 1 << 4, false, CDU737Keys.P),
                new CDUMappedCommandKey(8, 1 << 3, false, CDU737Keys.Q),
                new CDUMappedCommandKey(8, 1 << 2, false, CDU737Keys.R),
                new CDUMappedCommandKey(8, 1 << 1, false, CDU737Keys.S,"RIO_CAP_SW INC","RIO_CAP_SW DEC" ),
                new CDUMappedCommandKey(8, 1 << 0, false, CDU737Keys.T),

                // Group 9
                new CDUMappedCommandKey(9, 1 << 7, false, CDU737Keys.K7, "RIO_CAP_7 INC","RIO_CAP_7 DEC"),
                new CDUMappedCommandKey(9, 1 << 6, false, CDU737Keys.K8, "RIO_CAP_HDG_8 INC","RIO_CAP_HDG_8 DEC"),
                new CDUMappedCommandKey(9, 1 << 5, false, CDU737Keys.K9, "RIO_CAP_9 INC","RIO_CAP_9 DEC"),
                new CDUMappedCommandKey(9, 1 << 4, false, CDU737Keys.U),
                new CDUMappedCommandKey(9, 1 << 3, false, CDU737Keys.V),
                new CDUMappedCommandKey(9, 1 << 2, false, CDU737Keys.W ,"RIO_CAP_SW INC","RIO_CAP_SW DEC" ),
                new CDUMappedCommandKey(9, 1 << 1, false, CDU737Keys.X),
                new CDUMappedCommandKey(9, 1 << 0, false, CDU737Keys.Y),

                // Group 10
                new CDUMappedCommandKey(10, 1 << 7, false, CDU737Keys.KPT),
                new CDUMappedCommandKey(10, 1 << 6, false, CDU737Keys.K0, "RIO_CAP_BRG_0 INC","RIO_CAP_BRG_0 DEC"),
                new CDUMappedCommandKey(10, 1 << 5, false, CDU737Keys.KPM),
                new CDUMappedCommandKey(10, 1 << 4, false, CDU737Keys.Z),
                new CDUMappedCommandKey(10, 1 << 3, false, CDU737Keys.SP),
                new CDUMappedCommandKey(10, 1 << 2, false, CDU737Keys.DEL),
                new CDUMappedCommandKey(10, 1 << 1, false, CDU737Keys.SLASH),
                new CDUMappedCommandKey(10, 1 << 0, false, CDU737Keys.CLR, "RIO_CAP_CLEAR INC","RIO_CAP_CLEAR DEC"),
            };

            return result;
        }


    }
}
