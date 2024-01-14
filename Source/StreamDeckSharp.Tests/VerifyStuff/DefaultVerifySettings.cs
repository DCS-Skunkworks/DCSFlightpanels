using VerifyTests;

namespace StreamDeckSharp.Tests.VerifyStuff
{
    public static class DefaultVerifySettings
    {
        static DefaultVerifySettings()
        {
            VerifyImageSharp.Initialize();
        }

        public static ExtendedVerifySettings Build(bool autoVerify = false)
        {
            return new ExtendedVerifySettings
            {
                Directory = "VerifiedSnapshots",
                AutoVerify = autoVerify,
            };
        }
    }
}
