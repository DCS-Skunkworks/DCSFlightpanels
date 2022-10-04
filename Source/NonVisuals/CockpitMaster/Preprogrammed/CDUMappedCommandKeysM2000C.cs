using NonVisuals.CockpitMaster.Switches;
using NonVisuals.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    public class CDUMappedCommandKeyM2000C : CDUMappedCommandKey
    {
        public CDUMappedCommandKeyM2000C(int group, int mask, bool isOn, CDU737Keys _CDUKey, string commandOn = "", string commandOff = "")
            : base(group, mask, isOn, _CDUKey, commandOn, commandOff)
        {
        }

        public static HashSet<CDUMappedCommandKey> GetMappedPanelKeys()
        {
            var result = new HashSet<CDUMappedCommandKey>
            {
                // Group 1
                new CDUMappedCommandKey(1, 1 << 7, false, CDU737Keys.RSK1),
                new CDUMappedCommandKey(1, 1 << 6, false, CDU737Keys.RSK2),
                new CDUMappedCommandKey(1, 1 << 5, false, CDU737Keys.RSK3),
                new CDUMappedCommandKey(1, 1 << 4, false, CDU737Keys.RSK4),
                new CDUMappedCommandKey(1, 1 << 3, false, CDU737Keys.RSK5),
                new CDUMappedCommandKey(1, 1 << 2, false, CDU737Keys.RSK6),

                // Group 2
                new CDUMappedCommandKey(2, 1 << 7, false, CDU737Keys.LSK1),
                new CDUMappedCommandKey(2, 1 << 6, false, CDU737Keys.LSK2),
                new CDUMappedCommandKey(2, 1 << 5, false, CDU737Keys.LSK3),
                new CDUMappedCommandKey(2, 1 << 4, false, CDU737Keys.LSK4),
                new CDUMappedCommandKey(2, 1 << 3, false, CDU737Keys.LSK5),
                new CDUMappedCommandKey(2, 1 << 2, false, CDU737Keys.LSK6),

                // Group 3
                new CDUMappedCommandKey(3, 1 << 6, false, CDU737Keys.INITREF, "INS_PREP_SW 1", "INS_PREP_SW 0"),
                new CDUMappedCommandKey(3, 1 << 5, false, CDU737Keys.RTE, "INS_DEST_SW 1", "INS_DEST_SW 0"),
                new CDUMappedCommandKey(3, 1 << 4, false, CDU737Keys.CLB),
                new CDUMappedCommandKey(3, 1 << 3, false, CDU737Keys.CRZ),
                new CDUMappedCommandKey(3, 1 << 2, false, CDU737Keys.DES),
                new CDUMappedCommandKey(3, 1 << 1, false, CDU737Keys.BRT_MINUS),
                new CDUMappedCommandKey(3, 1 << 0, false, CDU737Keys.BRT_PLUS),

                // Group 4
                
                new CDUMappedCommandKey(4, 1 << 5, false, CDU737Keys.MENU),
                new CDUMappedCommandKey(4, 1 << 4, false, CDU737Keys.LEGS,"OFFSET_WP_TGT 1", "OFFSET_WP_TGT 0"),
                new CDUMappedCommandKey(4, 1 << 3, false, CDU737Keys.DEP_ARR),
                new CDUMappedCommandKey(4, 1 << 2, false, CDU737Keys.HOLD),
                new CDUMappedCommandKey(4, 1 << 1, false, CDU737Keys.PROG ),
                new CDUMappedCommandKey(4, 1 << 0, false, CDU737Keys.EXEC, "VAL_DATA_ENTRY 1", "VAL_DATA_ENTRY 0"),

                // Group 5
                new CDUMappedCommandKey(5, 1 << 6, false, CDU737Keys.N1LIMIT),
                new CDUMappedCommandKey(5, 1 << 5, false, CDU737Keys.FIX, "INS_UPDATE 1", "INS_UPDATE 0"),
                new CDUMappedCommandKey(5, 1 << 4, false, CDU737Keys.A),
                new CDUMappedCommandKey(5, 1 << 3, false, CDU737Keys.B),
                new CDUMappedCommandKey(5, 1 << 2, false, CDU737Keys.C),
                new CDUMappedCommandKey(5, 1 << 1, false, CDU737Keys.D),
                new CDUMappedCommandKey(5, 1 << 0, false, CDU737Keys.E),
                // Group 6
                new CDUMappedCommandKey(6, 1 << 6, false, CDU737Keys.PREV_PAGE, "VAL_DATA_ENTRY 1", "VAL_DATA_ENTRY 0"),
                new CDUMappedCommandKey(6, 1 << 5, false, CDU737Keys.NEXT_PAGE ,"MRK_POS 1", "MRK_POS 0"),
                new CDUMappedCommandKey(6, 1 << 4, false, CDU737Keys.F),
                new CDUMappedCommandKey(6, 1 << 3, false, CDU737Keys.G),
                new CDUMappedCommandKey(6, 1 << 2, false, CDU737Keys.H),
                new CDUMappedCommandKey(6, 1 << 1, false, CDU737Keys.I),
                new CDUMappedCommandKey(6, 1 << 0, false, CDU737Keys.J),

                // Group 7
                new CDUMappedCommandKey(7, 1 << 7, false, CDU737Keys.K1, "INS_BTN_1 1", "INS_BTN_1 0"),
                new CDUMappedCommandKey(7, 1 << 6, false, CDU737Keys.K2, "INS_BTN_2 1", "INS_BTN_2 0"),
                new CDUMappedCommandKey(7, 1 << 5, false, CDU737Keys.K3, "INS_BTN_3 1", "INS_BTN_3 0"),
                new CDUMappedCommandKey(7, 1 << 4, false, CDU737Keys.K),
                new CDUMappedCommandKey(7, 1 << 3, false, CDU737Keys.L),
                new CDUMappedCommandKey(7, 1 << 2, false, CDU737Keys.M),
                new CDUMappedCommandKey(7, 1 << 1, false, CDU737Keys.N),
                new CDUMappedCommandKey(7, 1 << 0, false, CDU737Keys.O),

                // Group 8
                new CDUMappedCommandKey(8, 1 << 7, false, CDU737Keys.K4, "INS_BTN_4 1", "INS_BTN_4 0"),
                new CDUMappedCommandKey(8, 1 << 6, false, CDU737Keys.K5, "INS_BTN_5 1", "INS_BTN_5 0"),
                new CDUMappedCommandKey(8, 1 << 5, false, CDU737Keys.K6, "INS_BTN_6 1", "INS_BTN_6 0"),
                new CDUMappedCommandKey(8, 1 << 4, false, CDU737Keys.P),
                new CDUMappedCommandKey(8, 1 << 3, false, CDU737Keys.Q),
                new CDUMappedCommandKey(8, 1 << 2, false, CDU737Keys.R),
                new CDUMappedCommandKey(8, 1 << 1, false, CDU737Keys.S),
                new CDUMappedCommandKey(8, 1 << 0, false, CDU737Keys.T),

                // Group 9
                new CDUMappedCommandKey(9, 1 << 7, false, CDU737Keys.K7, "INS_BTN_7 1", "INS_BTN_7 0"),
                new CDUMappedCommandKey(9, 1 << 6, false, CDU737Keys.K8, "INS_BTN_8 1", "INS_BTN_8 0"),
                new CDUMappedCommandKey(9, 1 << 5, false, CDU737Keys.K9, "INS_BTN_9 1", "INS_BTN_9 0"),
                new CDUMappedCommandKey(9, 1 << 4, false, CDU737Keys.U),
                new CDUMappedCommandKey(9, 1 << 3, false, CDU737Keys.V),
                new CDUMappedCommandKey(9, 1 << 2, false, CDU737Keys.W),
                new CDUMappedCommandKey(9, 1 << 1, false, CDU737Keys.X),
                new CDUMappedCommandKey(9, 1 << 0, false, CDU737Keys.Y),

                // Group 10
                new CDUMappedCommandKey(10, 1 << 7, false, CDU737Keys.KPT, "INS_CLR_BTN 1", "INS_CLR_BTN 0"),
                new CDUMappedCommandKey(10, 1 << 6, false, CDU737Keys.K0, "INS_BTN_0 1", "INS_BTN_0 0"),
                new CDUMappedCommandKey(10, 1 << 5, false, CDU737Keys.KPM, "INS_ENTER_BTN 1", "INS_ENTER_BTN 0"),
                new CDUMappedCommandKey(10, 1 << 4, false, CDU737Keys.Z),
                new CDUMappedCommandKey(10, 1 << 3, false, CDU737Keys.SP),
                new CDUMappedCommandKey(10, 1 << 2, false, CDU737Keys.DEL),
                new CDUMappedCommandKey(10, 1 << 1, false, CDU737Keys.SLASH),
                new CDUMappedCommandKey(10, 1 << 0, false, CDU737Keys.CLR, "INS_CLR_BTN 1", "INS_CLR_BTN 0"),
            };

            return result;
        }


    }
}
