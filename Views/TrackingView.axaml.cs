using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;
using UsageTracker.ViewModels;

namespace UsageTracker.Views;

public partial class TrackingView : ReactiveWindow<TrackingViewModel>
{
    public TrackingView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            ViewModel!.CloseCommand.Subscribe(_ => Close());
            ViewModel!.MinimizeCommand.Subscribe(_ => WindowState = Avalonia.Controls.WindowState.Minimized);
        });
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}
