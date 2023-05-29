using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;

namespace FloatingIce;

public partial class MainWindow : Window
{
    private static DateTime _lastAnimationTime = DateTime.MinValue;

    public MainWindow()
    {
        InitializeComponent();

        if (ActualThemeVariant == ThemeVariant.Dark)
            ExperimentalAcrylicBorder.Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintOpacity = 1,
                TintColor = Colors.Black,
                MaterialOpacity = 0.65
            };
        else
            ExperimentalAcrylicBorder.Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintOpacity = 1,
                TintColor = Colors.WhiteSmoke,
                MaterialOpacity = 0.65
            };
    }

    private void WindowCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        GetMainWindow()!.Close();
    }

    private void WindowMinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        GetMainWindow()!.WindowState = WindowState.Minimized;
    }

    private static MainWindow? GetMainWindow()
    {
        return (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
            .MainWindow as MainWindow;
    }

    private async void WindowsTopmostButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!CanAnimate()) return;
        if (GetMainWindow()!.Topmost)
        {
            await SwitchControlVisibility(TopmostIcon, TopNormalIcon, WindowsTopmostButton);
            GetMainWindow()!.Topmost = false;
        }
        else
        {
            await SwitchControlVisibility(TopNormalIcon, TopmostIcon, WindowsTopmostButton);
            GetMainWindow()!.Topmost = true;
        }
    }

    private static async Task SwitchControlVisibility(Visual from, Visual to, Control? parent = null)
    {
        if (parent != null) parent.IsEnabled = false;
        foreach (var control in from.GetLogicalDescendants())
            if (control is Control c)
                c.IsEnabled = false;

        var transition = new CrossFade(TimeSpan.FromMilliseconds(100));
        await transition.Start(from, to, new CancellationToken());

        foreach (var control in to.GetLogicalDescendants())
            if (control is Control c)
                c.IsEnabled = true;
        if (parent != null) parent.IsEnabled = true;
    }

    private static bool CanAnimate()
    {
        var now = DateTime.Now;
        var canAnimate = now - _lastAnimationTime > TimeSpan.FromMilliseconds(100);
        if (canAnimate) _lastAnimationTime = now;

        return canAnimate;
    }

    private async void FloatBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (float.TryParse(FloatBox.Text, out var f))
        {
            if (ErrorIcon.IsVisible) await SwitchControlVisibility(ErrorIcon, TransitionIcon);

            var result = BitConverter.ToString(BitConverter.GetBytes(f)).Split("-").Reverse()
                .Aggregate(string.Empty, (current, item) => current + item);

            HexBox.Text = result;
            ClipButtons.IsEnabled = true;
        }
        else
        {
            if (FloatBox.Text == string.Empty)
            {
                ClipButtons.IsEnabled = false;
                HexBox.Text = string.Empty;
                return;
            }

            await SwitchControlVisibility(TransitionIcon, ErrorIcon);
        }
    }

    private async void HexBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (HexBox.Text is { Length: 8 })
        {
            try
            {
                var result = BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToUInt32(HexBox.Text, 16)), 0);
                if (ErrorIcon.IsVisible) await SwitchControlVisibility(ErrorIcon, TransitionIcon);

                FloatBox.Text = result.ToString(CultureInfo.InvariantCulture);
                ClipButtons.IsEnabled = true;
            }
            catch
            {
                await SwitchControlVisibility(TransitionIcon, ErrorIcon);
            }
        }
        else
        {
            if (HexBox.Text == string.Empty)
            {
                ClipButtons.IsEnabled = false;
                FloatBox.Text = string.Empty;
                return;
            }

            await SwitchControlVisibility(TransitionIcon, ErrorIcon);
        }
    }

    private void CopyFloatButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Clipboard?.SetTextAsync(FloatBox.Text);
    }

    private void CopyHexButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Clipboard?.SetTextAsync(HexBox.Text);
    }
}