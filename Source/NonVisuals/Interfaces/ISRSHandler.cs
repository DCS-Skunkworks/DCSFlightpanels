using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals.Radios;

namespace NonVisuals.Interfaces
{
    public interface ISRSHandler
    {
        bool GuardIsOn { get; set; }
        double Frequency { get; set; }
        double Channel { get; set; }
        SRSRadioMode RadioMode { get; set; }
    }
}
