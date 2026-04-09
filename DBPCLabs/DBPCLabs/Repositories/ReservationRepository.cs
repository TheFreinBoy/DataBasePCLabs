using DBPCLabs.Models;
using Npgsql;

namespace DBPCLabs.Repositories
{
    public class ReservationRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ReservationRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        }

        private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<List<Reservation>> GetAllAsync()
        {
            var reservations = new List<Reservation>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = @"
                SELECT r.""Id"", r.""IsGroupReservation"", 
                       r.""ComputerId"", c.""InventoryNumber"", 
                       r.""StudentId"", s.""FullName"" AS ""StudentName"",
                       r.""LaboratoryId"", l.""Name"" AS ""LabName"",
                       r.""GroupId"", g.""Name"" AS ""GroupName"",
                       r.""TeacherId"", t.""FullName"" AS ""TeacherName"",
                       r.""StartTime"", r.""EndTime"", r.""Purpose""
                FROM ""Reservations"" r
                LEFT JOIN ""Computers"" c ON r.""ComputerId"" = c.""Id""
                LEFT JOIN ""Students"" s ON r.""StudentId"" = s.""Id""
                LEFT JOIN ""Laboratories"" l ON r.""LaboratoryId"" = l.""Id""
                LEFT JOIN ""Groups"" g ON r.""GroupId"" = g.""Id""
                LEFT JOIN ""Teachers"" t ON r.""TeacherId"" = t.""Id""
                ORDER BY r.""StartTime"" DESC";
                
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reservations.Add(new Reservation
                {
                    Id = reader.GetInt32(0),
                    IsGroupReservation = reader.GetBoolean(1),
                    ComputerId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    ComputerName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    StudentId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    StudentName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    LaboratoryId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    LaboratoryName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    GroupId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                    GroupName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    TeacherId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                    TeacherName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    StartTime = reader.GetDateTime(12),
                    EndTime = reader.GetDateTime(13),
                    Purpose = reader.IsDBNull(14) ? string.Empty : reader.GetString(14)
                });
            }
            return reservations;
        }
        
        public async Task<string?> ValidateReservationAsync(Reservation res)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            if (res.IsGroupReservation && res.LaboratoryId.HasValue && res.GroupId.HasValue)
            {
                var pcCountSql = "SELECT COUNT(*) FROM \"Computers\" WHERE \"LaboratoryId\" = @LabId";
                await using var pcCmd = new NpgsqlCommand(pcCountSql, connection);
                pcCmd.Parameters.AddWithValue("LabId", res.LaboratoryId.Value);
                long pcCount = (long)await pcCmd.ExecuteScalarAsync()!;

                var stCountSql = "SELECT COUNT(*) FROM \"Students\" WHERE \"GroupId\" = @GroupId";
                await using var stCmd = new NpgsqlCommand(stCountSql, connection);
                stCmd.Parameters.AddWithValue("GroupId", res.GroupId.Value);
                long stCount = (long)await stCmd.ExecuteScalarAsync()!;

                if (stCount > pcCount)
                {
                    return $"Недостатньо місць! У групі {stCount} студентів, а в лабораторії лише {pcCount} комп'ютерів.";
                }
                
                var overlapSql = @"
                    SELECT COUNT(*) FROM ""Reservations"" 
                    WHERE ""StartTime"" < @EndTime AND ""EndTime"" > @StartTime
                    AND (""LaboratoryId"" = @LabId OR ""ComputerId"" IN (SELECT ""Id"" FROM ""Computers"" WHERE ""LaboratoryId"" = @LabId))";
                
                await using var overlapCmd = new NpgsqlCommand(overlapSql, connection);
                overlapCmd.Parameters.AddWithValue("LabId", res.LaboratoryId.Value);
                overlapCmd.Parameters.AddWithValue("StartTime", res.StartTime);
                overlapCmd.Parameters.AddWithValue("EndTime", res.EndTime);
                if ((long)await overlapCmd.ExecuteScalarAsync()! > 0)
                    return "Ця лабораторія (або деякі комп'ютери в ній) вже заброньовані на цей час!";
            }
            else if (!res.IsGroupReservation && res.ComputerId.HasValue)
            {
                var overlapSql = @"
                    SELECT COUNT(*) FROM ""Reservations"" 
                    WHERE ""StartTime"" < @EndTime AND ""EndTime"" > @StartTime
                    AND (""ComputerId"" = @CompId OR ""LaboratoryId"" = (SELECT ""LaboratoryId"" FROM ""Computers"" WHERE ""Id"" = @CompId))";
                
                await using var overlapCmd = new NpgsqlCommand(overlapSql, connection);
                overlapCmd.Parameters.AddWithValue("CompId", res.ComputerId.Value);
                overlapCmd.Parameters.AddWithValue("StartTime", res.StartTime);
                overlapCmd.Parameters.AddWithValue("EndTime", res.EndTime);
                if ((long)await overlapCmd.ExecuteScalarAsync()! > 0)
                    return "Цей комп'ютер (або вся лабораторія) вже заброньовані на цей час!";
            }

            return null; 
        }

        public async Task AddAsync(Reservation res)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = @"
                INSERT INTO ""Reservations"" 
                (""IsGroupReservation"", ""ComputerId"", ""StudentId"", ""LaboratoryId"", ""GroupId"", ""TeacherId"", ""StartTime"", ""EndTime"", ""Purpose"") 
                VALUES (@IsGroup, @CompId, @StudId, @LabId, @GroupId, @TeacherId, @Start, @End, @Purpose)";
                
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("IsGroup", res.IsGroupReservation);
            command.Parameters.AddWithValue("CompId", res.ComputerId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("StudId", res.StudentId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("LabId", res.LaboratoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("GroupId", res.GroupId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("TeacherId", res.TeacherId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("Start", res.StartTime);
            command.Parameters.AddWithValue("End", res.EndTime);
            command.Parameters.AddWithValue("Purpose", res.Purpose ?? (object)DBNull.Value);
            
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = "DELETE FROM \"Reservations\" WHERE \"Id\" = @Id";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
}