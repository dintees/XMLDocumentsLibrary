using System.Data.SqlClient;

namespace XMLDocumentLibrary
{
    public class XMLService
    {
        private SqlConnection _connection;
        public bool Connect(string connectionString)
        {
            try
            {
                _connection = new SqlConnection(connectionString);
                _connection.Open();
                return true;
            } catch (Exception e)
            {
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                _connection.Close();
                return true;
            } catch (Exception e) { return false; }
        }

        public int CountDocuments()
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) AS howMany FROM XMLDocument", _connection);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                reader.Read();
                int howMany = (int)reader["howMany"];
                reader.Close();
                return howMany;
            } catch (Exception e) { return -1; }
        }
    }
}