using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Net.NetworkInformation;

namespace dw_backend.DBConnections.SqlServer
{
    public class MainConnection
    {
        public static SqlConnection Connection(IConfiguration configuration)
        {
            SqlConnection conn = new SqlConnection(configuration.GetConnectionString("MainConnection"));
            conn.Open();
            return conn;
        }
    }
}
