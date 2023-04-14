using XMLDocumentLibrary;

namespace XMLDocumentApp.Tests
{
    public class XMLServiceTests
    {
        private readonly XMLService _xService;
        public XMLServiceTests()
        {
            _xService = new XMLService(@"DATA SOURCE=MSSQLServer;INITIAL CATALOG=BD2_Project;Server=(localdb)\mssqllocaldb");
        }

        [Fact]
        public void Should_ReturnTrue_When_ConnectionStringIsGood()
        {
            bool status = _xService.CheckConnection();
            Assert.True(status);
        }

        [Fact]
        public void Should_ReturnFalse_When_XMLformatIsInvalid()
        {
            var xml = @"<book><title>Lalka</title></boook>";
            Assert.Throws<Exception>(() =>_xService.CreateDocument("title", "description", xml));
        }
    }
}