using System.Collections.Generic;
using NonVisuals.Panels;

namespace NonVisuals.Interfaces
{
    public interface IProfileHandler
    {
        public void RegisterPanelBinding(GamingPanel gamingPanel, List<string> strings);
        public void RegisterJSONProfileData(GamingPanel gamingPanel, string jsonData);
    }
}
