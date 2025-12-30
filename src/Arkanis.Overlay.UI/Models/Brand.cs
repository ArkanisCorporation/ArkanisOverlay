namespace Arkanis.Overlay.UI.Models;

using Avalonia;
using Avalonia.Media;

public static class Brand
{
    public static class Colors
    {
        public static readonly Color Black = Avalonia.Media.Colors.Black;

        public static class Arkanis
        {
            public static readonly Color Black = Color.Parse("#0E0E0F");
            public static readonly Color White = Color.Parse("#F0F0F1");
            public static readonly Color Magenta = Color.Parse("#FF50E5");
            public static readonly Color Purple = Color.Parse("#4300C0");
            public static readonly Color Blue = Color.Parse("#0015CA");
            public static readonly Color Cyan = Color.Parse("#38FFFF");
        }
    }

    public static class Brushes
    {
        public static readonly IBrush Black = new SolidColorBrush(Avalonia.Media.Colors.Black);

        public static class Arkanis
        {
            public static readonly IBrush Black = new SolidColorBrush(Colors.Arkanis.Black);
            public static readonly IBrush Magenta = new SolidColorBrush(Colors.Arkanis.Magenta);
            public static readonly IBrush Purple = new SolidColorBrush(Colors.Arkanis.Purple);
            public static readonly IBrush Blue = new SolidColorBrush(Colors.Arkanis.Blue);
            public static readonly IBrush Cyan = new SolidColorBrush(Colors.Arkanis.Cyan);

            public static readonly IBrush DiamondGradientNorthEast = new LinearGradientBrush
            {
                // Use 50% midpoint
                GradientStops = [new GradientStop(Colors.Arkanis.Magenta, 0.0), new GradientStop(Colors.Arkanis.Purple, 1.0)],
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            };

            public static readonly IBrush DiamondGradientSouthEast = new LinearGradientBrush
            {
                // TODO: Use 62% midpoint
                GradientStops = [new GradientStop(Colors.Arkanis.Purple, 0.0), new GradientStop(Colors.Black, 1.0)],
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            };

            public static readonly IBrush DiamondGradientNorthWest = new LinearGradientBrush
            {
                // TODO: Use 48% midpoint
                GradientStops = [new GradientStop(Colors.Arkanis.Blue, 0.0), new GradientStop(Colors.Black, 1.0)],
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            };

            public static readonly IBrush DiamondGradientSouthWest = new LinearGradientBrush
            {
                // TODO: Use 65% midpoint
                GradientStops = [new GradientStop(Colors.Arkanis.Cyan, 0.0), new GradientStop(Colors.Arkanis.Blue, 1.0)],
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            };
        }
    }
}
