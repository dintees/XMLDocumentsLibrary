using XMLDocumentLibrary;
using XMLDocumentLibrary.Models;

string conenctionString = @"DATA SOURCE=MSSQLServer;INITIAL CATALOG=BD2_Project;Server=(localdb)\mssqllocaldb";
XMLService xService = new XMLService(conenctionString);

if (!xService.CheckConnection())
{
    Console.WriteLine("Error occurred while connecting to database!");
    return;
}

Console.WriteLine("Connected to dababase!");

Console.WriteLine($"Number of XML documents: {await xService.CountDocumentsAsync()}");

var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<people>
<person>
<fname>Adam</fname>
<lname>Abacki</lname>
</person>
<person>
<fname>Bogdan</fname>
<lname>Babacki</lname>
</person>
</people>";

xml = @"<?xml version=""1.0"" ?> 
<catalog>
   <book id=""bk101"">
      <author>Gambardellas, Matthew</author>
      <title>XML Developers Guide</title>
      <genre>Computer</genre>
      <price>44.95</price>
      <publish_date>2000-10-01</publish_date>
      <description>An in-depth look at creating applications 
      with XML.</description>
   </book>
   <book id=""bk102"">
      <author>Ralls Kim</author>
      <title>Midnight Rain</title>
      <genre>Fantasy</genre>
      <price>5.95</price>
      <publish_date>2000-12-16</publish_date>
      <description>A former architect battles corporate zombies, 
      an evil sorceress and her own childhood to become queen 
      of the world.</description>
   </book>
</catalog>";

// Console.WriteLine(xml);

// Create document
try
{
    //var file = File.ReadAllText(@"D:\dotnet\XMLDocumentLibrary\XMLDocumentApp\books.xml");
    //file = file.Replace(Environment.NewLine, "");
    bool isCreated = await xService.CreateDocumentAsync("Books", "This is books document", xml);
    Console.WriteLine("Is created? : " + isCreated);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}


// Read document with specific id
XMLDoc? document = xService.GetDocumentById(5);
if (document is null) Console.WriteLine("There is no document with this ID");
else
{
    Console.WriteLine($"Title: {document.Title}\nDescription: {document.Description}\nContent: {document.XMLDocument}");
}

// Get document by name
document = xService.GetDocumentByTitle("Books");
if (document is null) Console.WriteLine("There is no document with this title");
else
{
    Console.WriteLine($"Title: {document.Title}\nDescription: {document.Description}\nContent: {document.XMLDocument}");
}

// Get all documents
List<XMLDoc> docs = xService.GetAllDocuments()!;
foreach (XMLDoc doc in docs)
{
    Console.WriteLine($" -- [{doc.Id}] {doc.Title}");
}

// Edit document parameters
// Console.WriteLine("Modified? : " + xService.ModifyDocument(5, "People", "Better XML document", xml));

// Delete document
Console.WriteLine("Deleted? : " + xService.DeleteDocument(7));

// Finding element using XQuery
Console.WriteLine("Finding elements: " + xService.GetNodes(17, "catalog/book/author"));

// Add new person to the end of catalog node
// Console.WriteLine("Created new node ? " + xService.AddNewNode(17, "<boaok><title>Lalka</title><author>Boleslaw Prus</author></book>", "catalog[1]"));

// Edit node text
Console.WriteLine("Edited node text ? " + xService.EditNodeText(17, "catalog/book[3]/title", "Lalka"));

// Add attribute to node
Console.WriteLine("Added attribute ? " + xService.AddAttributeToNode(17, "catalog/book[3]", "id", "bk103"));

// Remove attribute from node
Console.WriteLine("Removed attribute? : " + xService.RemoveAttributeFromNode(17, "catalog/book[3]", "age"));
