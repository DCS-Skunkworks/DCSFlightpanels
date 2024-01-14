using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace StreamDeckSharp.Tests.VerifyStuff
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class ExtendedVerifySettingsExtensions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static void Initialize(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            this ExtendedVerifySettings settings,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = -1
        )
        {
            settings.CallerLineNumber = lineNumber;
            settings.CallerMemberName = memberName;
            settings.CallerFilePath = filePath;

            var testClassFileName = Path.GetFileNameWithoutExtension(filePath);
            settings.UseFileName($"{testClassFileName}.{memberName}");

            // Use a file name for the test results. Overrides the
            // `{TestClassName}.{TestMethodName}_{Parameters}` of
            // `{Directory}/{TestClassName}.{TestMethodName}_{ParameterUniqueEtc}.verified.{extension}`.
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings UseFileNameAsDirectory(this ExtendedVerifySettings settings)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.UseAppendedFolder(settings.FileName).UseFileName(string.Empty);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings WithFileNameAsDirectory(this ExtendedVerifySettings settings)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.Clone().UseFileNameAsDirectory();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings UseFileName(this ExtendedVerifySettings settings, string fileName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            settings.FileName = fileName;
            return settings;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings WithFileName(this ExtendedVerifySettings settings, string fileName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.Clone().UseFileName(fileName);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings UseFileNameSuffix(this ExtendedVerifySettings settings, string suffix)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            settings.FileName += suffix;
            return settings;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings WithFileNameSuffix(this ExtendedVerifySettings settings, string suffix)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.Clone().UseFileNameSuffix(suffix);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings UseUniqueSuffix(this ExtendedVerifySettings settings, string suffix)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            settings.FileName += $"_{suffix}";
            return settings;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings WithUniqueSuffix(this ExtendedVerifySettings settings, string suffix)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.Clone().UseUniqueSuffix(suffix);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings UseExtension(this ExtendedVerifySettings settings, string extension)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            settings.Extension = extension;
            return settings;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings WithExtension(this ExtendedVerifySettings settings, string extension)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.Clone().UseExtension(extension);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings UseAppendedFolder(this ExtendedVerifySettings settings, string folderName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            settings.Directory = Path.Combine(settings.Directory, folderName);
            return settings;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings WithAppendedFolder(this ExtendedVerifySettings settings, string folderName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.Clone().UseAppendedFolder(folderName);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings UseUniqueClrSuffix(this ExtendedVerifySettings settings)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.UseUniqueSuffix($"CLR=v{Environment.Version}");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ExtendedVerifySettings WithUniqueClrSuffix(this ExtendedVerifySettings settings)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return settings.Clone().UseUniqueClrSuffix();
        }
    }
}
