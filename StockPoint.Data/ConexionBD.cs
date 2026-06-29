using Microsoft.Data.SqlClient;

namespace StockPoint.Data
{
    public class ConexionBD
    {
        private readonly string _connectionString;

        public ConexionBD(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_connectionString);
        }
    }
}