using UsageTracker.Entities;

namespace UsageTracker.Services;

/// <summary>
/// Stub auth service — returns a hardcoded user on any login attempt.
/// Replace the AuthenticateAsync body with a real IUserRepository call when ready.
/// </summary>
public sealed class AuthService : IAuthService
{
    public Task<User?> AuthenticateAsync(string email, string password)
    {
        // Short-circuit: accept any non-empty credentials and return a guest-like user.
        // TODO: replace with real DB lookup once password_hash migration is done.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Task.FromResult<User?>(null);

        var user = new User(0, email, string.Empty, email, string.Empty);
        return Task.FromResult<User?>(user);
    }

    public string HashPassword(string plaintext) =>
        BCrypt.Net.BCrypt.HashPassword(plaintext, workFactor: 12);

    public bool VerifyPassword(string plaintext, string storedHash) =>
        BCrypt.Net.BCrypt.Verify(plaintext, storedHash);
}
