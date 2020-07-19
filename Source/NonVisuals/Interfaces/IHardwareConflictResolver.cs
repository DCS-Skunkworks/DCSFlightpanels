using System.Collections.Generic;

namespace NonVisuals.Interfaces
{
    public interface IHardwareConflictResolver
    {
        List<GenericPanelBinding> ResolveConflicts();
    }
}
