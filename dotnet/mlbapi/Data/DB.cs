using Microsoft.Data.SqlClient;
using System.Data;

namespace mlbapi.Data;

public class DB
{
    private readonly string _connectionString;

    public DB(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AzureSql")
            ?? throw new Exception("AzureSql connection string not found");
    }

    public IDbConnection CreateConnection(){
        return new SqlConnection(_connectionString);
    }
}

