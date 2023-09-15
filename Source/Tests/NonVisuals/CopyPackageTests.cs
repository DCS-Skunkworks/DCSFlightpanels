using ClassLibraryCommon;
using MEF;
using NonVisuals;
using System.Collections.Generic;
using NonVisuals.KeyEmulation;
using Xunit;

namespace Tests.NonVisuals
{
    public class CopyPackageTests
    {
        [Fact]
        public void CopyPackage_MustBe_Serializable()
        {
            SortedList<int, IKeyPressInfo> keySequence = new();
            keySequence.Add(0, new KeyPressInfo());
            keySequence.Add(1, new KeyPressInfo());
            CopyPackage copyPackage = new()
            {
                Description = "Description AA",
                SourceName = "SourceName BB",
                ContentType = CopyContentType.KeySequence,
                Content = keySequence,
            };
            var clone = copyPackage.CloneJson();         
            Assert.Equal("Description AA", clone.Description);
            Assert.Equal("SourceName BB", clone.SourceName);
            Assert.IsType<SortedList<int, IKeyPressInfo>>(clone.Content);
        }
    }
}
