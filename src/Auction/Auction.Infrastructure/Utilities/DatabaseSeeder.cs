using Npgsql;

namespace Auction.Infrastructure.Utilities;

public static class DatabaseSeederUtility
{
    public static async Task SeedFromSqlFileAsync(string connectionString, string sqlFilePath)
    {
        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException($"Arquivo SQL não encontrado: {sqlFilePath}");
        }

        var sql = await File.ReadAllTextAsync(sqlFilePath);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.CommandTimeout = 300; // 5 minutes

        await command.ExecuteNonQueryAsync();
    }
}
