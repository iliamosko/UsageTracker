using System.Reactive;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;
using UsageTracker.ViewModels;

namespace UsageTracker.Views;

public partial class LoginView : ReactiveWindow<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            ViewModel!.NavigateToTracking.RegisterHandler(interaction =>
            {
                var trackingVm = App.GetService<TrackingViewModel>();
                var tracking = new TrackingView { DataContext = trackingVm };
                tracking.Show();
                Close();
                interaction.SetOutput(Unit.Default);
            });
        });
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}
