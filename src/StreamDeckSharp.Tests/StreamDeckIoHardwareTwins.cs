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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class StreamDeckIoHardwareTwins
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ExtendedVerifySettings Verifier { get; } = DefaultVerifySettings.Build();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        //Sets the classes attributes in alphabetical order to get a more predictable serialization.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class OrderedContractResolver : DefaultContractResolver
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
