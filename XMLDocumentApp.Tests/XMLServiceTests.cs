using Newtonsoft.Json.Bson;
using System.Xml;
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
            Assert.Throws<XmlException>(() => _xService.CreateDocument("title", "description", xml));
        }

        [Fact]
        public void Should_ThrowException_When_ThereIsDocumentWithTheSameTitle()
        {
            var xml = @"<book><title>Lalka</title></book>";
            string title = "books_" + DateTime.Now.ToString("hh.mm.ss.ffffff");
            _xService.CreateDocument(title, "description", xml);

            var caughtException = Assert.Throws<Exception>(() => _xService.CreateDocument(title, "description", xml));
            Assert.Equal("Document with this title is now in the database", caughtException.Message);
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
            var ex = Assert.Throws<Exception>(() => _xService.GetDocumentByTitle("titleThatNotExist"));
            Assert.Equal("Document with this title does not exist", ex.Message);
        }

        [Fact]
        public void Should_ThrowException_When_DocumentWithIdDoesNotExist()
        {
            var ex = Assert.Throws<Exception>(() => _xService.GetDocumentById(-123));
            Assert.Equal("Document with this id does not exist", ex.Message);
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

            Assert.Throws<XmlException>(() => _xService.ModifyDocument(id, "newBook", "testing", "<book></boook>"));
        }

        [Fact]
        public void Should_ThrowException_When_ModifiedDocumentDoesNotExist()
        {
            var ex = Assert.Throws<Exception>(() => _xService.ModifyDocument(-123, "newBook", "testing"));
            Assert.Equal("There is no document with this id", ex.Message);
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
        public void Should_ReturnNull_When_NodeDoesNotExist()
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
            Assert.Null(xmlString);
        }

        [Fact]
        public void Should_ReturnNull_When_NodeDoesNotExistDuringReadingNodeText()
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

            string text = _xService.GetNodeText(id, "catalog/title")!;
            Assert.Null(text);
        }

        [Fact]
        public void Should_ReturnNodeText_When_NodeExists()
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

            string? value = _xService.GetNodeText(id, "catalog/book[2]/title");

            Assert.Equal("Pan Tadeusz", value);
        }

        [Fact]
        public void Should_ReturnNull_When_DuringExecGetAllDocumentNodesQueriesNodeWithTheNameDoesNotExist()
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

            List<string>? docs = _xService.GetAllDocumentNodesQueries(id, "//book/authors");
            Assert.Null(docs);
        }

        [Fact]
        public void Should_ReturnListOfXMLString_When_NodeWithTheNameExists()
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

            List<string> docs = _xService.GetAllDocumentNodesQueries(id, "//book/author")!;
            Assert.Equal(2, docs.Count);
            Assert.Equal("<author>Boleslaw Prus</author>", docs[0]);
            Assert.Equal("<author>Adam Mickiewicz</author>", docs[1]);
        }

        [Fact]
        public void Should_ReturnNull_When_NodeHasNoAttribute()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"" test=""test"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";
            int id = _xService.CreateDocument("books", "testing", xml);

            Dictionary<string, string>? docs = _xService.GetAllAttributes(id, "catalog/book[2]/title");
            Assert.Null(docs);
        }

        [Fact]
        public void Should_ThrowException_When_NodeDoesNotExistDuringGettingAllAttributes()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"" test=""test"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";
            int id = _xService.CreateDocument("books", "testing", xml);

            var caughtException = Assert.Throws<Exception>(() => _xService.GetAllAttributes(id, "catalog/books[1]/title"));
            Assert.Equal("Node with this path does not exist", caughtException.Message);
        }

        [Fact]
        public void Should_ReturnDictionaryWithAttributeNamesAndValues_When_NodeHasAtLeastOneAttribute()
        {
            var xml = @"<catalog>
            <book id=""1"">
                <title>Lalka</title>
                <author>Boleslaw Prus</author>
            </book>
            <book id=""2"" test=""test"">
                <title>Pan Tadeusz</title>
                <author>Adam Mickiewicz</author>
            </book>
            </catalog>";
            int id = _xService.CreateDocument("books", "testing", xml);

            Dictionary<string, string> docs = _xService.GetAllAttributes(id, "catalog/book[2]")!;
            Assert.Equal(2, docs.Count);
            Assert.Equal("2", docs["id"]);
            Assert.Equal("test", docs["test"]);
        }

        [Fact]
        public void Should_ReturnFalse_When_NodeDoenNotExist()
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

            bool exists = _xService.CheckNodeIfExists(id, "catalog/books[2]/author");
            Assert.False(exists);
        }

        [Fact]
        public void Should_ReturnTrue_When_NodeExists()
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

            bool exists = _xService.CheckNodeIfExists(id, "catalog/book[2]/author");
            Assert.True(exists);
        }

        [Fact]
        public void Should_ReturnNull_When_DuringExecGetAllDocumentNodesValuesNodeWithTheNameDoesNotExist()
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

            List<string>? docs = _xService.GetAllDocumentNodesQueries(id, "//book/authors");
            Assert.Null(docs);
        }

        [Fact]
        public void Should_ThrowException_When_GettingStructuredNodesButXQueryIsInvalid()
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

            Assert.Throws<Exception>(() => { var docs = _xService.GetStructuredNodes(id, "//books", new string[] { "author", "title" }); });
        }

        [Fact]
        public void Should_ThrowKeyNotFoundException_When_GettingStructuredNodesButTypedNodeNameIsInvalid()
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

            var docs = _xService.GetStructuredNodes(id, "//book", new string[] { "author", "title" });
            Assert.Throws<KeyNotFoundException>(() => docs![0]["title1"]);
        }

        [Fact]
        public void Should_ReturnNull_When_AttributeDoesNotExist()
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

            var docs = _xService.GetNodesWithAttribute(id, "//book", "id", "3");
            Assert.Null(docs);
        }

        [Fact]
        public void Should_ReturnListOfStrings_When_AttributeExistsAndTheValueIsNotGiven()
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

            var docs = _xService.GetNodesWithAttribute(id, "//book", "id");
            Assert.IsType<List<string>>(docs);
            Assert.Equal(2, docs.Count);
        }

        [Fact]
        public void Should_ReturnListOfStrings_When_SearchingAttributeWithValueExists()
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

            var docs = _xService.GetNodesWithAttribute(id, "//book", "id", "2");
            Assert.IsType<List<string>>(docs);
            Assert.Single(docs);
            Assert.Equal(@"<book id=""2""><title>Pan Tadeusz</title><author>Adam Mickiewicz</author></book>", docs[0]);
        }

        [Fact]
        public void Should_ReturnDictionaryWithValues_When_GettingStructuredNodes()
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

            var docs = _xService.GetStructuredNodes(id, "//book", new string[] { "author", "title" });
            Assert.IsType<List<Dictionary<string, string>>>(docs);
            Assert.Equal("Boleslaw Prus", docs[0]["author"]);
            Assert.Equal("Pan Tadeusz", docs[1]["title"]);
        }

        [Fact]
        public void Should_ReturnNull_When_NodeWithTheNameDoNotExistDuringGettialAllDocumentsWithTheName()
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

            List<string> docs = _xService.GetAllDocumentNodesValues(id, "//book/authors")!;
            Assert.Null(docs);
        }

        [Fact]
        public void Should_ReturnListOfString_When_NodeWithTheNameExists()
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

            List<string> docs = _xService.GetAllDocumentNodesValues(id, "//book/author")!;
            Assert.Equal(2, docs.Count);
            Assert.Equal("Boleslaw Prus", docs[0]);
            Assert.Equal("Adam Mickiewicz", docs[1]);
        }

        [Fact]
        public void Should_ReturnFalse_When_NodeDoesNotExistDuringEditingTextNode()
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

            bool isModified = _xService.EditNodeText(id, "catalog/book[4]/title", "Sonety krymskie");
            Assert.False(isModified);
        }

        [Fact]
        public void Should_ChangeTextNodeAndReturnTrue_When_NodeExists()
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
        public void Should_ThrowException_When_XMLIsNotValid()
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

            Assert.Throws<XmlException>(() => _xService.AddNewNode(id, "catalog[1]", "<book><title>Pan Wolodyjowski</title><author>Henryk Sienkiewicz</author></boook>"));
        }

        [Fact]
        public void Should_ReturnFalse_When_ChangingNameOfTheNodeButNodeDoesNotExist()
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

            bool isModified = _xService.EditNodeName(id, "catalog/book[2]/autor", "autor");
            Assert.False(isModified);
        }

        [Fact]
        public void Should_ModifyNodeNameAndReturnTrue_When_ChangingNameOfTheNode()
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

            bool isModified = _xService.EditNodeName(id, "catalog/book[2]/author", "autor");
            Assert.True(isModified);

            Assert.Equal("Adam Mickiewicz", _xService.GetNodeText(id, "catalog/book[2]/autor"));
            Assert.False(_xService.CheckNodeIfExists(id, "catalog/book[2]/author"));
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

            bool isModified = _xService.AddNewNode(id, "books[1]", "<book><title>Pan Wolodyjowski</title><author>Henryk Sienkiewicz</author></book>");
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
        public void Should_ReturnFalse_When_NodeExistsDuringAddingAttributeToNode()
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

            bool isModified = _xService.AddAttributeToNode(id, "catalog/book[3]", "ISBN", "123456");
            Assert.False(isModified);;
        }

        [Fact]
        public void Should_AddAttributeToNodeAndReturnTrue_When_NodeExistsDuringAddingAttriuteToNode()
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
        public void Should_ReturnFalse_When_NodeDoesNotExistDuringRemovingTheAttribute()
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

            bool isRemoved = _xService.RemoveAttributeFromNode(id, "catalog/book[3]", "id");
            Assert.False(isRemoved);
        }

        [Fact]
        public void Should_ReturnFalse_When_NodeStructureIsInCorrectDuringDeletingNode()
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

            bool isDeleted = _xService.DeleteNodeFromDocument(id, "catalog/book[3]/title");
            Assert.False(isDeleted);
        }

        [Fact]
        public void Should_DeleteNodeFromDocumentAndReturnTrue_When_NodeStructureIsCorrectDuringDeleteNode()
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

            bool isDeleted = _xService.DeleteNodeFromDocument(id, "catalog/book[2]/title");
            Assert.True(isDeleted);

            Assert.Null(_xService.GetNodeText(id, "catalog/book[2]/title"));
        }

        #endregion
    }
}