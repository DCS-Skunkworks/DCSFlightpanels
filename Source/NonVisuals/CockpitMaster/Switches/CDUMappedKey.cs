using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals.CockpitMaster.Preprogrammed;

namespace NonVisuals.CockpitMaster.Switches
{
    public class CDUMappedKey : CDUPanelKey
    {
        public string CommmandOn { get; set; }
        public string CommmandOff { get; set; }
        public CDUMappedKey(int group,
            int mask,
            bool isOn,
            CDU737Keys _CDUKey,
            String commandOn = "", 
            String commandOff ="" ) : base(group, mask, isOn, _CDUKey) 
        {
            CommmandOn = commandOn;
            CommmandOff = commandOff;

        }
        public string MappedCommand() => IsOn ? CommmandOn+"\n" : CommmandOff+"\n";

        public static explicit operator CDUMappedKey(HashSet<CDUMappedKeyA10C> v)
        {
            throw new NotImplementedException();
        }
    }
}
