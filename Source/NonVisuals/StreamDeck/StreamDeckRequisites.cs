using System.Threading;
using StreamDeckSharp;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckRequisites
    {
        public StreamDeckPanel StreamDeck { get; set; }
        public IStreamDeckBoard StreamDeckBoard { get; set; }
        public CancellationToken ThreadCancellationToken { get; set; }
    }

    public static class DeckRequisite
    {
        public static StreamDeckRequisites Get(StreamDeckPanel streamDeck)
        {
            return new StreamDeckRequisites { StreamDeck = streamDeck };
        }

        public static StreamDeckRequisites Get(IStreamDeckBoard streamDeckBoard)
        {
            return new StreamDeckRequisites { StreamDeckBoard =  streamDeckBoard};
        }

        public static StreamDeckRequisites Get(CancellationToken threadCancellationToken)
        {
            return new StreamDeckRequisites { ThreadCancellationToken = threadCancellationToken};
        }
    }
}
