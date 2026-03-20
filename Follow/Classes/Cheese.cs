using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

public class Cheese : Image
{
    public static event Action<Cheese>? CheeseSpawned;
    public Double X, Y;
    public Cheese(double x, double y)
    {
        X = x; Y = y;
        this.Source = new BitmapImage(new Uri("/Cheese.png", UriKind.Relative));
        this.Width = 30;
        this.Height = 30;
        Canvas.SetLeft(this, x - 15);
        Canvas.SetTop(this, y - 15);
        Canvas.SetZIndex(this, -5);
        Point pos = new Point(x, y);
        CheeseSpawned?.Invoke(this);
    }
}