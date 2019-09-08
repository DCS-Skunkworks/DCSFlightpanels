using ClassLibraryCommon;

namespace NonVisuals
{
    public interface IGlobalHandler
    {
        void Attach(GamingPanel gamingPanel);
        void Detach(GamingPanel gamingPanel);
        DCSAirframe GetAirframe();
    }
}
