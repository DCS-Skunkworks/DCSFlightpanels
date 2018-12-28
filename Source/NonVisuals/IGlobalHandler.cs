using ClassLibraryCommon;

namespace NonVisuals
{
    public interface IGlobalHandler
    {
        void Attach(SaitekPanel saitekPanel);
        void Detach(SaitekPanel saitekPanel);
        DCSAirframe GetAirframe();
    }
}
