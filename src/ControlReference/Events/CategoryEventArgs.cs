namespace ControlReference.Events
{
    public class CategoryEventArgs : System.EventArgs
    {
        public object Sender { get; set; }
        public string Category { get; init; }
    }

    public class DCSBIOSDataCombinedEventArgs : System.EventArgs
    {
        public object Sender { get; set; }
        public uint Address { get; init; }
        public bool IsUIntValue { get; init; }
        public string StringValue { get; init; }
        public uint Data { get; init; }
    }
}
