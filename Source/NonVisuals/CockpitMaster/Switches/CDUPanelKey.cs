namespace NonVisuals.CockpitMaster.Switches
{
    using System.Collections.Generic;
    using Interfaces;

    public enum CDU737Keys
    {
        LSK1 = 0, LSK2, LSK3, LSK4, LSK5, LSK6,
        RSK1, RSK2, RSK3, RSK4, RSK5, RSK6,
        INITREF,
        RTE, CLB, CRZ, DES, BRT_MINUS, BRT_PLUS,
        MENU, LEGS, DEP_ARR, HOLD, PROG, EXEC,
        N1LIMIT, FIX, PREV_PAGE, NEXT_PAGE,
        A, B, C, D, E, F, G, H, I, J,
        K1, K2, K3, K4, K5, K6, K7, K8, K9, K0, KPT, KPM,
        K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, SP, DEL, SLASH, CLR,
    }

    public class CDUPanelKey : ICockpitMasterCDUKey
    {

        public CDUPanelKey(int group, int mask, bool isOn, CDU737Keys _CDUKey)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            CDUKey = _CDUKey;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }
        public CDU737Keys CDUKey { get; set; }

        public static HashSet<ICockpitMasterCDUKey> GetPanelKeys()
        {
            var result = new HashSet<ICockpitMasterCDUKey>
            {
                // Group 1
                new CDUPanelKey(1, 1 << 7, false, CDU737Keys.RSK1),
                new CDUPanelKey(1, 1 << 6, false, CDU737Keys.RSK2),
                new CDUPanelKey(1, 1 << 5, false, CDU737Keys.RSK3),
                new CDUPanelKey(1, 1 << 4, false, CDU737Keys.RSK4),
                new CDUPanelKey(1, 1 << 3, false, CDU737Keys.RSK5),
                new CDUPanelKey(1, 1 << 2, false, CDU737Keys.RSK6),

                // Group 2
                new CDUPanelKey(2, 1 << 7, false, CDU737Keys.LSK1),
                new CDUPanelKey(2, 1 << 6, false, CDU737Keys.LSK2),
                new CDUPanelKey(2, 1 << 5, false, CDU737Keys.LSK3),
                new CDUPanelKey(2, 1 << 4, false, CDU737Keys.LSK4),
                new CDUPanelKey(2, 1 << 3, false, CDU737Keys.LSK5),
                new CDUPanelKey(2, 1 << 2, false, CDU737Keys.LSK6),

                // Group 3
                new CDUPanelKey(3, 1 << 6, false, CDU737Keys.INITREF),
                new CDUPanelKey(3, 1 << 5, false, CDU737Keys.RTE),
                new CDUPanelKey(3, 1 << 4, false, CDU737Keys.CLB),
                new CDUPanelKey(3, 1 << 3, false, CDU737Keys.CRZ),
                new CDUPanelKey(3, 1 << 2, false, CDU737Keys.DES),
                new CDUPanelKey(3, 1 << 1, false, CDU737Keys.BRT_MINUS),
                new CDUPanelKey(3, 1 << 0, false, CDU737Keys.BRT_PLUS),

                // Group 4
                
                new CDUPanelKey(4, 1 << 5, false, CDU737Keys.MENU),
                new CDUPanelKey(4, 1 << 4, false, CDU737Keys.LEGS),
                new CDUPanelKey(4, 1 << 3, false, CDU737Keys.DEP_ARR),
                new CDUPanelKey(4, 1 << 2, false, CDU737Keys.HOLD),
                new CDUPanelKey(4, 1 << 1, false, CDU737Keys.PROG),
                new CDUPanelKey(4, 1 << 0, false, CDU737Keys.EXEC),

                // Group 5
                new CDUPanelKey(5, 1 << 6, false, CDU737Keys.N1LIMIT),
                new CDUPanelKey(5, 1 << 5, false, CDU737Keys.FIX),
                new CDUPanelKey(5, 1 << 4, false, CDU737Keys.A),
                new CDUPanelKey(5, 1 << 3, false, CDU737Keys.B),
                new CDUPanelKey(5, 1 << 2, false, CDU737Keys.C),
                new CDUPanelKey(5, 1 << 1, false, CDU737Keys.D),
                new CDUPanelKey(5, 1 << 0, false, CDU737Keys.E),
                // Group 6
                new CDUPanelKey(6, 1 << 6, false, CDU737Keys.PREV_PAGE),
                new CDUPanelKey(6, 1 << 5, false, CDU737Keys.NEXT_PAGE),
                new CDUPanelKey(6, 1 << 4, false, CDU737Keys.F),
                new CDUPanelKey(6, 1 << 3, false, CDU737Keys.G),
                new CDUPanelKey(6, 1 << 2, false, CDU737Keys.H),
                new CDUPanelKey(6, 1 << 1, false, CDU737Keys.I),
                new CDUPanelKey(6, 1 << 0, false, CDU737Keys.J),

                // Group 7
                new CDUPanelKey(7, 1 << 7, false, CDU737Keys.K1),
                new CDUPanelKey(7, 1 << 6, false, CDU737Keys.K2),
                new CDUPanelKey(7, 1 << 5, false, CDU737Keys.K3),
                new CDUPanelKey(7, 1 << 4, false, CDU737Keys.K),
                new CDUPanelKey(7, 1 << 3, false, CDU737Keys.L),
                new CDUPanelKey(7, 1 << 2, false, CDU737Keys.M),
                new CDUPanelKey(7, 1 << 1, false, CDU737Keys.N),
                new CDUPanelKey(7, 1 << 0, false, CDU737Keys.O),

                // Group 8
                new CDUPanelKey(8, 1 << 7, false, CDU737Keys.K4),
                new CDUPanelKey(8, 1 << 6, false, CDU737Keys.K5),
                new CDUPanelKey(8, 1 << 5, false, CDU737Keys.K6),
                new CDUPanelKey(8, 1 << 4, false, CDU737Keys.P),
                new CDUPanelKey(8, 1 << 3, false, CDU737Keys.Q),
                new CDUPanelKey(8, 1 << 2, false, CDU737Keys.R),
                new CDUPanelKey(8, 1 << 1, false, CDU737Keys.S),
                new CDUPanelKey(8, 1 << 0, false, CDU737Keys.T),

                // Group 9
                new CDUPanelKey(9, 1 << 7, false, CDU737Keys.K7),
                new CDUPanelKey(9, 1 << 6, false, CDU737Keys.K8),
                new CDUPanelKey(9, 1 << 5, false, CDU737Keys.K9),
                new CDUPanelKey(9, 1 << 4, false, CDU737Keys.U),
                new CDUPanelKey(9, 1 << 3, false, CDU737Keys.V),
                new CDUPanelKey(9, 1 << 2, false, CDU737Keys.W),
                new CDUPanelKey(9, 1 << 1, false, CDU737Keys.X),
                new CDUPanelKey(9, 1 << 0, false, CDU737Keys.Y),

                // Group 10
                new CDUPanelKey(10, 1 << 7, false, CDU737Keys.KPT),
                new CDUPanelKey(10, 1 << 6, false, CDU737Keys.K0),
                new CDUPanelKey(10, 1 << 5, false, CDU737Keys.KPM),
                new CDUPanelKey(10, 1 << 4, false, CDU737Keys.Z),
                new CDUPanelKey(10, 1 << 3, false, CDU737Keys.SP),
                new CDUPanelKey(10, 1 << 2, false, CDU737Keys.DEL),
                new CDUPanelKey(10, 1 << 1, false, CDU737Keys.SLASH),
                new CDUPanelKey(10, 1 << 0, false, CDU737Keys.CLR),

            };

            return result;
        }
    }
}

/*
 * Key mapping 
 * 
 Bx means byte X of the HID Input Report  ( 1st byte ?! ) 
 B2 is the 2nd Bye, mean xxxx[1] => Group 1

    bits  7    6        5    4    3       2    1         0
    ------------------------------------------------------
    B2  | RSK1 RSK2     RSK3 RSK4 RSK5    RSK6 *         *  
    B3  | LSK1 LSK2     LSK3 LSK4 LSK5    LSK6 *         *  
    B4  | *    INIT_REF RTE  CLB  DES     CRZ  BRT_MINUS BRT_PLUS
    B5  | *    *        MENU LEGS DEP_ARR HOLD PROG      EXEC
    B6  | *    N1       FIX  A    B       C    D         E 
    B7  | *    PREV     NEXT F    G       H    I         J
    B8  | 1    2        3    K    L       M    N         O
    B9  | 4    5        6    P    Q       R    S         T
    B10 | 7    8        9    U    V       W    X         Y
    B11 | .    0        ±    Z    sp      del  /         clr

*/
