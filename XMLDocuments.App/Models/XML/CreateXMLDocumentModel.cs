using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace XMLDocuments.App.Models.XML
{
    public class CreateXMLDocumentModel
    {
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? XMLDocument { get; set; }
        public bool CreateFromFile { get; set; } = false;
        public IFormFile? File { get; set; }
    }
}
