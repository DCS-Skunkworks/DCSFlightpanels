using Newtonsoft.Json;

namespace DCS_BIOS.Tests.Serialization.Common {
    internal static class JSonSettings
    {
        public static JsonSerializerSettings JsonDefaultSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) =>
            {
                throw new System.Exception($"JSON Serialization Error.{args.ErrorContext.Error.Message}");
            }
        };
    }
}
