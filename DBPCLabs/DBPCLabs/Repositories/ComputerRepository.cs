using Npgsql;
using DBPCLabs.Models;

namespace DBPCLabs.Repositories;

public class ComputerRepository : BaseRepository
{
    public ComputerRepository(IConfiguration configuration) : base(configuration) { }
    
    public async Task<List<Computer>> GetAllAsync()
    {
        var computers = new List<Computer>();
        
        var compDict = new Dictionary<int, Computer>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        var sqlPCs = @"
            SELECT c.""Id"", c.""InventoryNumber"", c.""Cpu"", c.""RamGb"", c.""LaboratoryId"",
                   l.""Name"" AS ""LabName"", l.""RoomNumber"" AS ""LabRoom""
            FROM ""Computers"" c
            LEFT JOIN ""Laboratories"" l ON c.""LaboratoryId"" = l.""Id""
            ORDER BY c.""Id"" DESC";

        await using var commandPCs = new NpgsqlCommand(sqlPCs, connection);
        await using var readerPCs = await commandPCs.ExecuteReaderAsync();

        while (await readerPCs.ReadAsync())
        {
            bool hasLab = !readerPCs.IsDBNull(readerPCs.GetOrdinal("LaboratoryId"));

            var pc = new Computer
            {
                Id = readerPCs.GetInt32(readerPCs.GetOrdinal("Id")),
                InventoryNumber = readerPCs.GetString(readerPCs.GetOrdinal("InventoryNumber")),
                Cpu = readerPCs.GetString(readerPCs.GetOrdinal("Cpu")),
                RamGb = readerPCs.GetInt32(readerPCs.GetOrdinal("RamGb")),
                LaboratoryId = hasLab ? readerPCs.GetInt32(readerPCs.GetOrdinal("LaboratoryId")) : 0,
                
                InstalledSoftware = new List<Software>() 
            };

            if (hasLab && !readerPCs.IsDBNull(readerPCs.GetOrdinal("LabName")))
            {
                pc.Laboratory = new Laboratory
                {
                    Id = pc.LaboratoryId,
                    Name = readerPCs.GetString(readerPCs.GetOrdinal("LabName")),
                    RoomNumber = readerPCs.GetString(readerPCs.GetOrdinal("LabRoom"))
                };
            }

            computers.Add(pc);
            compDict.Add(pc.Id, pc); 
        }
        await readerPCs.CloseAsync(); 
        
        var sqlSoft = @"
            SELECT cs.""ComputersId"", s.""Id"" AS ""SoftwareId"", s.""Name""
            FROM ""ComputerSoftware"" cs
            JOIN ""Softwares"" s ON cs.""InstalledSoftwareId"" = s.""Id""";

        await using var commandSoft = new NpgsqlCommand(sqlSoft, connection);
        await using var readerSoft = await commandSoft.ExecuteReaderAsync();

        while (await readerSoft.ReadAsync())
        {
            int compId = readerSoft.GetInt32(readerSoft.GetOrdinal("ComputersId"));
            
            if (compDict.TryGetValue(compId, out var pc))
            {
                pc.InstalledSoftware.Add(new Software
                {
                    Id = readerSoft.GetInt32(readerSoft.GetOrdinal("SoftwareId")),
                    Name = readerSoft.GetString(readerSoft.GetOrdinal("Name"))
                });
            }
        }

        return computers;
    }
    
    public async Task DeleteAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "DELETE FROM \"Computers\" WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);
        
        await command.ExecuteNonQueryAsync();
    }
 
    public async Task<Computer?> GetByIdAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT \"Id\", \"InventoryNumber\", \"Cpu\", \"RamGb\", \"LaboratoryId\" FROM \"Computers\" WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Computer
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                InventoryNumber = reader.GetString(reader.GetOrdinal("InventoryNumber")),
                Cpu = reader.GetString(reader.GetOrdinal("Cpu")),
                RamGb = reader.GetInt32(reader.GetOrdinal("RamGb")),
                
                LaboratoryId = !reader.IsDBNull(reader.GetOrdinal("LaboratoryId")) ? reader.GetInt32(reader.GetOrdinal("LaboratoryId")) : 0
            };
        }

        return null;
    }
    
    public async Task AddAsync(Computer pc)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "INSERT INTO \"Computers\" (\"InventoryNumber\", \"Cpu\", \"RamGb\", \"LaboratoryId\") VALUES (@InventoryNumber, @Cpu, @RamGb, @LaboratoryId)";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("InventoryNumber", pc.InventoryNumber);
        command.Parameters.AddWithValue("Cpu", pc.Cpu);
        command.Parameters.AddWithValue("RamGb", pc.RamGb);
        
        if (pc.LaboratoryId == 0) 
            command.Parameters.AddWithValue("LaboratoryId", DBNull.Value);
        else 
            command.Parameters.AddWithValue("LaboratoryId", pc.LaboratoryId);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task UpdateAsync(Computer pc)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "UPDATE \"Computers\" SET \"InventoryNumber\" = @InventoryNumber, \"Cpu\" = @Cpu, \"RamGb\" = @RamGb, \"LaboratoryId\" = @LaboratoryId WHERE \"Id\" = @Id";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("InventoryNumber", pc.InventoryNumber);
        command.Parameters.AddWithValue("Cpu", pc.Cpu);
        command.Parameters.AddWithValue("RamGb", pc.RamGb);
        command.Parameters.AddWithValue("Id", pc.Id);

        if (pc.LaboratoryId == 0) 
            command.Parameters.AddWithValue("LaboratoryId", DBNull.Value);
        else 
            command.Parameters.AddWithValue("LaboratoryId", pc.LaboratoryId);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<int> GetMaxIdAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        var sql = "SELECT MAX(\"Id\") FROM \"Computers\"";
        
        await using var command = new NpgsqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();
        
        return result != DBNull.Value ? Convert.ToInt32(result) : 0;
    }

    public async Task<int> CountComputersInLabAsync(int labId)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT COUNT(*) FROM \"Computers\" WHERE \"LaboratoryId\" = @LabId";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("LabId", labId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
  
    public async Task<List<int>> GetInstalledSoftwareIdsAsync(int computerId)
    {
        var softwareIds = new List<int>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        var sql = "SELECT \"InstalledSoftwareId\" FROM \"ComputerSoftware\" WHERE \"ComputersId\" = @ComputerId";
        
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("ComputerId", computerId);

        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            softwareIds.Add(reader.GetInt32(0));
        }

        return softwareIds;
    }
    
    public async Task UpdateInstalledSoftwareAsync(int computerId, HashSet<int> softwareIds)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var deleteSql = "DELETE FROM \"ComputerSoftware\" WHERE \"ComputersId\" = @ComputerId";
            await using var deleteCommand = new NpgsqlCommand(deleteSql, connection, transaction);
            deleteCommand.Parameters.AddWithValue("ComputerId", computerId);
            await deleteCommand.ExecuteNonQueryAsync();
            
            if (softwareIds.Any())
            {
                var insertSql = "INSERT INTO \"ComputerSoftware\" (\"ComputersId\", \"InstalledSoftwareId\") VALUES (@ComputerId, @SoftwareId)";
                
                foreach (var softId in softwareIds)
                {
                    await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction);
                    insertCommand.Parameters.AddWithValue("ComputerId", computerId);
                    insertCommand.Parameters.AddWithValue("SoftwareId", softId);
                    await insertCommand.ExecuteNonQueryAsync();
                }
            }
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}