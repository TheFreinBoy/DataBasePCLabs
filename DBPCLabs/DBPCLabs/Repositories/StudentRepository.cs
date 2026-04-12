using DBPCLabs.Models;
using Npgsql;

namespace DBPCLabs.Repositories
{
    public class StudentRepository:BaseRepository
    {
        public StudentRepository(IConfiguration configuration) : base(configuration)
        {
            
        }
        
        public async Task<List<Student>> GetAllAsync()
        {
            var students = new List<Student>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = @"
                SELECT s.""Id"", s.""FullName"", s.""Email"", s.""GroupId"", g.""Name"" AS ""GroupName""
                FROM ""Students"" s
                JOIN ""Groups"" g ON s.""GroupId"" = g.""Id""
                ORDER BY s.""FullName""";
                
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    GroupId = reader.GetInt32(3),
                    GroupName = reader.GetString(4) 
                });
            }
            return students;
        }
        
        public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
        {
            var students = new List<Student>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = @"
                SELECT s.""Id"", s.""FullName"", s.""Email"", s.""GroupId"", g.""Name"" AS ""GroupName""
                FROM ""Students"" s
                JOIN ""Groups"" g ON s.""GroupId"" = g.""Id""
                WHERE s.""GroupId"" = @GroupId
                ORDER BY s.""FullName""";
                
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("GroupId", groupId);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    GroupId = reader.GetInt32(3),
                    GroupName = reader.GetString(4)
                });
            }
            return students;
        }
        
        public async Task<Student?> GetByIdAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "SELECT \"Id\", \"FullName\", \"Email\", \"GroupId\" FROM \"Students\" WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Student
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    GroupId = reader.GetInt32(3)
                };
            }
            return null;
        }
        
        
        public async Task AddAsync(Student student)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "INSERT INTO \"Students\" (\"FullName\", \"Email\", \"GroupId\") VALUES (@FullName, @Email, @GroupId)";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("FullName", student.FullName);
            command.Parameters.AddWithValue("Email", student.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("GroupId", student.GroupId);
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task UpdateAsync(Student student)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "UPDATE \"Students\" SET \"FullName\" = @FullName, \"Email\" = @Email, \"GroupId\" = @GroupId WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", student.Id);
            command.Parameters.AddWithValue("FullName", student.FullName);
            command.Parameters.AddWithValue("Email", student.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("GroupId", student.GroupId);
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task DeleteAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "DELETE FROM \"Students\" WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await command.ExecuteNonQueryAsync();
        }
        public async Task<bool> IsDuplicateEmailAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = "SELECT COUNT(*) FROM \"Students\" WHERE LOWER(\"Email\") = LOWER(@Email)";
            
            if (excludeId.HasValue) 
                sql += " AND \"Id\" != @ExcludeId";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Email", email.Trim());
            
            if (excludeId.HasValue) 
                command.Parameters.AddWithValue("ExcludeId", excludeId.Value);

            var count = (long)await command.ExecuteScalarAsync()!;
            return count > 0;
        }
    }
}