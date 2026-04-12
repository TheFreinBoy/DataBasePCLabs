using DBPCLabs.Models;
using Npgsql;

namespace DBPCLabs.Repositories
{
    public class ComputerSoftwareRepository : BaseRepository 
    {
        public ComputerSoftwareRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<ComputerSoftware>> GetAllAsync()
        {
            var installations = new List<ComputerSoftware>();
            await using var connection = CreateConnection(); 
            await connection.OpenAsync();
            
            var sql = @"
                SELECT cs.""Id"", 
                       cs.""ComputerId"", c.""InventoryNumber"", 
                       cs.""SoftwareId"", s.""Name"" AS ""SoftwareName"", s.""Version"" AS ""SoftwareVersion"",
                       cs.""InstallationDate""
                FROM ""ComputerSoftware"" cs
                JOIN ""Computers"" c ON cs.""ComputerId"" = c.""Id""
                JOIN ""Softwares"" s ON cs.""SoftwareId"" = s.""Id""
                ORDER BY cs.""InstallationDate"" DESC";
                
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                installations.Add(new ComputerSoftware
                {
                    Id = reader.GetInt32(0),
                    ComputerId = reader.GetInt32(1),
                    ComputerInventoryNumber = reader.GetString(2),
                    SoftwareId = reader.GetInt32(3),
                    SoftwareName = reader.GetString(4),
                    SoftwareVersion = reader.GetString(5),
                    InstallationDate = reader.GetDateTime(6)
                });
            }
            return installations;
        }

        public async Task<bool> IsAlreadyInstalledAsync(int computerId, int softwareId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "SELECT COUNT(*) FROM \"ComputerSoftware\" WHERE \"ComputerId\" = @CompId AND \"SoftwareId\" = @SoftId";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("CompId", computerId);
            command.Parameters.AddWithValue("SoftId", softwareId);
            return (long)await command.ExecuteScalarAsync()! > 0;
        }

        public async Task AddAsync(ComputerSoftware cs)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "INSERT INTO \"ComputerSoftware\" (\"ComputerId\", \"SoftwareId\", \"InstallationDate\") VALUES (@CompId, @SoftId, @InstDate)";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("CompId", cs.ComputerId);
            command.Parameters.AddWithValue("SoftId", cs.SoftwareId);
            command.Parameters.AddWithValue("InstDate", cs.InstallationDate);
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "DELETE FROM \"ComputerSoftware\" WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
}