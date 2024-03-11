using Newtonsoft.Json;
using NonVisuals.Panels.StreamDeck;

namespace NonVisuals.Tests.Serialization.Common {
    internal static class JSonSettings {
        internal static JsonSerializerSettings JsonDefaultSettings = new()
        {
            ContractResolver = new ExcludeObsoletePropertiesResolver(),
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) => {
                throw new System.Exception($"JSON Serialization Error.{args.ErrorContext.Error.Message}");
            }
        };
    }
}
