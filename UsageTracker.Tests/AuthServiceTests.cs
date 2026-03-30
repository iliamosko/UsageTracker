using FluentAssertions;
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
        user.Should().NotBeNull();
        user!.GetEmail().Should().Be("user@example.com");
    }

    [Fact]
    public async Task AuthenticateAsync_EmptyEmail_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync(string.Empty, "password");
        user.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_EmptyPassword_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync("user@example.com", string.Empty);
        user.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WhitespaceCredentials_ReturnsNull()
    {
        var user = await _sut.AuthenticateAsync("   ", "   ");
        user.Should().BeNull();
    }

    [Fact]
    public void HashPassword_ProducesValidBCryptHash()
    {
        var hash = _sut.HashPassword("mypassword");
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void HashPassword_SameInput_ProducesDifferentHashes()
    {
        var hash1 = _sut.HashPassword("mypassword");
        var hash2 = _sut.HashPassword("mypassword");
        hash1.Should().NotBe(hash2); // BCrypt includes random salt
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("secret");
        _sut.VerifyPassword("secret", hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("secret");
        _sut.VerifyPassword("wrong", hash).Should().BeFalse();
    }
}
