using NonVisuals.CockpitMaster.Switches;
using NonVisuals.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    public class CDUMappedKeyA10C : CDUMappedKey
    {
        public CDUMappedKeyA10C(int group, int mask, bool isOn, CDU737Keys _CDUKey, string commandOn = "", string commandOff = "")
            : base(group, mask, isOn, _CDUKey, commandOn, commandOff)
        {
        }

        public static HashSet<CDUMappedKey> GetMappedPanelKeys()
        {
            var result = new HashSet<CDUMappedKey>
            {
                // Group 1
                new CDUMappedKey(1, 1 << 7, false, CDU737Keys.RSK1,"CDU_LSK_3R INC" , "CDU_LSK_3R DEC"),
                new CDUMappedKey(1, 1 << 6, false, CDU737Keys.RSK2,"CDU_LSK_5R INC" , "CDU_LSK_5R DEC"),
                new CDUMappedKey(1, 1 << 5, false, CDU737Keys.RSK3,"CDU_LSK_7R INC" , "CDU_LSK_7R DEC"),
                new CDUMappedKey(1, 1 << 4, false, CDU737Keys.RSK4,"CDU_LSK_9R INC" , "CDU_LSK_9R DEC"),
                new CDUMappedKey(1, 1 << 3, false, CDU737Keys.RSK5,"CDU_DATA 2", "CDU_DATA 1"),
                new CDUMappedKey(1, 1 << 2, false, CDU737Keys.RSK6,"CDU_DATA 0", "CDU_DATA 1"),

                // Group 2
                new CDUMappedKey(2, 1 << 7, false, CDU737Keys.LSK1,"CDU_LSK_3L INC" , "CDU_LSK_3L DEC"),
                new CDUMappedKey(2, 1 << 6, false, CDU737Keys.LSK2,"CDU_LSK_5L INC" , "CDU_LSK_5L DEC"),
                new CDUMappedKey(2, 1 << 5, false, CDU737Keys.LSK3,"CDU_LSK_7L INC" , "CDU_LSK_7L DEC"),
                new CDUMappedKey(2, 1 << 4, false, CDU737Keys.LSK4,"CDU_LSK_9L INC" , "CDU_LSK_9L DEC"),
                new CDUMappedKey(2, 1 << 3, false, CDU737Keys.LSK5,"CDU_PG 2", "CDU_PG 1"),
                new CDUMappedKey(2, 1 << 2, false, CDU737Keys.LSK6,"CDU_PG 0", "CDU_PG 1"),

                // Group 3
                new CDUMappedKey(3, 1 << 6, false, CDU737Keys.INITREF,"CDU_SYS 1", "CDU_SYS 0"),
                new CDUMappedKey(3, 1 << 5, false, CDU737Keys.RTE, "CDU_NAV 1", "CDU_NAV 0"),
                new CDUMappedKey(3, 1 << 4, false, CDU737Keys.CLB, "CDU_WP 1", "CDU_WP 0"),
                new CDUMappedKey(3, 1 << 3, false, CDU737Keys.CRZ, "CDU_OSET 1", "CDU_OSET 0"),
                new CDUMappedKey(3, 1 << 2, false, CDU737Keys.DES, "CDU_FPM 1", "CDU_FPM 0" ),
                new CDUMappedKey(3, 1 << 1, false, CDU737Keys.BRT_MINUS, "CDU_BRT 2", "CDU_BRT 1"),
                new CDUMappedKey(3, 1 << 0, false, CDU737Keys.BRT_PLUS, "CDU_BRT 0", "CDU_BRT 1"),

                // Group 4
                
                new CDUMappedKey(4, 1 << 5, false, CDU737Keys.MENU),
                new CDUMappedKey(4, 1 << 4, false, CDU737Keys.LEGS),
                new CDUMappedKey(4, 1 << 3, false, CDU737Keys.DEP_ARR),
                new CDUMappedKey(4, 1 << 2, false, CDU737Keys.HOLD),
                new CDUMappedKey(4, 1 << 1, false, CDU737Keys.PROG , "CDU_PREV 1", "CDU_PREV 0"),
                new CDUMappedKey(4, 1 << 0, false, CDU737Keys.EXEC),

                // Group 5
                new CDUMappedKey(5, 1 << 6, false, CDU737Keys.N1LIMIT),
                new CDUMappedKey(5, 1 << 5, false, CDU737Keys.FIX,"CDU_MK 1", "CDU_MK 0"),
                new CDUMappedKey(5, 1 << 4, false, CDU737Keys.A,"CDU_A INC", "CDU_A DEC"),
                new CDUMappedKey(5, 1 << 3, false, CDU737Keys.B,"CDU_B INC", "CDU_B DEC"),
                new CDUMappedKey(5, 1 << 2, false, CDU737Keys.C,"CDU_C INC", "CDU_C DEC"),
                new CDUMappedKey(5, 1 << 1, false, CDU737Keys.D,"CDU_D INC", "CDU_D DEC"),
                new CDUMappedKey(5, 1 << 0, false, CDU737Keys.E,"CDU_E INC", "CDU_E DEC"),
                // Group 6
                new CDUMappedKey(6, 1 << 6, false, CDU737Keys.PREV_PAGE,"CDU_PG 2", "CDU_PG 1"),
                new CDUMappedKey(6, 1 << 5, false, CDU737Keys.NEXT_PAGE,"CDU_PG 0", "CDU_PG 1"),
                new CDUMappedKey(6, 1 << 4, false, CDU737Keys.F,"CDU_F INC", "CDU_F DEC"),
                new CDUMappedKey(6, 1 << 3, false, CDU737Keys.G,"CDU_G INC", "CDU_G DEC"),
                new CDUMappedKey(6, 1 << 2, false, CDU737Keys.H,"CDU_H INC", "CDU_H DEC"),
                new CDUMappedKey(6, 1 << 1, false, CDU737Keys.I,"CDU_I INC", "CDU_I DEC"),
                new CDUMappedKey(6, 1 << 0, false, CDU737Keys.J,"CDU_J INC", "CDU_J DEC"),

                // Group 7
                new CDUMappedKey(7, 1 << 7, false, CDU737Keys.K1,"CDU_1 INC", "CDU_1 DEC"),
                new CDUMappedKey(7, 1 << 6, false, CDU737Keys.K2,"CDU_2 INC", "CDU_2 DEC"),
                new CDUMappedKey(7, 1 << 5, false, CDU737Keys.K3,"CDU_3 INC", "CDU_3 DEC"),
                new CDUMappedKey(7, 1 << 4, false, CDU737Keys.K,"CDU_K INC", "CDU_K DEC"),
                new CDUMappedKey(7, 1 << 3, false, CDU737Keys.L,"CDU_L INC", "CDU_L DEC"),
                new CDUMappedKey(7, 1 << 2, false, CDU737Keys.M,"CDU_M INC", "CDU_M DEC"),
                new CDUMappedKey(7, 1 << 1, false, CDU737Keys.N,"CDU_N INC", "CDU_N DEC"),
                new CDUMappedKey(7, 1 << 0, false, CDU737Keys.O,"CDU_O INC", "CDU_O DEC"),

                // Group 8
                new CDUMappedKey(8, 1 << 7, false, CDU737Keys.K4,"CDU_4 INC", "CDU_4 DEC"),
                new CDUMappedKey(8, 1 << 6, false, CDU737Keys.K5,"CDU_5 INC", "CDU_5 DEC"),
                new CDUMappedKey(8, 1 << 5, false, CDU737Keys.K6,"CDU_6 INC", "CDU_6 DEC"),
                new CDUMappedKey(8, 1 << 4, false, CDU737Keys.P,"CDU_P INC", "CDU_P DEC"),
                new CDUMappedKey(8, 1 << 3, false, CDU737Keys.Q,"CDU_Q INC", "CDU_Q DEC"),
                new CDUMappedKey(8, 1 << 2, false, CDU737Keys.R,"CDU_R INC", "CDU_R DEC"),
                new CDUMappedKey(8, 1 << 1, false, CDU737Keys.S,"CDU_S INC", "CDU_S DEC"),
                new CDUMappedKey(8, 1 << 0, false, CDU737Keys.T,"CDU_T INC", "CDU_T DEC"),

                // Group 9
                new CDUMappedKey(9, 1 << 7, false, CDU737Keys.K7,"CDU_7 INC", "CDU_7 DEC"),
                new CDUMappedKey(9, 1 << 6, false, CDU737Keys.K8,"CDU_8 INC", "CDU_8 DEC"),
                new CDUMappedKey(9, 1 << 5, false, CDU737Keys.K9,"CDU_9 INC", "CDU_9 DEC"),
                new CDUMappedKey(9, 1 << 4, false, CDU737Keys.U,"CDU_U INC", "CDU_U DEC"),
                new CDUMappedKey(9, 1 << 3, false, CDU737Keys.V,"CDU_V INC", "CDU_V DEC"),
                new CDUMappedKey(9, 1 << 2, false, CDU737Keys.W,"CDU_W INC", "CDU_W DEC"),
                new CDUMappedKey(9, 1 << 1, false, CDU737Keys.X,"CDU_X INC", "CDU_X DEC"),
                new CDUMappedKey(9, 1 << 0, false, CDU737Keys.Y,"CDU_Y INC", "CDU_Y DEC"),

                // Group 10
                new CDUMappedKey(10, 1 << 7, false, CDU737Keys.KPT,"CDU_POINT INC", "CDU_POINT DEC"),
                new CDUMappedKey(10, 1 << 6, false, CDU737Keys.K0,"CDU_0 INC", "CDU_0 DEC"),
                new CDUMappedKey(10, 1 << 5, false, CDU737Keys.KPM),
                new CDUMappedKey(10, 1 << 4, false, CDU737Keys.Z,"CDU_Z INC", "CDU_Z DEC"),
                new CDUMappedKey(10, 1 << 3, false, CDU737Keys.SP,"CDU_SPC INC", "CDU_SPC DEC"),
                new CDUMappedKey(10, 1 << 2, false, CDU737Keys.DEL,"CDU_BCK INC", "CDU_BCK DEC"),
                new CDUMappedKey(10, 1 << 1, false, CDU737Keys.SLASH,"CDU_SLASH INC", "CDU_SLASH DEC"),
                new CDUMappedKey(10, 1 << 0, false, CDU737Keys.CLR,"CDU_CLR INC", "CDU_CLR DEC"),
            };

            return result;
        }


    }
}
