using Newtonsoft.Json.Bson;
using XMLDocumentLibrary;
using XMLDocumentLibrary.Models;

namespace XMLDocumentApp.Tests
{
    public class XMLServiceTests
    {
        private readonly XMLService _xService;
        public XMLServiceTests()
        {
            _xService = new XMLService(@"DATA SOURCE=MSSQLServer;INITIAL CATALOG=BD2_Project_Test;Server=(localdb)\mssqllocaldb");
        }

        [Fact]
        public void Should_ReturnTrue_When_ConnectionStringIsGood()
        {
            bool status = _xService.CheckConnection();
            Assert.True(status);
        }

        [Fact]
        public void Should_ThrowException_When_XMLFormatIsInvalid()
        {
            var xml = @"<book><title>Lalka</title></boook>";
            Assert.Throws<Exception>(() =>_xService.CreateDocument("title", "description", xml));
        }

        [Fact]
        public void Should_ThrowException_When_ThereIsDocumentWithTheSameTitle()
        {
            var xml = @"<book><title>Lalka</title></book>";
            string title = "books_" + DateTime.Now.ToString("hh.mm.ss.ffffff");
            _xService.CreateDocument(title, "description", xml);

            Assert.Throws<Exception>(() => _xService.CreateDocument(title, "description", xml));
        }

        [Fact]
        public void Should_InsertRowToDatabase_When_XMLFormalIsValid()
        {
            var xml = @"<book><title>Lalka</title></book>";
            int newId = _xService.CreateDocument("books " + DateTime.Now.ToString("hh.mm.ss.ffffff"), "description", xml);

            Assert.True(newId > 0);

            var xmlDoc = _xService.GetDocumentById(newId);
            Assert.NotNull(xmlDoc);
        }

        [Fact]
        public void Should_ThrowException_When_DocumentWithTheTitleDoesNotExist()
        {
            Assert.Throws<Exception>(() => _xService.GetDocumentByTitle("titleThatNotExist"));
        }

        [Fact]
        public void Should_ThrowException_When_DocumentWithIdDoesNotExist()
        {
            Assert.Throws<Exception>(() => _xService.GetDocumentById(-123));
        }

        [Fact]
        public void Should_ReturnXMLDocObject_When_DocumentWithTheTitleExists()
        {
            var xml = @"<book><title>Lalka</title></book>";
            string title = "books_" + DateTime.Now.ToString("hh.mm.ss.ffffff");

            _xService.CreateDocument(title, "testing", xml);

            var doc = _xService.GetDocumentByTitle(title);
            Assert.NotNull(doc);
            Assert.IsType<XMLDoc>(doc);
        }

    }
}