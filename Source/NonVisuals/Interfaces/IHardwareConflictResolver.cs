using System.Collections.Generic;

namespace NonVisuals.Interfaces
{
    public interface IHardwareConflictResolver
    {
        bool ResolveConflicts(List<GenericPanelBinding> genericBindings, List<GamingPanel> gamingPanels);
    }
}
