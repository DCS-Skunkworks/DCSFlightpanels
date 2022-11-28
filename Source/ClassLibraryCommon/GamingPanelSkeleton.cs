namespace ClassLibraryCommon
{
    /// <summary>
    /// Minimum information needed about a gaming panel. Type and manufacturer.
    /// </summary>
    public class GamingPanelSkeleton
    {
        public GamingPanelEnum GamingPanelType { get; }
        public int VendorId { get; }
        public int ProductId { get; }

        public GamingPanelSkeleton(GamingPanelVendorEnum gamingPanelVendor, GamingPanelEnum gamingPanelsEnum)
        {
            GamingPanelType = gamingPanelsEnum;
            VendorId = (int)gamingPanelVendor;
            ProductId = (int)gamingPanelsEnum;
        }

        public override string ToString()
        {
            return $"Skeleton : {GamingPanelType} {VendorId} {ProductId}";
        }
    }
}
