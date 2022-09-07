
namespace NonVisuals.CockpitMaster.Switches
{
    public class CDUMappedCommandKey : CDUPanelKey
    {
        public string CommmandOn { get; set; }
        public string CommmandOff { get; set; }
        public CDUMappedCommandKey(int group,
            int mask,
            bool isOn,
            CDU737Keys _CDUKey,
            string commandOn = "",
            string commandOff = "") : base(group, mask, isOn, _CDUKey)
        {
            CommmandOn = commandOn;
            CommmandOff = commandOff;
        }
        public string MappedCommand() => IsOn ? CommmandOn+"\n" : CommmandOff+"\n";

    }
}
