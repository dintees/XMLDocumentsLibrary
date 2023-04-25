using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace XMLDocuments.App.Models.XML
{
    public class CreateXMLDocumentModel
    {
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? XMLDocument { get; set; }
        public bool CreateFromFile { get; set; } = false;
        public string? Filepath { get; set; }
    }
}
