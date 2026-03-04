
using Npgsql;
using DBPCLabs.Models;

namespace DBPCLabs.Repositories;

public class LaboratoryRepository : BaseRepository
{
    public LaboratoryRepository(IConfiguration configuration) : base(configuration) { }
    
    public async Task<List<Laboratory>> GetAllAsync()
    {
        var laboratories = new List<Laboratory>();
        
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        var sql = "SELECT \"Id\", \"Name\", \"RoomNumber\", \"Capacity\" FROM \"Laboratories\" ORDER BY \"Id\"";
        
        await using var command = new NpgsqlCommand(sql, connection);
        
        await  using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            laboratories.Add(new Laboratory
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                RoomNumber = reader.GetString(reader.GetOrdinal("RoomNumber")),
                Capacity = reader.GetInt32(reader.GetOrdinal("Capacity"))
            });
        }

        return laboratories;
    }
    public async Task DeleteAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        var sql = "DELETE FROM \"Laboratories\" WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);
        
        await command.ExecuteNonQueryAsync();
    }
    public async Task<Laboratory?> GetByIdAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT \"Id\", \"Name\", \"RoomNumber\", \"Capacity\" FROM \"Laboratories\" WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Laboratory
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                RoomNumber = reader.GetString(reader.GetOrdinal("RoomNumber")),
                Capacity = reader.GetInt32(reader.GetOrdinal("Capacity"))
            };
        }

        return null;
    }
    
    public async Task AddAsync(Laboratory lab)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        var sql = "INSERT INTO \"Laboratories\" (\"Name\", \"RoomNumber\", \"Capacity\") VALUES (@Name, @RoomNumber, @Capacity)";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Name", lab.Name);
        command.Parameters.AddWithValue("RoomNumber", lab.RoomNumber);
        command.Parameters.AddWithValue("Capacity", lab.Capacity);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task UpdateAsync(Laboratory lab)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "UPDATE \"Laboratories\" SET \"Name\" = @Name, \"RoomNumber\" = @RoomNumber, \"Capacity\" = @Capacity WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Name", lab.Name);
        command.Parameters.AddWithValue("RoomNumber", lab.RoomNumber);
        command.Parameters.AddWithValue("Capacity", lab.Capacity);
        command.Parameters.AddWithValue("Id", lab.Id);

        await command.ExecuteNonQueryAsync();
    }
    public async Task<Laboratory?> GetByIdWithComputersAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        var sql = @"
            SELECT l.""Id"" AS ""LabId"", l.""Name"", l.""RoomNumber"", l.""Capacity"",
                   c.""Id"" AS ""CompId"", c.""InventoryNumber"", c.""Cpu"", c.""RamGb""
            FROM ""Laboratories"" l
            LEFT JOIN ""Computers"" c ON l.""Id"" = c.""LaboratoryId""
            WHERE l.""Id"" = @Id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        await using var reader = await command.ExecuteReaderAsync();

        Laboratory? lab = null;

        while (await reader.ReadAsync())
        {
            if (lab == null)
            {
                lab = new Laboratory
                {
                    Id = reader.GetInt32(reader.GetOrdinal("LabId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    RoomNumber = reader.GetString(reader.GetOrdinal("RoomNumber")),
                    Capacity = reader.GetInt32(reader.GetOrdinal("Capacity")),
                    Computers = new List<Computer>() 
                };
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("CompId")))
            {
                var pc = new Computer
                {
                    Id = reader.GetInt32(reader.GetOrdinal("CompId")),
                    InventoryNumber = reader.GetString(reader.GetOrdinal("InventoryNumber")),
                    Cpu = reader.GetString(reader.GetOrdinal("Cpu")),
                    RamGb = reader.GetInt32(reader.GetOrdinal("RamGb")),
                    LaboratoryId = lab.Id
                };
                
                lab.Computers.Add(pc);
            }
        }

        return lab;
    }
}