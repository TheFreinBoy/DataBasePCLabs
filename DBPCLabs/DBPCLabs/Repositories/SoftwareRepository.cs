using Microsoft.Extensions.Configuration;
using Npgsql;
using DBPCLabs.Models;

namespace DBPCLabs.Repositories;

public class SoftwareRepository : BaseRepository
{
    public SoftwareRepository(IConfiguration configuration) : base(configuration) { }
    
    public async Task<List<Software>> GetAllAsync(string sortOrder = "id_desc")
    {
        var softwares = new List<Software>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        string orderBy = sortOrder switch
        {
            "id_asc" => "\"Id\" ASC",
            "name_asc" => "\"Name\" ASC",
            "name_desc" => "\"Name\" DESC",
            _ => "\"Id\" DESC" 
        };

        var sql = $"SELECT \"Id\", \"Name\", \"Version\", \"LicenseType\" FROM \"Softwares\" ORDER BY {orderBy}";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            softwares.Add(new Software
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Version = reader.GetString(reader.GetOrdinal("Version")),
                LicenseType = reader.GetString(reader.GetOrdinal("LicenseType"))
            });
        }

        return softwares;
    }
    
    public async Task<Software?> GetByIdAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT \"Id\", \"Name\", \"Version\", \"LicenseType\" FROM \"Softwares\" WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Software
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Version = reader.GetString(reader.GetOrdinal("Version")),
                LicenseType = reader.GetString(reader.GetOrdinal("LicenseType"))
            };
        }

        return null;
    }
    
    public async Task<bool> IsDuplicateAsync(string name, string version, int? excludeId = null)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT COUNT(*) FROM \"Softwares\" WHERE \"Name\" = @Name AND \"Version\" = @Version";
        
        if (excludeId.HasValue)
        {
            sql += " AND \"Id\" != @ExcludeId";
        }

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Name", name);
        command.Parameters.AddWithValue("Version", version);
        
        if (excludeId.HasValue) 
            command.Parameters.AddWithValue("ExcludeId", excludeId.Value);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }
    
    public async Task AddAsync(Software soft)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "INSERT INTO \"Softwares\" (\"Name\", \"Version\", \"LicenseType\") VALUES (@Name, @Version, @LicenseType)";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Name", soft.Name);
        command.Parameters.AddWithValue("Version", soft.Version);
        command.Parameters.AddWithValue("LicenseType", soft.LicenseType);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task UpdateAsync(Software soft)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "UPDATE \"Softwares\" SET \"Name\" = @Name, \"Version\" = @Version, \"LicenseType\" = @LicenseType WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Name", soft.Name);
        command.Parameters.AddWithValue("Version", soft.Version);
        command.Parameters.AddWithValue("LicenseType", soft.LicenseType);
        command.Parameters.AddWithValue("Id", soft.Id);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "DELETE FROM \"Softwares\" WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        await command.ExecuteNonQueryAsync();
    }
    public async Task<List<string>> GetUniqueNamesAsync()
    {
        var list = new List<string>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        var sql = "SELECT DISTINCT \"Name\" FROM \"Softwares\" ORDER BY \"Name\"";
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(reader.GetString(0));
        return list;
    }

    public async Task<List<string>> GetUniqueVersionsAsync()
    {
        var list = new List<string>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        var sql = "SELECT DISTINCT \"Version\" FROM \"Softwares\" ORDER BY \"Version\"";
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(reader.GetString(0));
        return list;
    }

    public async Task<List<string>> GetUniqueLicensesAsync()
    {
        var list = new List<string>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        var sql = "SELECT DISTINCT \"LicenseType\" FROM \"Softwares\" ORDER BY \"LicenseType\"";
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(reader.GetString(0));
        return list;
    }
}