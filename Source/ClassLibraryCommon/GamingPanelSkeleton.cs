namespace ClassLibraryCommon
{
    using System.Text;

    public class GamingPanelSkeleton
    {
        private GamingPanelEnum _gamingPanelsEnum = GamingPanelEnum.Unknown;
        private int _vendorId;
        private int _productId;
        private string _serialNumber;

        public GamingPanelSkeleton(GamingPanelVendorEnum gamingPanelVendor, GamingPanelEnum gamingPanelsEnum)
        {
            _gamingPanelsEnum = gamingPanelsEnum;
            _vendorId = (int)gamingPanelVendor;
            _productId = (int)gamingPanelsEnum;
        }

        public GamingPanelSkeleton(GamingPanelVendorEnum gamingPanelVendor, GamingPanelEnum gamingPanelsEnum, string serialNumber)
        {
            _gamingPanelsEnum = gamingPanelsEnum;
            _vendorId = (int)gamingPanelVendor;
            _productId = (int)gamingPanelsEnum;
            _serialNumber = serialNumber;
        }

        public bool HasSerialNumber()
        {
            return !string.IsNullOrEmpty(_serialNumber) && !_serialNumber.Equals("0");
        }

        public GamingPanelEnum GamingPanelType
        {
            get { return _gamingPanelsEnum; }
            set { _gamingPanelsEnum = value; }
        }

        public int VendorId
        {
            get { return _vendorId; }
            set { _vendorId = value; }
        }

        public int ProductId
        {
            get { return _productId; }
            set { _productId = value; }
        }

        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Skeleton : ").Append(GamingPanelType).Append(" ").Append(VendorId).Append(" ").Append(ProductId);
            return stringBuilder.ToString();
        }
    }
}
