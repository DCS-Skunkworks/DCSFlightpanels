using DCS_BIOS.Json;
using System.Collections.Generic;

namespace DCS_BIOS.ControlLocator
{
    internal class DCSBIOSControlComparer : IEqualityComparer<DCSBIOSControl>
    {
        // DCSBIOSControls are equal if their names are equal.
        public bool Equals(DCSBIOSControl x, DCSBIOSControl y)
        {

            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Identifier == y.Identifier;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(DCSBIOSControl dcsbiosControl)
        {
            //Get hash code for the Identifier field if it is not null.
            return dcsbiosControl.Identifier == null ? 0 : dcsbiosControl.Identifier.GetHashCode();
        }
    }
}
