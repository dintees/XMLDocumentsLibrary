namespace XMLDocuments.App.Models.XML
{
    public class XMLViewModel
    {
        public int XMLDocumentId { get; set; }
        public string? XMLDocumentContent { get; set; }
        public string? SelectedFunction { get; set; }
        public string? XQuery { get; set; }
        public string? AttributeName { get; set; }
        public string? AttributeValue { get; set; }
        public string? Results { get; set; }
    }
}
