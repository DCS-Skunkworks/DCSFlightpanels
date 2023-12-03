﻿using StreamDeckSharp.Internals;
using System;
using System.IO;

namespace StreamDeckSharp.Tests
{
    internal sealed class StreamDeckHidTestContext : IDisposable
    {
        public StreamDeckHidTestContext(IHardwareInternalInfos hardware)
        {
            Hardware = hardware ?? throw new ArgumentNullException(nameof(hardware));
            Log = new StringWriter();
            Hid = new FakeStreamDeckHid(Log, Hardware);
            Board = new BasicHidClient(Hid, Hardware);
        }

        public TextWriter Log { get; }
        public FakeStreamDeckHid Hid { get; }
        public BasicHidClient Board { get; }
        public IHardwareInternalInfos Hardware { get; }

        public void Dispose()
        {
            Log.Dispose();
        }
    }
}
