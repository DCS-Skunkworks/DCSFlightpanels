namespace NonVisuals
{
    using System;
    using ClassLibraryCommon;

    [Serializable]
    public class CopyPackage
    {
        public string SourceName { get; set; } = string.Empty;
        public object Content { get; set; }
        public CopyContentType ContentType { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
