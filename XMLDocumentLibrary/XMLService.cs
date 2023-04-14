using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;
using System.Threading.Tasks;
using XMLDocumentLibrary.Models;
using System.Collections.Concurrent;

namespace XMLDocumentLibrary
{
    public class XMLService
    {
        private readonly string? _connectionString;

        public XMLService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region DocumentOperations
        /// <summary>
        /// Validates connection iwth database
        /// </summary>
        /// <returns>Bool value represents connectivity state</returns>
        public bool CheckConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                connection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Counts documents in the database
        /// </summary>
        /// <returns>A number of documents. An error occured when -1 was returned</returns>
        public async Task<int> CountDocumentsAsync()
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) AS howMany FROM XMLDocument", connection);
                SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();
                reader.Read();
                int howMany = (int)reader["howMany"];

                await reader.CloseAsync();
                await connection.CloseAsync();

                return howMany;
            }
            catch (Exception) { return -1; }
        }

        /// <summary>
        /// Insert document to database
        /// </summary>
        /// <param name="title">Title of the document</param>
        /// <param name="description">Description of the document</param>
        /// <param name="xmlString">Content of XML document</param>
        /// <returns>Bool value represents state of inserted document. If true - document has been saved</returns>
        /// <exception cref="Exception">An error with format of XML</exception>
        public bool CreateDocument(string title, string description, string xmlString)
        {
            // if document exists in the database
            int count = execScalarValue($"SELECT COUNT(*) AS value FROM XMLDocument WHERE title = '{title}'");
            if (count > 0) throw new Exception("Document with this title is now in the datanase");
            try
            {

                // validate XML format
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                // insert document to database
                int howMany = execNonQuery($"INSERT INTO XMLDocument (Title, Description, XDocument) VALUES (@title, @description, @xmlString)",
                    new List<(string, string)> { ("@title", title), ("@description", description), ("@xmlString", xmlString) });
                return (howMany > 0);
            }
            catch (XmlException e)
            {
                throw new Exception("XML format is invalid: " + e.Message);
            }
        }

        /// <summary>
        /// Insert document to database asynchnonously
        /// </summary>
        /// <param name="title">Title of the document</param>
        /// <param name="description">Description of the document</param>
        /// <param name="xmlString">Content of XML document</param>
        /// <returns>Bool value represents state of inserted document. If true - document has been saved</returns>
        /// <exception cref="Exception">An error with format of XML</exception>
        public async Task<bool> CreateDocumentAsync(string title, string description, string xmlString)
        {
            int count = execScalarValue($"SELECT COUNT(*) AS value FROM XMLDocument WHERE title = '{title}'");
            if (count > 0) throw new Exception("Document with this title is now in the datanase");
            try
            {
                // validate XML format
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                // insert document to database
                int howMany = await execNonQueryAsync($"INSERT INTO XMLDocument (Title, Description, XDocument) VALUES (@title, @description, @xmlString)",
                    new List<(string, string)> { ("@title", title), ("@description", description), ("@xmlString", xmlString) });
                return (howMany > 0);
            }
            catch (XmlException e)
            {
                throw new Exception("XML format is invalid: " + e.Message);
            }
        }

        /// <summary>
        /// Gets all documents from the database
        /// </summary>
        /// <returns>List of XMLDoc objects</returns>
        public List<XMLDoc>? GetAllDocuments()
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, Title, Description, XDocument FROM XMLDocument", connection);
                SqlDataReader reader = cmd.ExecuteReader();
                List<XMLDoc> xDocs = new List<XMLDoc>();
                while (reader.Read())
                    xDocs.Add(new XMLDoc { Id = (int)reader["Id"], Title = reader["Title"].ToString(), Description = reader["Description"].ToString(), XMLDocument = reader["XDocument"].ToString() });

                connection.Close();
                return xDocs;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Gets one document with specific id from the database.
        /// </summary>
        /// <param name="id">The id of the document</param>
        /// <returns>XMLDoc object</returns>
        public XMLDoc? GetDocumentById(int id)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, Title, Description, XDocument FROM XMLDocument WHERE id = " + id, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                XMLDoc xDoc = new XMLDoc { Id = (int)reader["Id"], Title = reader["Title"].ToString(), Description = reader["Description"].ToString(), XMLDocument = reader["XDocument"].ToString() };
                connection.Close();
                return xDoc;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Gets document using title
        /// </summary>
        /// <param name="title">Title of the document</param>
        /// <returns>XMLDoc object of the found document. If document does not exists -> null</returns>
        public XMLDoc? GetDocumentByTitle(string title)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, Title, Description, XDocument FROM XMLDocument WHERE title = @title", connection);
                cmd.Parameters.Add(new SqlParameter("@title", title));
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                XMLDoc xDoc = new XMLDoc { Id = (int)reader["Id"], Title = reader["Title"].ToString(), Description = reader["Description"].ToString(), XMLDocument = reader["XDocument"].ToString() };
                connection.Close();
                return xDoc;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Modifies properties of document, like title, description or XMLDocument
        /// </summary>
        /// <param name="id">Id of element which will be modified</param>
        /// <param name="newTitle">New title</param>
        /// <param name="newDescription">New description</param>
        /// <param name="newXMLDocument">New XML string document</param>
        /// <returns>True if document has been edited, false otherwise</returns>
        public bool ModifyDocument(int id, string? newTitle = null, string? newDescription = null, string? newXMLDocument = null)
        {
            if (newXMLDocument is not null)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(newXMLDocument);
                }
                catch (XmlException)
                {
                    return false;
                }
            }

            string sqlCommand = "UPDATE XMLDocument SET"
               + (newTitle is not null ? " Title = '" + newTitle + "'" : "")
               + (newDescription is not null ? ", Description = '" + newDescription + "'" : "")
               + (newXMLDocument is not null ? ", XDocument = '" + newXMLDocument + "'" : "")
               + " WHERE id = " + id;

            return (execNonQuery(sqlCommand) > 0);
        }

        /// <summary>
        /// Deletes one document with specified id from the database
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>True if document has been deleted</returns>
        public bool DeleteDocument(int id)
        {
            return (execNonQuery("DELETE FROM XMLDocument WHERE id = " + id) > 0);
        }
#endregion

        #region XMLOperation
        /// <summary>
        /// Get nodes from document with specific id
        /// </summary>
        /// <param name="id">Od of the document</param>
        /// <param name="xQuery">xQuery or xPath expression</param>
        /// <returns>Value of processed XML document</returns>
        public string? GetNodes(int id, string xQuery)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT XDocument.query('{xQuery}') AS xml FROM XMLDocument WHERE Id = @id", connection);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                string result = reader["xml"].ToString()!;
                connection.Close();
                return result;
            }
            catch (Exception e) { Console.WriteLine(e.Message); return null; }
        }

        public bool AddNewNode(int id, string newNodeString, string xQuery)
        {
            bool isValid = validateXML(newNodeString);
            if (isValid)
            {
                int howMany = execNonQuery($"UPDATE XMLDocument SET XDocument.modify('insert {newNodeString} as last into ({xQuery})') WHERE Id = @id;", new List<(string, string)> { ("@id", id.ToString()) });
                return (howMany > 0);
            }
            else return false;
        }

        public bool EditNodeText(int id, string xQuery, string newValue)
        {
            int howMany = execNonQuery($"UPDATE XMLDocument SET XDocument.modify('replace value of ({xQuery}/text())[1] with \"{@newValue}\"') WHERE Id = @id;", new List<(string, string)> { ("@id", id.ToString()) });
            return (howMany > 0);
        }

        public bool AddAttributeToNode(int id, string xQuery, string nameOfAttribute, string valueOfAttribute)
        {
            int howMany = execNonQuery($"UPDATE XMLDocument SET XDocument.modify('insert attribute {nameOfAttribute}{{\"{valueOfAttribute}\"}} into ({xQuery})[1]') WHERE Id = @id;", new List<(string, string)> { ("@id", id.ToString()) });
            return (howMany > 0);
        }

        public bool RemoveAttributeFromNode(int id, string xQuery, string nameOfAttribute)
        {
            int howMany = execNonQuery($"UPDATE XMLDocument SET XDocument.modify('delete {xQuery}/@{nameOfAttribute}') WHERE Id = @id;", new List<(string, string)> { ("@id", id.ToString()) });
            return (howMany > 0);
        }
        #endregion

        #region auxuliaryFunctions
        private static bool validateXML(string xmlString)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private int execNonQuery(string command, List<(string, string)>? parameters = null)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(command, connection);
                foreach (var param in parameters) cmd.Parameters.Add(new SqlParameter(param.Item1, param.Item2));
                int howMany = cmd.ExecuteNonQuery();
                connection.Close();
                return howMany;
            }
            catch (Exception e) { Console.WriteLine(e.Message); return -1; }
        }

        private int execScalarValue(string command)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(command, connection);
                var reader = cmd.ExecuteReader(); reader.Read();
                int value = (int)reader["value"];
                connection.Close();
                return value;
            }
            catch (Exception) { return -1; }
        }

        private async Task<int> execNonQueryAsync(string command, List<(string, string)>? parameters = null)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                SqlCommand cmd = new SqlCommand(command, connection);
                if (parameters is not null)
                    foreach (var param in parameters) cmd.Parameters.Add(new SqlParameter(param.Item1, param.Item2));
                int howMany = await cmd.ExecuteNonQueryAsync();
                await connection.CloseAsync();
                return howMany;
            }
            catch (Exception e) { await Console.Out.WriteLineAsync(e.Message); return -1; }
        }
        #endregion
    }
}