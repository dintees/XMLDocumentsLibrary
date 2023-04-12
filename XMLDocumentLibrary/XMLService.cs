using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;
using System.Threading.Tasks;
using XMLDocumentLibrary.Models;

namespace XMLDocumentLibrary
{
    public class XMLService
    {
        private readonly string? _connectionString;

        public XMLService(string connectionString)
        {
            _connectionString = connectionString;
        }

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
            } catch (Exception)
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
            } catch (Exception) { return -1; }
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
            try
            {
                // validate XML format
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                // insert document to database
                int howMany = ExecNonQuery($"INSERT INTO XMLDocument (Title, Description, XDocument) VALUES ('{title}', '{description}', '{xmlString}')");
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
            try
            {
                // validate XML format
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                // insert document to database
                int howMany = await ExecNonQueryAsync($"INSERT INTO XMLDocument (Title, Description, XDocument) VALUES ('{title}', '{description}', '{xmlString}')");
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
            } catch (Exception) { return null; }
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

            return (ExecNonQuery(sqlCommand) > 0);
        }

        /// <summary>
        /// Deletes one document with specified id from the database
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>True if document has been deleted</returns>
        public bool DeleteDocument(int id)
        {
            return (ExecNonQuery("DELETE FROM XMLDocument WHERE id = " + id) > 0);
        }


        private int ExecNonQuery(string command)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(command, connection);
                int howMany = cmd.ExecuteNonQuery();
                connection.Close();
                return howMany;
            }
            catch (Exception) { return -1; }
        }

        private async Task<int> ExecNonQueryAsync(string command)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                SqlCommand cmd = new SqlCommand(command, connection);
                int howMany = await cmd.ExecuteNonQueryAsync();
                await connection.CloseAsync();
                return howMany;
            } catch (Exception) { return -1; }
        }
    }
}