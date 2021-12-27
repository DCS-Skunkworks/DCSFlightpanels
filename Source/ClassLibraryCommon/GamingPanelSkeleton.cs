namespace ClassLibraryCommon
{
    public class GamingPanelSkeleton
    {
        public GamingPanelEnum GamingPanelType { get; set; } = GamingPanelEnum.Unknown;
        public int VendorId { get; set; }
        public int ProductId { get; set; }

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
