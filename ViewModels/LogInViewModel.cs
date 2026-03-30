using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using UsageTracker.Entities;
using UsageTracker.Services;

namespace UsageTracker.ViewModels;

public sealed class LoginViewModel : ReactiveObject
{
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public ReactiveCommand<Unit, User?> LoginCommand { get; }

    /// <summary>
    /// Raised when authentication succeeds. The View registers a handler —
    /// the ViewModel never references View types.
    /// </summary>
    public Interaction<User, Unit> NavigateToTracking { get; } = new();

    public LoginViewModel(IAuthService authService)
    {
        var canLogin = this.WhenAnyValue(
            x => x.Email, x => x.Password,
            (e, p) => !string.IsNullOrWhiteSpace(e) && !string.IsNullOrWhiteSpace(p));

        LoginCommand = ReactiveCommand.CreateFromTask(
            async () => await authService.AuthenticateAsync(Email, Password),
            canLogin);

        LoginCommand
            .Where(user => user is not null)
            .SelectMany(user => NavigateToTracking.Handle(user!).Select(_ => user))
            .Subscribe();

        LoginCommand
            .Where(user => user is null)
            .Subscribe(_ => ErrorMessage = "Invalid email or password.");

        LoginCommand.ThrownExceptions
            .Subscribe(ex => ErrorMessage = $"Error: {ex.Message}");

        LoginCommand.IsExecuting
            .Subscribe(busy => IsBusy = busy);
    }
}
