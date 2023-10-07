namespace ControlReference.Events
{
    public class CategoryEventArgs : System.EventArgs
    {
        public object Sender { get; set; }
        public string Category { get; set; }
    }

    public class DCSBIOSDataCombinedEventArgs : System.EventArgs
    {
        public object Sender { get; set; }
        public uint Address { get; set; }
        public bool IsUIntValue { get; set; }
        public string StringValue { get; set; }
        public uint Data { get; set; }
    }
}
