namespace ControlReference.Events
{
    public class CategoryEventArgs : System.EventArgs
    {
        public object Sender { get; set; }
        public string Category { get; set; }
    }
}
