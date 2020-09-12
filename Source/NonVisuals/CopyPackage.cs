using System;
using ClassLibraryCommon;

namespace NonVisuals
{
    [Serializable]
    public class CopyPackage
    {
        private string _sourceName = "";
        private object _content;
        private CopyContentType _copyContentType;
        private string _description = "";

        public string SourceName
        {
            get => _sourceName;
            set => _sourceName = value;
        }

        public object Content
        {
            get => _content;
            set => _content = value;
        }

        public CopyContentType ContentType
        {
            get => _copyContentType;
            set => _copyContentType = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }
    }
}
