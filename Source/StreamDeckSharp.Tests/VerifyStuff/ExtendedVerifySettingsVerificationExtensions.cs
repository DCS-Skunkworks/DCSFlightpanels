using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Argon;
using VerifyTests;
using VerifyXunit;

namespace StreamDeckSharp.Tests.VerifyStuff
{
    [SuppressMessage("Minor Code Smell", "S3236:Caller information arguments should not be provided explicitly", Justification = "Intentional")]
    [SuppressMessage("Naming", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "They are async!")]
    [SuppressMessage("Naming", "RCS1047FadeOut:Non-asynchronous method name should not end with 'Async'.", Justification = "They are async!")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class ExtendedVerifySettingsVerificationExtensions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyAsync(this ExtendedVerifySettings settings, byte[] target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.Verify(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyAsync(this ExtendedVerifySettings settings, Task<byte[]> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.Verify(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyAsync<T>(this ExtendedVerifySettings settings, T target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.Verify(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyAsync<T>(this ExtendedVerifySettings settings, Task<T> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.Verify(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyAsync<T>(this ExtendedVerifySettings settings, ValueTask<T> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.Verify(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyFileAsync(this ExtendedVerifySettings settings, string path)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.VerifyFile(path, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyFileAsync(this ExtendedVerifySettings settings, FileInfo path)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.VerifyFile(path, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyJsonAsync(this ExtendedVerifySettings settings, string target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.VerifyJson(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyJsonAsync(this ExtendedVerifySettings settings, JToken target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            //todo
            return null;//Verifier.VerifyJson(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask VerifyJsonAsync(this ExtendedVerifySettings settings, Stream target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.VerifyJson(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask ThrowsAsync(this ExtendedVerifySettings settings, Action target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.Throws(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask ThrowsAsync(this ExtendedVerifySettings settings, Func<object> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.Throws(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask ThrowsTaskAsync(this ExtendedVerifySettings settings, Func<Task> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.ThrowsTask(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask ThrowsTaskAsync<T>(this ExtendedVerifySettings settings, Func<Task<T>> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.ThrowsTask(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask ThrowsValueTaskAsync(this ExtendedVerifySettings settings, Func<ValueTask> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.ThrowsValueTask(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static SettingsTask ThrowsValueTaskAsync<T>(this ExtendedVerifySettings settings, Func<ValueTask<T>> target)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return Verifier.ThrowsValueTask(target, settings.BuildVerifySettings(), settings.GetSourceFileOrThrow());
        }

        private static string GetSourceFileOrThrow(this ExtendedVerifySettings settings)
        {
            if (string.IsNullOrEmpty(settings.CallerFilePath))
            {
                throw new InvalidOperationException("Source file is null or empty. Make sure to initialize the caller information.");
            }

            return settings.CallerFilePath;
        }
    }
}
