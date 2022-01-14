using System.Collections.Generic;

namespace NonVisuals.Interfaces
{
    public interface IProfileHandler
    {
        public void RegisterPanelBinding(GamingPanel gamingPanel, List<string> strings);
        public void RegisterJSONProfileData(GamingPanel gamingPanel, string jsonData);
    }
}
