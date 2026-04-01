using UsageTracker.Services;
using Xunit;

namespace UsageTracker.Tests;

public class AuthServiceTests
{
    private readonly AuthService _sut = new();

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsUser()
    {
        var user = await _sut.AuthenticateAsync("user@example.com", "password123");
        Assert.NotNull(user);
        Assert.Equal("user@example.com", user!.GetEmail());
    }

    [Fact]
    public async Task AuthenticateAsync_EmptyEmail_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync(string.Empty, "password");
        Assert.Null(user);
    }

    [Fact]
    public async Task AuthenticateAsync_EmptyPassword_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync("user@example.com", string.Empty);
        Assert.Null(user);
    }

    [Fact]
    public async Task AuthenticateAsync_WhitespaceCredentials_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync("   ", "   ");
        Assert.Null(user);
    }

    [Fact]
    public void HashPassword_ProducesValidBCryptHash()
    {
        var hash = _sut.HashPassword("mypassword");
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void HashPassword_SameInput_ProducesDifferentHashes()
    {
        var hash1 = _sut.HashPassword("mypassword");
        var hash2 = _sut.HashPassword("mypassword");
        Assert.NotEqual(hash1, hash2); // BCrypt includes random salt
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("secret");
        Assert.True(_sut.VerifyPassword("secret", hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("secret");
        Assert.False(_sut.VerifyPassword("wrong", hash));
    }
}
