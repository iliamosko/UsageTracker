using Npgsql;
using UsageTracker.Entities;

namespace UsageTracker.Data;

public sealed class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var cmd = _dataSource.CreateCommand(
            "SELECT id, firstName, lastName, email, password FROM accounts WHERE email = $1");
        cmd.Parameters.AddWithValue(email);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return new User(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4));
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        var users = new List<User>();
        await using var cmd = _dataSource.CreateCommand(
            "SELECT id, firstName, lastName, email, password FROM accounts");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            users.Add(new User(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4)));
        return users;
    }

    public async Task UpdatePasswordHashAsync(int userId, string newHash)
    {
        await using var cmd = _dataSource.CreateCommand(
            "UPDATE accounts SET password = $1 WHERE id = $2");
        cmd.Parameters.AddWithValue(newHash);
        cmd.Parameters.AddWithValue(userId);
        await cmd.ExecuteNonQueryAsync();
    }
}
