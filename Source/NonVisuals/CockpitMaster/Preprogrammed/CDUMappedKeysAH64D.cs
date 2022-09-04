using NonVisuals.CockpitMaster.Switches;
using NonVisuals.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    public class CDUMappedKeyAH64D : CDUMappedKey
    {
        public CDUMappedKeyAH64D(int group, int mask, bool isOn, CDU737Keys _CDUKey, string commandOn = "", string commandOff = "")
            : base(group, mask, isOn, _CDUKey, commandOn, commandOff)
        {
        }

        public static HashSet<CDUMappedKey> GetMappedPanelKeys()
        {
            var result = new HashSet<CDUMappedKey>
            {
                // Group 1
                new CDUMappedKey(1, 1 << 7, false, CDU737Keys.RSK1),
                new CDUMappedKey(1, 1 << 6, false, CDU737Keys.RSK2),
                new CDUMappedKey(1, 1 << 5, false, CDU737Keys.RSK3),
                new CDUMappedKey(1, 1 << 4, false, CDU737Keys.RSK4),
                new CDUMappedKey(1, 1 << 3, false, CDU737Keys.RSK5),
                new CDUMappedKey(1, 1 << 2, false, CDU737Keys.RSK6),

                // Group 2
                new CDUMappedKey(2, 1 << 7, false, CDU737Keys.LSK1),
                new CDUMappedKey(2, 1 << 6, false, CDU737Keys.LSK2),
                new CDUMappedKey(2, 1 << 5, false, CDU737Keys.LSK3),
                new CDUMappedKey(2, 1 << 4, false, CDU737Keys.LSK4),
                new CDUMappedKey(2, 1 << 3, false, CDU737Keys.LSK5),
                new CDUMappedKey(2, 1 << 2, false, CDU737Keys.LSK6),

                // Group 3
                new CDUMappedKey(3, 1 << 6, false, CDU737Keys.INITREF),
                new CDUMappedKey(3, 1 << 5, false, CDU737Keys.RTE),
                new CDUMappedKey(3, 1 << 4, false, CDU737Keys.CLB),
                new CDUMappedKey(3, 1 << 3, false, CDU737Keys.CRZ),
                new CDUMappedKey(3, 1 << 2, false, CDU737Keys.DES),
                new CDUMappedKey(3, 1 << 1, false, CDU737Keys.BRT_MINUS),
                new CDUMappedKey(3, 1 << 0, false, CDU737Keys.BRT_PLUS),

                // Group 4
                
                new CDUMappedKey(4, 1 << 5, false, CDU737Keys.MENU),
                new CDUMappedKey(4, 1 << 4, false, CDU737Keys.LEGS),
                new CDUMappedKey(4, 1 << 3, false, CDU737Keys.DEP_ARR),
                new CDUMappedKey(4, 1 << 2, false, CDU737Keys.HOLD),
                new CDUMappedKey(4, 1 << 1, false, CDU737Keys.PROG),
                new CDUMappedKey(4, 1 << 0, false, CDU737Keys.EXEC, "PLT_KU_ENT 1" , "PLT_KU_ENT 0"),

                // Group 5
                new CDUMappedKey(5, 1 << 6, false, CDU737Keys.N1LIMIT),
                new CDUMappedKey(5, 1 << 5, false, CDU737Keys.FIX),
                new CDUMappedKey(5, 1 << 4, false, CDU737Keys.A , "PLT_KU_A 1" , "PLT_KU_A 0"),
                new CDUMappedKey(5, 1 << 3, false, CDU737Keys.B, "PLT_KU_B 1" , "PLT_KU_B 0"),
                new CDUMappedKey(5, 1 << 2, false, CDU737Keys.C, "PLT_KU_C 1" , "PLT_KU_C 0"),
                new CDUMappedKey(5, 1 << 1, false, CDU737Keys.D, "PLT_KU_D 1" , "PLT_KU_D 0"),
                new CDUMappedKey(5, 1 << 0, false, CDU737Keys.E, "PLT_KU_E 1" , "PLT_KU_E 0"),
                // Group 6
                new CDUMappedKey(6, 1 << 6, false, CDU737Keys.PREV_PAGE, "PLT_KU_LEFT 1" , "PLT_KU_LEFT 0"),
                new CDUMappedKey(6, 1 << 5, false, CDU737Keys.NEXT_PAGE, "PLT_KU_RIGHT 1" , "PLT_KU_RIGHT 0"),
                new CDUMappedKey(6, 1 << 4, false, CDU737Keys.F, "PLT_KU_F 1" , "PLT_KU_F 0"),
                new CDUMappedKey(6, 1 << 3, false, CDU737Keys.G, "PLT_KU_G 1" , "PLT_KU_G 0"),
                new CDUMappedKey(6, 1 << 2, false, CDU737Keys.H, "PLT_KU_H 1" , "PLT_KU_H 0"),
                new CDUMappedKey(6, 1 << 1, false, CDU737Keys.I, "PLT_KU_I 1" , "PLT_KU_I 0"),
                new CDUMappedKey(6, 1 << 0, false, CDU737Keys.J, "PLT_KU_J 1" , "PLT_KU_J 0"),

                // Group 7
                new CDUMappedKey(7, 1 << 7, false, CDU737Keys.K1, "PLT_KU_1 1" , "PLT_KU_1 0"),
                new CDUMappedKey(7, 1 << 6, false, CDU737Keys.K2, "PLT_KU_2 1" , "PLT_KU_2 0"),
                new CDUMappedKey(7, 1 << 5, false, CDU737Keys.K3, "PLT_KU_3 1" , "PLT_KU_3 0"),
                new CDUMappedKey(7, 1 << 4, false, CDU737Keys.K, "PLT_KU_K 1" , "PLT_KU_K 0"),
                new CDUMappedKey(7, 1 << 3, false, CDU737Keys.L, "PLT_KU_L 1" , "PLT_KU_L 0"),
                new CDUMappedKey(7, 1 << 2, false, CDU737Keys.M, "PLT_KU_M 1" , "PLT_KU_M 0"),
                new CDUMappedKey(7, 1 << 1, false, CDU737Keys.N, "PLT_KU_N 1" , "PLT_KU_N 0"),
                new CDUMappedKey(7, 1 << 0, false, CDU737Keys.O, "PLT_KU_O 1" , "PLT_KU_O 0"),

                // Group 8
                new CDUMappedKey(8, 1 << 7, false, CDU737Keys.K4, "PLT_KU_4 1" , "PLT_KU_4 0"),
                new CDUMappedKey(8, 1 << 6, false, CDU737Keys.K5, "PLT_KU_5 1" , "PLT_KU_5 0"),
                new CDUMappedKey(8, 1 << 5, false, CDU737Keys.K6, "PLT_KU_6 1" , "PLT_KU_6 0"),
                new CDUMappedKey(8, 1 << 4, false, CDU737Keys.P, "PLT_KU_P 1" , "PLT_KU_P 0"),
                new CDUMappedKey(8, 1 << 3, false, CDU737Keys.Q, "PLT_KU_Q 1" , "PLT_KU_Q 0"),
                new CDUMappedKey(8, 1 << 2, false, CDU737Keys.R, "PLT_KU_R 1" , "PLT_KU_R 0"),
                new CDUMappedKey(8, 1 << 1, false, CDU737Keys.S, "PLT_KU_S 1" , "PLT_KU_S 0"),
                new CDUMappedKey(8, 1 << 0, false, CDU737Keys.T, "PLT_KU_T 1" , "PLT_KU_T 0"),

                // Group 9
                new CDUMappedKey(9, 1 << 7, false, CDU737Keys.K7, "PLT_KU_7 1" , "PLT_KU_7 0"),
                new CDUMappedKey(9, 1 << 6, false, CDU737Keys.K8, "PLT_KU_8 1" , "PLT_KU_8 0"),
                new CDUMappedKey(9, 1 << 5, false, CDU737Keys.K9, "PLT_KU_9 1" , "PLT_KU_9 0"),
                new CDUMappedKey(9, 1 << 4, false, CDU737Keys.U, "PLT_KU_U 1" , "PLT_KU_U 0"),
                new CDUMappedKey(9, 1 << 3, false, CDU737Keys.V, "PLT_KU_V 1" , "PLT_KU_V 0"),
                new CDUMappedKey(9, 1 << 2, false, CDU737Keys.W, "PLT_KU_W 1" , "PLT_KU_W 0"),
                new CDUMappedKey(9, 1 << 1, false, CDU737Keys.X, "PLT_KU_X 1" , "PLT_KU_Y 0"),
                new CDUMappedKey(9, 1 << 0, false, CDU737Keys.Y, "PLT_KU_Y 1" , "PLT_KU_Y 0"),

                // Group 10
                new CDUMappedKey(10, 1 << 7, false, CDU737Keys.KPT, "PLT_KU_DOT 1" , "PLT_KU_DOT 0"),
                new CDUMappedKey(10, 1 << 6, false, CDU737Keys.K0, "PLT_KU_0 1" , "PLT_KU_0 0"),
                new CDUMappedKey(10, 1 << 5, false, CDU737Keys.KPM, "PLT_KU_SIGN 1", "PLT_KU_SIGN 0"),
                new CDUMappedKey(10, 1 << 4, false, CDU737Keys.Z, "PLT_KU_Z 1" , "PLT_KU_Z 0"),
                new CDUMappedKey(10, 1 << 3, false, CDU737Keys.SP, "PLT_KU_SPC 1" , "PLT_KU_SPC 0"),
                new CDUMappedKey(10, 1 << 2, false, CDU737Keys.DEL, "PLT_KU_BKS 1", "PLT_KU_BKS 0"),
                new CDUMappedKey(10, 1 << 1, false, CDU737Keys.SLASH, "PLT_KU_DIV 1" , "PLT_KU_DIV 0"),
                new CDUMappedKey(10, 1 << 0, false, CDU737Keys.CLR, "PLT_KU_CLR 1" , "PLT_KU_CLR 0"),
            };

            return result;
        }


    }
}
