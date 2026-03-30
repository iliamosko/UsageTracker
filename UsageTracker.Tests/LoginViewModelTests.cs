using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using Xunit;
using NSubstitute;
using UsageTracker.Entities;
using UsageTracker.Services;
using UsageTracker.ViewModels;

namespace UsageTracker.Tests;

public class LoginViewModelTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();

    private LoginViewModel CreateSut() => new(_authService);

    [Fact]
    public void LoginCommand_CannotExecute_WhenEmailEmpty()
    {
        var sut = CreateSut();
        sut.Email = string.Empty;
        sut.Password = "password";

        bool canExecute = false;
        sut.LoginCommand.CanExecute.Subscribe(x => canExecute = x);

        canExecute.Should().BeFalse();
    }

    [Fact]
    public void LoginCommand_CannotExecute_WhenPasswordEmpty()
    {
        var sut = CreateSut();
        sut.Email = "user@example.com";
        sut.Password = string.Empty;

        bool canExecute = false;
        sut.LoginCommand.CanExecute.Subscribe(x => canExecute = x);

        canExecute.Should().BeFalse();
    }

    [Fact]
    public void LoginCommand_CanExecute_WhenBothFieldsFilled()
    {
        var sut = CreateSut();
        sut.Email = "user@example.com";
        sut.Password = "password";

        bool canExecute = false;
        sut.LoginCommand.CanExecute.Subscribe(x => canExecute = x);

        canExecute.Should().BeTrue();
    }

    [Fact]
    public async Task LoginCommand_OnFailedAuth_SetsErrorMessage()
    {
        _authService.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult<User?>(null));

        var sut = CreateSut();
        sut.Email = "bad@user.com";
        sut.Password = "wrongpassword";

        // Register a no-op handler for navigation so the interaction doesn't throw
        sut.NavigateToTracking.RegisterHandler(i => i.SetOutput(Unit.Default));

        await sut.LoginCommand.Execute().FirstAsync();

        sut.ErrorMessage.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginCommand_OnException_SetsErrorMessage()
    {
        _authService.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns<Task<User?>>(_ => throw new Exception("DB down"));

        var sut = CreateSut();
        sut.Email = "user@example.com";
        sut.Password = "password";

        sut.NavigateToTracking.RegisterHandler(i => i.SetOutput(Unit.Default));

        // ReactiveUI routes the exception to ThrownExceptions (which sets ErrorMessage),
        // then re-throws from the Execute() observable. Swallow it here; check ErrorMessage.
        try { await sut.LoginCommand.Execute().FirstAsync(); } catch { }

        sut.ErrorMessage.Should().Contain("DB down");
    }
}
