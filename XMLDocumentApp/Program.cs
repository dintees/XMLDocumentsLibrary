using XMLDocumentLibrary;
using XMLDocumentLibrary.Models;

string conenctionString = @"DATA SOURCE=MSSQLServer;INITIAL CATALOG=BD2_Project;Server=(localdb)\mssqllocaldb";
XMLService XService = new XMLService(conenctionString);

if (!XService.CheckConnection())
{
    Console.WriteLine("Error occurred while connecting to database!");
    return;
}

Console.WriteLine("Connected to dababase!");

Console.WriteLine($"Number of XML documents: {await XService.CountDocumentsAsync()}");

var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<people>
<person>
<fname>Adam</fname>
<lname>Abacki</lname>
</person>
<fname>Bogdan</fname>
<lname>Babacki</lname>
</person>
</people>";

// Console.WriteLine(xml);

// Create document
/*try
{
    bool isCreated = await Service.CreateDocumentAsync("Title", "This is description", xml);
    Console.WriteLine("Is created? : " + isCreated);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}*/

// Read document with specific id
XMLDoc? document = XService.GetDocumentById(5);
if (document is null) Console.WriteLine("There is no document with this ID");
else
{
    Console.WriteLine($"Title: {document.Title}\nDescription: {document.Description}\nContent: {document.XMLDocument}");
}

// Read all documents
List<XMLDoc> docs = XService.GetAllDocuments()!;
foreach (XMLDoc doc in docs)
{
    Console.WriteLine($" -- [{doc.Id}] {doc.Title}");
}

// Edit document parameters
Console.WriteLine("Modified? : " + XService.ModifyDocument(5, "People", "Better XML document", "<people><person><fname>Adam</fname><lname>Abacki</lname></person></people>"));

// Delete document
Console.WriteLine("Deleted? : " + XService.DeleteDocument(7));