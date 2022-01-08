using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.Interfaces
{
    public interface IProfileHandler
    {
        public void RegisterPanelBinding(GamingPanel gamingPanel, List<string> strings);
        public void RegisterJSONProfileData(GamingPanel gamingPanel, string jsonData);
    }
}
