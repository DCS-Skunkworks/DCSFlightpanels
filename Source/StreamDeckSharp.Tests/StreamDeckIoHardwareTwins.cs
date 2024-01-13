using Newtonsoft.Json;
using StreamDeckSharp.Internals;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Newtonsoft.Json.Serialization;
using System.Linq;
using StreamDeckSharp.Tests.VerifyStuff;

namespace StreamDeckSharp.Tests
{
    [UsesVerify]
    public class StreamDeckIoHardwareTwins
    {
        public ExtendedVerifySettings Verifier { get; } = DefaultVerifySettings.Build();

        //Sets the classes attributes in alphabetical order to get a more predictable serialization.
        public class OrderedContractResolver : DefaultContractResolver
        {
            protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
            }
        }

        [Theory]
        [ClassData(typeof(AllHardwareInfoTestData))]
        internal async Task HardwareTwinsHaveExpectedValues(IHardwareInternalInfos hardware)
        {
            // This test is to make sure we don't accidentially change some important constants.
            Verifier.Initialize();

            Verifier
                .UseFileNameAsDirectory()
                .UseFileName(hardware.DeviceName)
                ;

            JsonSerializerSettings settings = new()
            {
                ContractResolver = new OrderedContractResolver()
            };

            var hardwareJson = JsonConvert.SerializeObject(hardware, Formatting.Indented, settings);
            await Verifier.VerifyJsonAsync(hardwareJson);
        }
    }
}
