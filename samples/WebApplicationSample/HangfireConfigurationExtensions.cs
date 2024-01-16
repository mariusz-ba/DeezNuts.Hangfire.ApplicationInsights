using Microsoft.Data.SqlClient;

namespace WebApplicationSample;

public static class HangfireConfigurationExtensions
{
    public static string GetHangfireConnectionString(this IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") ??
                               throw new ArgumentException("Database connection string is not provided");
        
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.InitialCatalog;
        connectionStringBuilder.InitialCatalog = "master";

        using var connection = new SqlConnection(connectionStringBuilder.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
             BEGIN
                 CREATE DATABASE [{databaseName}]
             END
             """;

        command.ExecuteNonQuery();
        return connectionString;
    }
}