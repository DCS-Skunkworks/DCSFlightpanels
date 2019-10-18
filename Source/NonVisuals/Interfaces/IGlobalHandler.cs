using ClassLibraryCommon;

namespace NonVisuals.Interfaces
{
    public interface IGlobalHandler
    {
        void Attach(GamingPanel gamingPanel);
        void Detach(GamingPanel gamingPanel);
        DCSAirframe GetAirframe();
    }
}
