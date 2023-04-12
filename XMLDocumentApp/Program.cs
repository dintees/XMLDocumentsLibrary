using XMLDocumentLibrary;

Console.WriteLine("Hello, World!");

XMLService Service = new XMLService();
string conenctionString = @"DATA SOURCE=MSSQLServer;INITIAL CATALOG=BD2_Project;Server=(localdb)\mssqllocaldb";

Console.WriteLine("Connecting...");

if (!Service.Connect(conenctionString))
{
    Console.WriteLine("Error occurred while connecting to database!");
    return;
}

Console.WriteLine("Connected to dababase!");

Console.WriteLine($"Number of XML documents: {Service.CountDocuments()}");

Service.Disconnect();
