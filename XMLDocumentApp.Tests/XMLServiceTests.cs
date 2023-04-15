using Newtonsoft.Json.Bson;
using XMLDocumentLibrary;
using XMLDocumentLibrary.Models;

namespace XMLDocumentApp.Tests
{
    public class XMLServiceTests : IDisposable
    {
        private readonly XMLService _xService;
        public XMLServiceTests()
        {
            _xService = new XMLService(@"DATA SOURCE=MSSQLServer;INITIAL CATALOG=BD2_Project_Test;Server=(localdb)\mssqllocaldb");
            _xService.DeleteAllDocuments();
        }

        public void Dispose()
        {
            _xService.DeleteAllDocuments();
        }


        #region DocumentTests
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

        [Fact]
        public void Should_ThrowException_When_ModifiedDocumentHasInvalidXMLFormat()
        {
            var xml = @"<book><title>Lalka</title></book>";
            string title = "books_" + DateTime.Now.ToString("hh.mm.ss.ffffff");

            int id = _xService.CreateDocument("book", "testing", xml);

            Assert.Throws<Exception>(() => _xService.ModifyDocument(id, "newBook", "testing", "<book></boook>"));
        }

        [Fact]
        public void Should_ThrowException_When_ModifiedDocumentDoesNotExist()
        {
            Assert.Throws<Exception>(() => _xService.ModifyDocument(-123, "newBook", "testing"));
        }

        [Fact]
        public void Should_CorrectlyModifyRow_When_DocumentExists()
        {
            var xml = @"<book><title>Lalka</title></book>";
            int id = _xService.CreateDocument("book", "testing", xml);

            _xService.ModifyDocument(id, "newBook", "testing");

            XMLDoc doc = _xService.GetDocumentById(id)!;
            Assert.Equal("newBook", doc.Title);
        }

        [Fact]
        public void Should_ReturnFalse_When_DeletedDocumentWithIdDoesNotExist()
        {
            bool isDeleted = _xService.DeleteDocumentById(-123);
            Assert.False(isDeleted);
        }

        [Fact]
        public void Should_DeleteRowFromDatabase_When_DocumentWithIdExistsInDatabase()
        {
            var xml = @"<book><title>Lalka</title></book>";
            int newId = _xService.CreateDocument("books " + DateTime.Now.ToString("hh.mm.ss.ffffff"), "description", xml);

            bool isDeleted = _xService.DeleteDocumentById(newId);

            Assert.True(isDeleted);
            Assert.Throws<Exception>(() => _xService.GetDocumentById(newId));
        }

        [Fact]
        public void Should_ReturnZero_When_DeletedAllDocuments()
        {
            _xService.DeleteAllDocuments();

            int howMany = _xService.CountDocuments();
            Assert.Equal(0, howMany);
        }
        #endregion

        #region XMLOperations
        [Fact]
        public void Should_ReturnXMLString_When_NodeExists()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            string xmlString = _xService.GetNodes(id, "catalog/book/title")!;
            Assert.Equal("<title>Lalka</title><title>Pan Tadeusz</title>", xmlString);
        }

        [Fact]
        public void Should_ReturnXMLString_When_NodeDoesNotExist()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            string xmlString = _xService.GetNodes(id, "catalog/title")!;
            Assert.Equal("", xmlString);
        }

        [Fact]
        public void Should_ChangeTextNode_When_NodeExists()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            bool isModified = _xService.EditNodeText(id, "catalog/book[2]/title", "Sonety krymskie");
            Assert.True(isModified);

            string xmlString = _xService.GetNodes(id, "catalog/book[2]/title/text()")!;
            Assert.Equal("Sonety krymskie", xmlString);
        }

        [Fact]
        public void Should_ReturnFalse_When_XMLIsNotValid()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            bool isModified = _xService.AddNewNode(id, "catalog[1]", "<book><title>Pan Wolodyjowski</title><author>Henryk Sienkiewicz</author></boook>");
            Assert.False(isModified);
        }

        [Fact]
        public void Should_ReturnFalse_When_NodeDoesNotExist()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            bool isModified = _xService.AddNewNode(id, "books[1]", "<book><title>Pan Wolodyjowski</title><author>Henryk Sienkiewicz</author></boook>");
            Assert.False(isModified);
        }

        [Fact]
        public void Should_AddNewNode_When_NodeExists()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            bool isModified = _xService.AddNewNode(id, "catalog[1]", "<book><title>Pan Wolodyjowski</title><author>Henryk Sienkiewicz</author></book>");
            Assert.True(isModified);

            int count = int.Parse(_xService.GetNodes(id, "count(catalog/book)")!);
            Assert.Equal(3, count);
        }

        [Fact]
        public void Should_ThrowException_When_AttributeDoesNotExist()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            Assert.Throws<Exception>(() => _xService.GetValueOfAttribute(id, "catalog/book[2]", "ISBN"));
        }

        [Fact]
        public void Should_AddAttributeToNode_When_NodeExists()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            bool isModified = _xService.AddAttributeToNode(id, "catalog/book[2]", "ISBN", "123456");
            Assert.True(isModified);

            string isbn = _xService.GetValueOfAttribute(id, "catalog/book[2]", "ISBN");
            Assert.Equal("123456", isbn);
        }

        [Fact]
        public void Should_RemoveAttributeFromNode_When_NodeExists()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            bool isModified = _xService.RemoveAttributeFromNode(id, "catalog/book[2]", "id");
            Assert.True(isModified);
            Assert.Throws<Exception>(() => _xService.GetValueOfAttribute(id, "catalog/book[2]", "id"));
        }

        [Fact]
        public void Should_RemoveAttributeToNode_When_NodeAndAttributeExists()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";

            int id = _xService.CreateDocument("books", "testing", xml);

            bool isModified = _xService.RemoveAttributeFromNode(id, "catalog/book[2]", "id");

            Assert.Throws<Exception>(() => _xService.GetValueOfAttribute(id, "catalog/book[2]", "id"));
        }
        #endregion

    }
}