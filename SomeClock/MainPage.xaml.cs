using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace SomeClock;

public partial class MainPage : ContentPage
{
    private readonly ClockDrawable _clockDrawable;
    private IDispatcherTimer? _timer;
    private DateTime _currentTime;

    public MainPage()
    {
        InitializeComponent();

        _clockDrawable = new ClockDrawable();
        ClockView.Drawable = _clockDrawable;

        UpdateTime();
        InitializeTimer();
    }

    private void InitializeTimer()
    {
        // Create UI dispatcher timer that ticks every second.
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer is null)
        {
            return;
        }

        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        // Read current time and update both digital and analog representations.
        _currentTime = DateTime.Now;

        DigitalTimeLabel.Text = _currentTime.ToString("HH:mm:ss");
        DateLabel.Text = _currentTime.ToString("dd.MM.yyyy");

        _clockDrawable.UpdateTime(_currentTime);
        InvalidateClock();
    }

    private void InvalidateClock()
    {
        // Request the GraphicsView to redraw with the new angles.
        ClockView.Invalidate();
    }
}

internal class ClockDrawable : IDrawable
{
    private DateTime _time = DateTime.Now;

    public void UpdateTime(DateTime time)
    {
        _time = time;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        var center = new PointF(dirtyRect.Center.X, dirtyRect.Center.Y);
        var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f - 12f;

        DrawFace(canvas, center, radius);
        DrawTicks(canvas, center, radius);
        DrawHands(canvas, center, radius);
    }

    private static void DrawFace(ICanvas canvas, PointF center, float radius)
    {
        canvas.FillColor = Colors.White;
        canvas.StrokeColor = Color.FromArgb("#D1D5DB");
        canvas.StrokeSize = 4;
        canvas.FillCircle(center, radius);
        canvas.DrawCircle(center, radius);
    }

    private static void DrawTicks(ICanvas canvas, PointF center, float radius)
    {
        canvas.StrokeColor = Color.FromArgb("#9CA3AF");
        canvas.StrokeSize = 2;

        for (int i = 0; i < 60; i++)
        {
            float angle = (float)(Math.PI * 2 * i / 60);
            float outer = radius - 8;
            float inner = i % 5 == 0 ? radius - 22 : radius - 16;

            var start = new PointF(
                center.X + inner * (float)Math.Sin(angle),
                center.Y - inner * (float)Math.Cos(angle));

            var end = new PointF(
                center.X + outer * (float)Math.Sin(angle),
                center.Y - outer * (float)Math.Cos(angle));

            canvas.DrawLine(start, end);
        }
    }

    private void DrawHands(ICanvas canvas, PointF center, float radius)
    {
        // Calculate angles for hour, minute, and second hands.
        float seconds = _time.Second;
        float minutes = _time.Minute + seconds / 60f;
        float hours = (_time.Hour % 12) + minutes / 60f;

        float secondAngle = seconds * 6f;
        float minuteAngle = minutes * 6f;
        float hourAngle = hours * 30f;

        DrawHand(canvas, center, radius * 0.5f, hourAngle, 6, Color.FromArgb("#111827"));
        DrawHand(canvas, center, radius * 0.75f, minuteAngle, 4, Color.FromArgb("#111827"));
        DrawHand(canvas, center, radius * 0.9f, secondAngle, 2, Color.FromArgb("#EF4444"));

        canvas.FillColor = Color.FromArgb("#111827");
        canvas.FillCircle(center, 6);
    }

    private static void DrawHand(ICanvas canvas, PointF center, float length, float angleDegrees, float stroke, Color color)
    {
        float angleRadians = (float)(Math.PI / 180 * angleDegrees);

        var end = new PointF(
            center.X + length * (float)Math.Sin(angleRadians),
            center.Y - length * (float)Math.Cos(angleRadians));

        canvas.StrokeColor = color;
        canvas.StrokeSize = stroke;
        canvas.DrawLine(center, end);
    }
}
