using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckSharp.Tests
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class AllHardwareInfoTestData : IEnumerable<object[]>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public IEnumerator<object[]> GetEnumerator()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return HardwareInfoResolver
                .GetAllHardwareInfos()
                .Select(h => new object[] { h })
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
