using UsageTracker.Entities;

namespace UsageTracker.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string email, string password);
    string HashPassword(string plaintext);
    bool VerifyPassword(string plaintext, string storedHash);
}
