using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCS_BIOS;

namespace NonVisuals.DCSBIOSBindings
{
    public class DCSBIOSActionBindingSkeleton
    {
        public string MultiPanelMode;
        public string KeyName;
        public string Description;
        public bool WhenTurnedOn;
        public bool IsSequenced;
        public List<DCSBIOSInput> DCSBIOSInputs;
    }
}
