using DBPCLabs.Models;
using Npgsql;

namespace DBPCLabs.Repositories
{
    public class ReservationRepository:BaseRepository
    {

        public ReservationRepository(IConfiguration configuration) : base(configuration)
        {
            
        }
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
            
            int? targetLabId = res.LaboratoryId;
            if (!res.IsGroupReservation && res.ComputerId.HasValue)
            {
                var getLabSql = "SELECT \"LaboratoryId\" FROM \"Computers\" WHERE \"Id\" = @CompId";
                await using var getLabCmd = new NpgsqlCommand(getLabSql, connection);
                getLabCmd.Parameters.AddWithValue("CompId", res.ComputerId.Value);
                var result = await getLabCmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    targetLabId = Convert.ToInt32(result);
            }
            
            if (!res.IsGroupReservation && res.StudentId.HasValue)
            {
                var stOverlapSql = @"
                    SELECT COUNT(*) FROM ""Reservations"" 
                    WHERE ""StudentId"" = @StudentId 
                      AND ""StartTime"" < @EndTime AND ""EndTime"" > @StartTime 
                      AND ""Id"" != @ResId";
                await using var cmd = new NpgsqlCommand(stOverlapSql, connection);
                cmd.Parameters.AddWithValue("StudentId", res.StudentId.Value);
                cmd.Parameters.AddWithValue("StartTime", res.StartTime);
                cmd.Parameters.AddWithValue("EndTime", res.EndTime);
                cmd.Parameters.AddWithValue("ResId", res.Id);
                if ((long)await cmd.ExecuteScalarAsync()! > 0) return "Цей студент вже має бронювання на цей час!";
            }
            
            if (res.IsGroupReservation && res.GroupId.HasValue)
            {
                var grOverlapSql = @"
                    SELECT COUNT(*) FROM ""Reservations"" 
                    WHERE ""GroupId"" = @GroupId 
                      AND ""StartTime"" < @EndTime AND ""EndTime"" > @StartTime 
                      AND ""Id"" != @ResId";
                await using var cmd = new NpgsqlCommand(grOverlapSql, connection);
                cmd.Parameters.AddWithValue("GroupId", res.GroupId.Value);
                cmd.Parameters.AddWithValue("StartTime", res.StartTime);
                cmd.Parameters.AddWithValue("EndTime", res.EndTime);
                cmd.Parameters.AddWithValue("ResId", res.Id);
                if ((long)await cmd.ExecuteScalarAsync()! > 0) return "Ця група вже має заняття на цей час!";
            }
            
            if (res.TeacherId.HasValue && targetLabId.HasValue)
            {
                var teacherOverlapSql = @"
                    SELECT COUNT(*) FROM ""Reservations"" r
                    LEFT JOIN ""Computers"" c ON r.""ComputerId"" = c.""Id""
                    WHERE r.""TeacherId"" = @TeacherId
                      AND r.""StartTime"" < @EndTime AND r.""EndTime"" > @StartTime
                      AND r.""Id"" != @ResId
                      AND (
                           (r.""IsGroupReservation"" = true AND r.""LaboratoryId"" != @TargetLabId)
                           OR
                           (r.""IsGroupReservation"" = false AND c.""LaboratoryId"" != @TargetLabId)
                      )";
                await using var cmd = new NpgsqlCommand(teacherOverlapSql, connection);
                cmd.Parameters.AddWithValue("TeacherId", res.TeacherId.Value);
                cmd.Parameters.AddWithValue("StartTime", res.StartTime);
                cmd.Parameters.AddWithValue("EndTime", res.EndTime);
                cmd.Parameters.AddWithValue("TargetLabId", targetLabId.Value);
                cmd.Parameters.AddWithValue("ResId", res.Id);
                if ((long)await cmd.ExecuteScalarAsync()! > 0) return "Цей викладач вже веде пару в ІНШІЙ аудиторії у цей час!";
            }
            
            if (!res.IsGroupReservation && res.ComputerId.HasValue)
            {
                var pcOverlapSql = @"
                    SELECT COUNT(*) FROM ""Reservations"" 
                    WHERE ""ComputerId"" = @CompId 
                      AND ""StartTime"" < @EndTime AND ""EndTime"" > @StartTime 
                      AND ""Id"" != @ResId";
                await using var cmd = new NpgsqlCommand(pcOverlapSql, connection);
                cmd.Parameters.AddWithValue("CompId", res.ComputerId.Value);
                cmd.Parameters.AddWithValue("StartTime", res.StartTime);
                cmd.Parameters.AddWithValue("EndTime", res.EndTime);
                cmd.Parameters.AddWithValue("ResId", res.Id);
                if ((long)await cmd.ExecuteScalarAsync()! > 0) return "Цей конкретний комп'ютер вже заброньовано!";
            }
            
            if (targetLabId.HasValue)
            {
                var pcCountSql = "SELECT COUNT(*) FROM \"Computers\" WHERE \"LaboratoryId\" = @LabId";
                await using var pcCmd = new NpgsqlCommand(pcCountSql, connection);
                pcCmd.Parameters.AddWithValue("LabId", targetLabId.Value);
                long totalPcs = (long)await pcCmd.ExecuteScalarAsync()!;
                
                var existingDemandSql = @"
                    SELECT COALESCE(SUM(
                        CASE
                            WHEN r.""IsGroupReservation"" = false THEN 1
                            WHEN r.""IsGroupReservation"" = true THEN (SELECT COUNT(*) FROM ""Students"" s WHERE s.""GroupId"" = r.""GroupId"")
                            ELSE 0
                        END
                    ), 0)
                    FROM ""Reservations"" r
                    WHERE r.""StartTime"" < @EndTime AND r.""EndTime"" > @StartTime
                      AND r.""Id"" != @ResId
                      AND (r.""LaboratoryId"" = @LabId OR r.""ComputerId"" IN (SELECT ""Id"" FROM ""Computers"" WHERE ""LaboratoryId"" = @LabId))";
                
                await using var demandCmd = new NpgsqlCommand(existingDemandSql, connection);
                demandCmd.Parameters.AddWithValue("LabId", targetLabId.Value);
                demandCmd.Parameters.AddWithValue("StartTime", res.StartTime);
                demandCmd.Parameters.AddWithValue("EndTime", res.EndTime);
                demandCmd.Parameters.AddWithValue("ResId", res.Id);
                long existingDemand = Convert.ToInt64(await demandCmd.ExecuteScalarAsync());
                
                long newDemand = 1; 
                if (res.IsGroupReservation && res.GroupId.HasValue)
                {
                    var stCountSql = "SELECT COUNT(*) FROM \"Students\" WHERE \"GroupId\" = @GroupId";
                    await using var stCmd = new NpgsqlCommand(stCountSql, connection);
                    stCmd.Parameters.AddWithValue("GroupId", res.GroupId.Value);
                    newDemand = (long)await stCmd.ExecuteScalarAsync()!;
                }
                
                if (existingDemand + newDemand > totalPcs)
                {
                    return $"Недостатньо місць! В аудиторії {totalPcs} ПК. Вже зайнято: {existingDemand}. Ви намагаєтесь посадити ще: {newDemand}.";
                }
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
        public async Task UpdateAsync(Reservation res)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var sql = @"
        UPDATE ""Reservations"" SET 
            ""IsGroupReservation"" = @IsGroup,
            ""ComputerId"" = @CompId,
            ""StudentId"" = @StudentId,
            ""LaboratoryId"" = @LabId,
            ""GroupId"" = @GroupId,
            ""TeacherId"" = @TeacherId,
            ""StartTime"" = @StartTime,
            ""EndTime"" = @EndTime,
            ""Purpose"" = @Purpose
        WHERE ""Id"" = @Id";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("Id", res.Id);
            command.Parameters.AddWithValue("IsGroup", res.IsGroupReservation);
            command.Parameters.AddWithValue("CompId", (object?)res.ComputerId ?? DBNull.Value);
            command.Parameters.AddWithValue("StudentId", (object?)res.StudentId ?? DBNull.Value);
            command.Parameters.AddWithValue("LabId", (object?)res.LaboratoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("GroupId", (object?)res.GroupId ?? DBNull.Value);
            command.Parameters.AddWithValue("TeacherId", (object?)res.TeacherId ?? DBNull.Value);
            command.Parameters.AddWithValue("StartTime", res.StartTime);
            command.Parameters.AddWithValue("EndTime", res.EndTime);
            command.Parameters.AddWithValue("Purpose", (object?)res.Purpose ?? DBNull.Value);

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