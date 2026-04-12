using DBPCLabs.Models;
using Npgsql;

namespace DBPCLabs.Repositories
{
    public class GroupRepository: BaseRepository
    {
        public GroupRepository(IConfiguration configuration) : base(configuration) { }
        
        public async Task<List<Group>> GetAllAsync()
        {
            var groups = new List<Group>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = @"
                SELECT g.""Id"", g.""Name"", g.""DepartmentId"", d.""Name"" AS ""DepartmentName""
                FROM ""Groups"" g
                JOIN ""Departments"" d ON g.""DepartmentId"" = d.""Id""
                ORDER BY g.""Name""";
                
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                groups.Add(new Group
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    DepartmentId = reader.GetInt32(2),
                    DepartmentName = reader.GetString(3) 
                });
            }
            return groups;
        }
        
        public async Task<Group?> GetByIdAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = @"
                SELECT g.""Id"", g.""Name"", g.""DepartmentId"", d.""Name"" AS ""DepartmentName""
                FROM ""Groups"" g
                JOIN ""Departments"" d ON g.""DepartmentId"" = d.""Id""
                WHERE g.""Id"" = @Id";
                
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Group
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    DepartmentId = reader.GetInt32(2),
                    DepartmentName = reader.GetString(3)
                };
            }
            return null;
        }
        
        public async Task AddAsync(Group group)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = "INSERT INTO \"Groups\" (\"Name\", \"DepartmentId\") VALUES (@Name, @DepartmentId)";
            
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Name", group.Name);
            command.Parameters.AddWithValue("DepartmentId", group.DepartmentId);
            
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task UpdateAsync(Group group)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = "UPDATE \"Groups\" SET \"Name\" = @Name, \"DepartmentId\" = @DepartmentId WHERE \"Id\" = @Id";
            
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", group.Id);
            command.Parameters.AddWithValue("Name", group.Name);
            command.Parameters.AddWithValue("DepartmentId", group.DepartmentId);
            
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task DeleteAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "DELETE FROM \"Groups\" WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task<bool> IsDuplicateNameAsync(string name, int? excludeId = null)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "SELECT COUNT(*) FROM \"Groups\" WHERE \"Name\" = @Name";
            if (excludeId.HasValue) sql += " AND \"Id\" != @ExcludeId";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Name", name);
            if (excludeId.HasValue) command.Parameters.AddWithValue("ExcludeId", excludeId.Value);

            var count = (long)await command.ExecuteScalarAsync()!;
            return count > 0;
        }
    }
}