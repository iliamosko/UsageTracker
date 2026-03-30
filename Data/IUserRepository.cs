using UsageTracker.Entities;

namespace UsageTracker.Data;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task UpdatePasswordHashAsync(int userId, string newHash);
}
