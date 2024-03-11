using ExpectedObjects;
using Xunit;

namespace Tests.Common {
    public class DeepAssert
    {
        public static void Equal<T>(T expected, T actual)
        {
            if (expected is null || actual is null)
            {
                Assert.Equal(expected, actual);
                return;
            }
            actual.ToExpectedObject().ShouldEqual(expected);
        }
    }
}
