using Microsoft.Data.SqlClient;
using StockPoint.Domain;
using StockPoint.Data.Interfaces;

namespace StockPoint.Data
{
    public class ClienteData : IClienteData
    {
        private readonly ConexionBD _conexion;

        public ClienteData(ConexionBD conexion)
        {
            _conexion = conexion;
        }

        public List<Cliente> ObtenerTodos()
        {
            var lista = new List<Cliente>();
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id, nombre, apellidos, numero_identificacion, email, direccion, activo
                                  FROM Cliente WHERE activo = 1";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearCliente(reader));
            }
            return lista;
        }

        public Cliente ObtenerPorId(int id)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id, nombre, apellidos, numero_identificacion, email, direccion, activo
                                  FROM Cliente WHERE id = @id";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearCliente(reader) : null;
        }

        private Cliente MapearCliente(SqlDataReader reader)
        {
            return new Cliente
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellidos = reader.GetString(2),
                NumeroIdentificacion = reader.GetString(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                Direccion = reader.IsDBNull(5) ? null : reader.GetString(5),
                Activo = reader.GetBoolean(6)
            };
        }
    }
}
