using XMLDocumentLibrary.Models;

namespace XMLDocumentLibrary
{
    public interface IXMLService
    {
        bool AddAttributeToNode(int id, string xQuery, string nameOfAttribute, string valueOfAttribute);
        bool AddNewNode(int id, string xQuery, string newNodeString);
        bool CheckConnection();
        bool CheckNodeIfExists(int id, string xQuery);
        int CountDocuments();
        int CreateDocument(string title, string description, string xmlString);
        int CreateDocumentFromFile(string title, string description, string filepath);
        int DeleteAllDocuments();
        bool DeleteDocumentById(int id);
        bool DeleteNode(int id, string xQuery);
        bool EditNodeName(int id, string xQuery, string newName);
        bool EditNodeText(int id, string xQuery, string newValue);
        Dictionary<string, string>? GetAllAttributes(int id, string xQuery);
        List<string>? GetAllDocumentNodesQueries(int id, string xQuery);
        List<string>? GetAllDocumentNodesValues(int id, string xQuery);
        List<XMLDoc>? GetAllDocuments();
        XMLDoc GetDocumentById(int id);
        XMLDoc? GetDocumentByTitle(string title);
        string? GetNodes(int id, string xQuery);
        List<string>? GetNodesWithAttribute(int id, string xQuery, string nameOfAttribute, string? valueOfAttribute = null);
        string? GetNodeText(int id, string xQuery);
        List<Dictionary<string, string>>? GetStructuredNodes(int id, string xQuery, string[] values);
        string GetValueOfAttribute(int id, string xQuery, string nameOfAttribute);
        bool ModifyDocument(int id, string? newTitle = null, string? newDescription = null, string? newXMLDocument = null);
        bool RemoveAttributeFromNode(int id, string xQuery, string nameOfAttribute);
    }
}