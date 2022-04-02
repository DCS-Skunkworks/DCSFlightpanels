using ClassLibraryCommon.Enums;

namespace ClassLibraryCommon
{
    public class DCSFPModule
    {
        public int ID { get; set; }
        public string JSONFilename { get; set; }
        public string Description { get; set; }
        public bool UseGenericRadio { get; set; } = false;

        public DCSFPModule(int id, string description, string jsonFilename)
        {
            ID = id;
            JSONFilename = jsonFilename;
            Description = description;
        }

        public bool IsModule(ManagedModule module)
        {
            return ID == (int)module;
        }
    }
}
