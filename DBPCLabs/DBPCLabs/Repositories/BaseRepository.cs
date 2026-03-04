using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DBPCLabs.Repositories;

public abstract class BaseRepository
{
    private readonly string _connectionString;
    
    protected BaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("Рядок підключення до БД не знайдено!");
    }
    
    protected NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}