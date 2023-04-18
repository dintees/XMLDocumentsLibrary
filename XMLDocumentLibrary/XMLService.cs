using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;
using System.Threading.Tasks;
using XMLDocumentLibrary.Models;
using System.Collections.Concurrent;
using System.Text;

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
        /// <returns>Number of documents. An error occured when -1 was returned</returns>
        public int CountDocuments()
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) AS howMany FROM XMLDocument", connection);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                reader.Read();
                int howMany = (int)reader["howMany"];

                reader.Close();
                connection.Close();

                return howMany;
            }
            catch (Exception) { return -1; }
        }

        /// <summary>
        /// Creates document in the database with content from string variable
        /// </summary>
        /// <param name="title">Title of the document</param>
        /// <param name="description">Description of the document</param>
        /// <param name="xmlString">Content of the XML document</param>
        /// <returns>Id of element which is in database</returns>
        /// <exception cref="XmlException">A format of XML file is invalid</exception>
        public int CreateDocument(string title, string description, string xmlString)
        {
            // if document exists in the database
            int count = execScalarValue($"SELECT COUNT(*) AS value FROM XMLDocument WHERE title = '{title}'");
            if (count > 0) throw new Exception("Document with this title is now in the datanase");
            // validate XML format
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            // insert document to database
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlCommand insertCommand = new SqlCommand("INSERT INTO XMLDocument (Title, Description, XDocument) VALUES (@title, @description, @xmlString)", connection);
            insertCommand.Parameters.Add(new SqlParameter("@title", title));
            insertCommand.Parameters.Add(new SqlParameter("@description", description));
            insertCommand.Parameters.Add(new SqlParameter("@xmlString", xmlString));
            int numberOfInserted = insertCommand.ExecuteNonQuery();
            int id = -1;
            // get document id
            if (numberOfInserted > 0)
            {
                SqlCommand selectCommand = new SqlCommand("SELECT IDENT_CURRENT('XMLDocument') AS Id", connection);
                SqlDataReader reader = selectCommand.ExecuteReader();
                reader.Read();
                id = int.Parse(reader["Id"].ToString()!);
                reader.Close();
            }
            connection.Close();
            return id;
        }

        /// <summary>
        /// Creates new document with a given content from the file
        /// </summary>
        /// <param name="title">Title of the document</param>
        /// <param name="description">Description of the document</param>
        /// <param name="filepath">Path where the file is stored with the filename</param>
        /// <returns>Id of new created document in database</returns>
        public int CreateDocumentFromFile(string title, string description, string filepath)
        {
            string lines = File.ReadAllText(filepath);
            return CreateDocument(title, description, lines);
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
                
                reader.Close();
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
                reader.Close();
                connection.Close();
                return xDoc;
            }
            catch (Exception) { throw new Exception("Document with this id does not exist"); }
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
            catch (Exception) { throw new Exception("Document with this title does not exist"); }
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
                catch (XmlException e)
                {
                    throw new Exception("XML format is invalid: " + e.Message);
                }
            }

            string sqlCommand = "UPDATE XMLDocument SET"
               + (newTitle is not null ? " Title = '" + newTitle + "'" : "")
               + (newDescription is not null ? ", Description = '" + newDescription + "'" : "")
               + (newXMLDocument is not null ? ", XDocument = '" + newXMLDocument + "'" : "")
               + " WHERE id = " + id;

            int howMany = execNonQuery(sqlCommand);
            if (howMany == 0) throw new Exception("There is no document with this id");
            return true;
        }

        /// <summary>
        /// Deletes one document with specified id from the database
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>True if document has been deleted</returns>
        public bool DeleteDocumentById(int id)
        {
            return (execNonQuery("DELETE FROM XMLDocument WHERE id = " + id) > 0);
        }

        /// <summary>
        /// Deletes all documents from the database
        /// </summary>
        /// <returns>Number of deleted rows</returns>
        public int DeleteAllDocuments()
        {
            return (execNonQuery("DELETE FROM XMLDocument"));
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
            catch (Exception) { return null; }
        }

        public string? GetNodeText(int id, string xQuery)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT XDocument.value('({xQuery})[1]', 'varchar(255)') AS xml FROM XMLDocument WHERE Id = @id", connection);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                string result = reader["xml"].ToString()!;
                reader.Close();
                connection.Close();
                return result == "" ? null : result;
            }
            catch (Exception) { return null; }
        }

        public List<string>? GetAllDocumentNodesQueries(int id, string xQuery)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT T.c.query('.') AS results FROM XMLDocument CROSS APPLY XDocument.nodes('{xQuery}') AS T(c) WHERE Id = @id", connection);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = cmd.ExecuteReader();
                List<string> docs = new();
                while (reader.Read())
                {
                    docs.Add(reader["results"].ToString()!);
                }
                if (docs.Count == 0) return null;
                reader.Close();
                connection.Close();
                return docs;
            }
            catch (Exception) { return null; }
        }

        public List<string>? GetAllDocumentNodesValues(int id, string xQuery)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT T.c.value('.', 'varchar(255)') AS results FROM XMLDocument CROSS APPLY XDocument.nodes('{xQuery}') AS T(c) WHERE Id = @id", connection);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = cmd.ExecuteReader();
                List<string> docs = new();
                while (reader.Read())
                {
                    docs.Add(reader["results"].ToString()!);
                }
                if (docs.Count == 0) return null;
                reader.Close();
                connection.Close();
                return docs;
            }
            catch (Exception) { return null; }
        }

        public bool CheckNodeIfExists(int id, string xQuery)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT XDocument.exist('{xQuery}') AS ex FROM XMLDocument WHERE Id = @id", connection);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                bool exists = (bool)reader["ex"]!;
                reader.Close();
                connection.Close();
                return exists;
            }
            catch { return false; }
        }

        public Dictionary<string, string>? GetAllAttributes(int id, string xQuery)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT attr.value('local-name(.)', 'varchar(255)') AS attributeName, attr.value('.', 'varchar(255)') AS attributeValue FROM XMLDocument CROSS APPLY XDocument.nodes('{xQuery}/@*') AS Node(attr) WHERE Id = @id", connection);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = cmd.ExecuteReader();
                Dictionary<string, string> docs = new();
                while (reader.Read())
                    docs.Add(reader["attributeName"].ToString()!, reader["attributeValue"].ToString()!);

                if (docs.Count == 0) return null;
                reader.Close();
                connection.Close();
                return docs;
            }
            catch (Exception) { return null; }
        }

        public List<Dictionary<string, string>>? GetStructuredNodes(int id, string xQuery, string[] values)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            StringBuilder commandString = new StringBuilder("SELECT ");
            for (int i = 0; i < values.Length - 1; ++i)
            {
                commandString.Append($"c.value('({values[i]})[1]', 'varchar(255)') AS {values[i]},");
            }
            commandString.Append($"c.value('({values.Last()})[1]', 'varchar(255)') AS {values.Last()} FROM XMLDocument CROSS APPLY XDocument.nodes('{xQuery}') AS XMLData(c) WHERE Id = @Id;");

            Console.WriteLine(commandString.ToString());

            SqlCommand cmd = new SqlCommand(commandString.ToString(), connection);
            cmd.Parameters.Add(new SqlParameter("@id", id));
            SqlDataReader reader = cmd.ExecuteReader();
            List<Dictionary<string, string>> docs = new();
            while (reader.Read())
            {
                Dictionary<string, string> row = new Dictionary<string, string>();
                foreach (string value in values)
                {
                    row.Add(value, reader[value].ToString()!);
                }
                docs.Add(row);
            }
            if (docs.Count == 0) throw new Exception("There is no nodes that meet the given criteria");
            reader.Close();
            connection.Close();
            return docs;
        }

        //
        // TODO: Get all attributes from node: distinct-values(//element/@parametrName/name())
        //

        public string GetValueOfAttribute(int id, string xQuery, string nameOfAttribute)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlCommand cmd = new SqlCommand($"SELECT XDocument.value('({xQuery}/@{nameOfAttribute})[1]', 'varchar(255)') AS value FROM XMLDocument WHERE Id = @id", connection);
            cmd.Parameters.Add(new SqlParameter("@id", id));
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            string result = reader["value"].ToString()!;
            connection.Close();
            if (result == "") throw new Exception("There is no attribute with this name in node");
            return result;
        }

        public bool AddNewNode(int id, string xQuery, string newNodeString)
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

        public bool DeleteNodeFromDocument(int id, string xQuery)
        {
            int howMany = execNonQuery($"UPDATE XMLDocument SET XDocument.modify('delete {xQuery}') WHERE Id = @id;", new List<(string, string)> { ("@id", id.ToString()) });
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
                if (parameters is not null)
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