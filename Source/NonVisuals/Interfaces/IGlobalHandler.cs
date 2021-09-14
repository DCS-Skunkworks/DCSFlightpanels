namespace NonVisuals.Interfaces
{
    using ClassLibraryCommon;

    public interface IGlobalHandler
    {
        void Attach(GamingPanel gamingPanel);
        void Detach(GamingPanel gamingPanel);
        DCSFPProfile GetProfile();
    }
}
