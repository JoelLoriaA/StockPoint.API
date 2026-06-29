using Microsoft.Data.SqlClient;
using StockPoint.Domain;
using StockPoint.Data.Interfaces;

namespace StockPoint.Data
{
    public class EmpleadoData : IEmpleadoData
    {
        private readonly ConexionBD _conexion;

        public EmpleadoData(ConexionBD conexion)
        {
            _conexion = conexion;
        }

        public List<Empleado> ObtenerTodos()
        {
            var lista = new List<Empleado>();
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id, nombre, apellidos, numero_identificacion, email, activo
                                  FROM Empleado WHERE activo = 1";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearEmpleado(reader));
            }
            return lista;
        }

        public Empleado ObtenerPorId(int id)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id, nombre, apellidos, numero_identificacion, email, activo
                                  FROM Empleado WHERE id = @id";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearEmpleado(reader) : null;
        }

        private Empleado MapearEmpleado(SqlDataReader reader)
        {
            return new Empleado
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellidos = reader.GetString(2),
                NumeroIdentificacion = reader.GetString(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                Activo = reader.GetBoolean(5)
            };
        }
    }
}
