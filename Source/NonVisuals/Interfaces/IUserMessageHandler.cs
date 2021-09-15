namespace NonVisuals.Interfaces
{
    using NonVisuals.EventArgs;

    public interface IUserMessageHandler
    {
        void UserMessage(object sender, UserMessageEventArgs e);
    }
}
