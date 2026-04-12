using DBPCLabs.Models;
using Npgsql;

namespace DBPCLabs.Repositories
{
    public class DepartmentRepository:BaseRepository
    {
        public DepartmentRepository(IConfiguration configuration) : base(configuration)
        {
            
        }
        public async Task<List<Department>> GetAllAsync()
        {
            var departments = new List<Department>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "SELECT \"Id\", \"Name\", \"Faculty\" FROM \"Departments\" ORDER BY \"Faculty\", \"Name\"";
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                departments.Add(new Department
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Faculty = reader.GetString(2)
                });
            }
            return departments;
        }

        public async Task AddAsync(Department department)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "INSERT INTO \"Departments\" (\"Name\", \"Faculty\") VALUES (@Name, @Faculty)";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Name", department.Name);
            command.Parameters.AddWithValue("Faculty", department.Faculty);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "UPDATE \"Departments\" SET \"Name\" = @Name, \"Faculty\" = @Faculty WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", department.Id);
            command.Parameters.AddWithValue("Name", department.Name);
            command.Parameters.AddWithValue("Faculty", department.Faculty);
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "DELETE FROM \"Departments\" WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
}