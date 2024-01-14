using VerifyTests;

namespace StreamDeckSharp.Tests.VerifyStuff
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class DefaultVerifySettings
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        static DefaultVerifySettings()
        {
            VerifyImageSharp.Initialize();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings Build(bool autoVerify = false)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return new ExtendedVerifySettings
            {
                Directory = "VerifiedSnapshots",
                AutoVerify = autoVerify,
            };
        }
    }
}
