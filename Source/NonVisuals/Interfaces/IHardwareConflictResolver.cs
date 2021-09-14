namespace NonVisuals.Interfaces
{
    using System.Collections.Generic;

    public interface IHardwareConflictResolver
    {
        List<ModifiedGenericBinding> ResolveConflicts();
    }
}
