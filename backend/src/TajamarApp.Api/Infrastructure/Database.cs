using Microsoft.Data.SqlClient;

sealed class Database(IConfiguration configuration)
{
    public async Task<T> Read<T>(Func<SqlConnection, Task<T>> work, CancellationToken ct)
    {
        var connectionString = configuration.GetConnectionString("Tajamar")
            ?? throw new InvalidOperationException("Configura ConnectionStrings:Tajamar.");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        return await work(connection);
    }

    public Task<T> Write<T>(Func<SqlConnection, SqlTransaction, Task<T>> work, CancellationToken ct) =>
        Read(async connection =>
        {
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(ct);
            var result = await work(connection, transaction);
            await transaction.CommitAsync(ct);
            return result;
        }, ct);

    public static SqlCommand Command(SqlConnection connection, SqlTransaction? transaction, string sql,
        params (string Key, object? Value)[] parameters)
    {
        var command = new SqlCommand(sql, connection, transaction);
        foreach (var (key, value) in parameters)
        {
            var parameter = command.Parameters.AddWithValue(key, value ?? DBNull.Value);
            if (value is DateTimeOffset)
                parameter.SqlDbType = System.Data.SqlDbType.DateTimeOffset;
        }
        return command;
    }
}

static class ApiError
{
    public static IResult Result(int status, string code, string title) =>
        Results.Problem(statusCode: status, title: title, extensions: new Dictionary<string, object?>
        {
            ["code"] = code
        });
}
