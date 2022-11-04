using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    public class CDUMappedCommandKeySA342 : CDUMappedCommandKey
    {
        public CDUMappedCommandKeySA342(int group, int mask, bool isOn, CDU737Keys _CDUKey, string commandOn = "", string commandOff = "")
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
                new CDUMappedCommandKey(1, 1 << 3, false, CDU737Keys.RSK5, "NADIR_PARAMETER INC" ,""),
                new CDUMappedCommandKey(1, 1 << 2, false, CDU737Keys.RSK6, "NADIR_PARAMETER DEC" ,""),

                // Group 2
                new CDUMappedCommandKey(2, 1 << 7, false, CDU737Keys.LSK1),
                new CDUMappedCommandKey(2, 1 << 6, false, CDU737Keys.LSK2),
                new CDUMappedCommandKey(2, 1 << 5, false, CDU737Keys.LSK3),
                new CDUMappedCommandKey(2, 1 << 4, false, CDU737Keys.LSK4),
                new CDUMappedCommandKey(2, 1 << 3, false, CDU737Keys.LSK5, "NADIR_DOPPLER_MODE INC",""),
                new CDUMappedCommandKey(2, 1 << 2, false, CDU737Keys.LSK6, "NADIR_DOPPLER_MODE DEC",""),

                // Group 3
                new CDUMappedCommandKey(3, 1 << 6, false, CDU737Keys.INITREF, "NADIR_ENTER_BUTTON 1", "NADIR_ENTER_BUTTON 0"),
                new CDUMappedCommandKey(3, 1 << 5, false, CDU737Keys.RTE ,"NADIR_DEST_BUTTON 1" , "NADIR_DEST_BUTTON 0"),
                new CDUMappedCommandKey(3, 1 << 4, false, CDU737Keys.CLB, "NADIR_AUX_BUTTON 1","NADIR_AUX_BUTTON 0"),
                new CDUMappedCommandKey(3, 1 << 3, false, CDU737Keys.CRZ, "NADIR_MAP_IND_BUTTON 1", "NADIR_MAP_IND_BUTTON 0"),
                new CDUMappedCommandKey(3, 1 << 2, false, CDU737Keys.DES),
                new CDUMappedCommandKey(3, 1 << 1, false, CDU737Keys.BRT_MINUS),
                new CDUMappedCommandKey(3, 1 << 0, false, CDU737Keys.BRT_PLUS),

                // Group 4
                
                new CDUMappedCommandKey(4, 1 << 5, false, CDU737Keys.MENU, "NADIR_GEO_BUTTON 1", "NADIR_GEO_BUTTON 0"),
                new CDUMappedCommandKey(4, 1 << 4, false, CDU737Keys.LEGS, "NADIR_POL_BUTTON 1", "NADIR_POL_BUTTON 0"),
                new CDUMappedCommandKey(4, 1 << 3, false, CDU737Keys.DEP_ARR),
                new CDUMappedCommandKey(4, 1 << 2, false, CDU737Keys.HOLD),
                new CDUMappedCommandKey(4, 1 << 1, false, CDU737Keys.PROG),
                new CDUMappedCommandKey(4, 1 << 0, false, CDU737Keys.EXEC, "NADIR_ENTER_BUTTON 1", "NADIR_ENTER_BUTTON 0"),

                // Group 5
                new CDUMappedCommandKey(5, 1 << 6, false, CDU737Keys.N1LIMIT),
                new CDUMappedCommandKey(5, 1 << 5, false, CDU737Keys.FIX, "NADIR_POS_BUTTON 1", "NADIR_POS_BUTTON 0"),
                new CDUMappedCommandKey(5, 1 << 4, false, CDU737Keys.A),
                new CDUMappedCommandKey(5, 1 << 3, false, CDU737Keys.B),
                new CDUMappedCommandKey(5, 1 << 2, false, CDU737Keys.C),
                new CDUMappedCommandKey(5, 1 << 1, false, CDU737Keys.D),
                new CDUMappedCommandKey(5, 1 << 0, false, CDU737Keys.E),
                // Group 6
                new CDUMappedCommandKey(6, 1 << 6, false, CDU737Keys.PREV_PAGE),
                new CDUMappedCommandKey(6, 1 << 5, false, CDU737Keys.NEXT_PAGE,"NADIR_DOWN_ARROW 1", "NADIR_DOWN_ARROW 0"),
                new CDUMappedCommandKey(6, 1 << 4, false, CDU737Keys.F),
                new CDUMappedCommandKey(6, 1 << 3, false, CDU737Keys.G),
                new CDUMappedCommandKey(6, 1 << 2, false, CDU737Keys.H),
                new CDUMappedCommandKey(6, 1 << 1, false, CDU737Keys.I),
                new CDUMappedCommandKey(6, 1 << 0, false, CDU737Keys.J),

                // Group 7
                new CDUMappedCommandKey(7, 1 << 7, false, CDU737Keys.K1, "NADIR_1_BUTTON 1","NADIR_1_BUTTON 0"),
                new CDUMappedCommandKey(7, 1 << 6, false, CDU737Keys.K2, "NADIR_2_BUTTON 1","NADIR_2_BUTTON 0"),
                new CDUMappedCommandKey(7, 1 << 5, false, CDU737Keys.K3, "NADIR_3_BUTTON 1","NADIR_3_BUTTON 0"),
                new CDUMappedCommandKey(7, 1 << 4, false, CDU737Keys.K),
                new CDUMappedCommandKey(7, 1 << 3, false, CDU737Keys.L),
                new CDUMappedCommandKey(7, 1 << 2, false, CDU737Keys.M),
                new CDUMappedCommandKey(7, 1 << 1, false, CDU737Keys.N),
                new CDUMappedCommandKey(7, 1 << 0, false, CDU737Keys.O),

                // Group 8
                new CDUMappedCommandKey(8, 1 << 7, false, CDU737Keys.K4, "NADIR_4_BUTTON 1","NADIR_4_BUTTON 0"),
                new CDUMappedCommandKey(8, 1 << 6, false, CDU737Keys.K5, "NADIR_5_BUTTON 1","NADIR_5_BUTTON 0"),
                new CDUMappedCommandKey(8, 1 << 5, false, CDU737Keys.K6, "NADIR_6_BUTTON 1","NADIR_6_BUTTON 0"),
                new CDUMappedCommandKey(8, 1 << 4, false, CDU737Keys.P),
                new CDUMappedCommandKey(8, 1 << 3, false, CDU737Keys.Q),
                new CDUMappedCommandKey(8, 1 << 2, false, CDU737Keys.R),
                new CDUMappedCommandKey(8, 1 << 1, false, CDU737Keys.S),
                new CDUMappedCommandKey(8, 1 << 0, false, CDU737Keys.T),

                // Group 9
                new CDUMappedCommandKey(9, 1 << 7, false, CDU737Keys.K7, "NADIR_7_BUTTON 1","NADIR_7_BUTTON 0"),
                new CDUMappedCommandKey(9, 1 << 6, false, CDU737Keys.K8, "NADIR_8_BUTTON 1","NADIR_8_BUTTON 0"),
                new CDUMappedCommandKey(9, 1 << 5, false, CDU737Keys.K9, "NADIR_9_BUTTON 1","NADIR_9_BUTTON 0"),
                new CDUMappedCommandKey(9, 1 << 4, false, CDU737Keys.U),
                new CDUMappedCommandKey(9, 1 << 3, false, CDU737Keys.V),
                new CDUMappedCommandKey(9, 1 << 2, false, CDU737Keys.W),
                new CDUMappedCommandKey(9, 1 << 1, false, CDU737Keys.X),
                new CDUMappedCommandKey(9, 1 << 0, false, CDU737Keys.Y),

                // Group 10
                new CDUMappedCommandKey(10, 1 << 7, false, CDU737Keys.KPT, "NADIR_GEL_BUTTON 1","NADIR_GEL_BUTTON 0"),
                new CDUMappedCommandKey(10, 1 << 6, false, CDU737Keys.K0, "NADIR_0_BUTTON 1","NADIR_0_BUTTON 0"),
                new CDUMappedCommandKey(10, 1 << 5, false, CDU737Keys.KPM,"NADIR_EFF_BUTTON 1", "NADIR_EFF_BUTTON 0"),
                new CDUMappedCommandKey(10, 1 << 4, false, CDU737Keys.Z),
                new CDUMappedCommandKey(10, 1 << 3, false, CDU737Keys.SP),
                new CDUMappedCommandKey(10, 1 << 2, false, CDU737Keys.DEL,"NADIR_EFF_BUTTON 1", "NADIR_EFF_BUTTON 0"),
                new CDUMappedCommandKey(10, 1 << 1, false, CDU737Keys.SLASH),
                new CDUMappedCommandKey(10, 1 << 0, false, CDU737Keys.CLR,"NADIR_EFF_BUTTON 1", "NADIR_EFF_BUTTON 0"),
            };

            return result;
        }


    }
}
