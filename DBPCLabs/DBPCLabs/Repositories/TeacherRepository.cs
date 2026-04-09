using DBPCLabs.Models;
using Npgsql;

namespace DBPCLabs.Repositories
{
    public class TeacherRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TeacherRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        }

        private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);
        
        public async Task<List<Teacher>> GetAllAsync()
        {
            var teachers = new List<Teacher>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = @"
                SELECT t.""Id"", t.""FullName"", t.""Email"", t.""DepartmentId"", d.""Name"" AS ""DepartmentName""
                FROM ""Teachers"" t
                JOIN ""Departments"" d ON t.""DepartmentId"" = d.""Id""
                ORDER BY t.""FullName""";
                
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                teachers.Add(new Teacher
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DepartmentId = reader.GetInt32(3),
                    DepartmentName = reader.GetString(4) 
                });
            }
            return teachers;
        }
        
        public async Task AddAsync(Teacher teacher)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = "INSERT INTO \"Teachers\" (\"FullName\", \"DepartmentId\", \"Email\") VALUES (@FullName, @DepartmentId, @Email)";
            
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("FullName", teacher.FullName);
            command.Parameters.AddWithValue("DepartmentId", teacher.DepartmentId);
            command.Parameters.AddWithValue("Email", teacher.Email ?? (object)DBNull.Value);
            
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task UpdateAsync(Teacher teacher)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = "UPDATE \"Teachers\" SET \"FullName\" = @FullName, \"DepartmentId\" = @DepartmentId, \"Email\" = @Email WHERE \"Id\" = @Id";
            
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", teacher.Id);
            command.Parameters.AddWithValue("FullName", teacher.FullName);
            command.Parameters.AddWithValue("DepartmentId", teacher.DepartmentId);
            command.Parameters.AddWithValue("Email", teacher.Email ?? (object)DBNull.Value);
            
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task DeleteAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "DELETE FROM \"Teachers\" WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task<bool> IsDuplicateEmailAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "SELECT COUNT(*) FROM \"Teachers\" WHERE LOWER(\"Email\") = LOWER(@Email)";
            
            if (excludeId.HasValue) sql += " AND \"Id\" != @ExcludeId";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Email", email.Trim());
            if (excludeId.HasValue) command.Parameters.AddWithValue("ExcludeId", excludeId.Value);

            var count = (long)await command.ExecuteScalarAsync()!;
            return count > 0;
        }
    }
}