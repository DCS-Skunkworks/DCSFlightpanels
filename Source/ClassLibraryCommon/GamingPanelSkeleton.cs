namespace ClassLibraryCommon
{
    using System.Text;

    public class GamingPanelSkeleton
    {
        public GamingPanelEnum GamingPanelType { get; set; } = GamingPanelEnum.Unknown;
        public int VendorId { get; set; }
        public int ProductId { get; set; }
        public string SerialNumber { get; set; }

        public GamingPanelSkeleton(GamingPanelVendorEnum gamingPanelVendor, GamingPanelEnum gamingPanelsEnum)
        {
            GamingPanelType = gamingPanelsEnum;
            VendorId = (int)gamingPanelVendor;
            ProductId = (int)gamingPanelsEnum;
        }

        public GamingPanelSkeleton(GamingPanelVendorEnum gamingPanelVendor, GamingPanelEnum gamingPanelsEnum, string serialNumber)
        {
            GamingPanelType = gamingPanelsEnum;
            VendorId = (int)gamingPanelVendor;
            ProductId = (int)gamingPanelsEnum;
            SerialNumber = serialNumber;
        }

        public bool HasSerialNumber()
        {
            return !string.IsNullOrEmpty(SerialNumber) && !SerialNumber.Equals("0");
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Skeleton : ").Append(GamingPanelType).Append(" ").Append(VendorId).Append(" ").Append(ProductId);
            return stringBuilder.ToString();
        }
    }
}
