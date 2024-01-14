using System;
using VerifyTests;

namespace StreamDeckSharp.Tests.VerifyStuff
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ExtendedVerifySettings : ICloneable
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string CallerFilePath { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string CallerMemberName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int CallerLineNumber { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string Directory { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string FileName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string Extension { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool AutoVerify { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public VerifySettings BuildVerifySettings()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var settings = new VerifySettings();

            if (string.IsNullOrEmpty(FileName))
            {
                settings.UseFileName("Snapshot");
            }
            else
            {
                settings.UseFileName(FileName);
            }

            if (!string.IsNullOrEmpty(Extension))
            {
                //todo
                //settings.UseExtension(Extension);
            }

            settings.UseDirectory(Directory);

            if (AutoVerify)
            {
                settings.AutoVerify(false);
            }

            return settings;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ExtendedVerifySettings Clone()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return (ExtendedVerifySettings)MemberwiseClone();
        }
    }
}
