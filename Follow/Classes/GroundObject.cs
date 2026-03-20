using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Common; // Hier kommt dein Float-Vector2 her

namespace Follow
{
    internal class GroundObject
    {
        private Body _body;
        private Rectangle _view;
        private Canvas _canvas;
        private const float M2P = 20f;
        internal GroundObject(World world, Canvas canvas, Vector2 pos, Vector2 size)
        {
            _canvas = canvas;
            while (world.IsLocked) ;
            _body = world.CreateBody(pos, 0, BodyType.Static);
            var fixture = _body.CreateRectangle(size.X, size.Y, 1f, Vector2.Zero);
            fixture.Restitution = 0.3f;
            fixture.Friction = 0.5f;
            _view = new Rectangle
            {
                Width = size.X * M2P,
                Height = size.Y * M2P,
                Fill = new SolidColorBrush(Color.FromRgb(255,0,200)),
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(_view, (pos.X - size.X/2f) * M2P);
            Canvas.SetTop(_view, (pos.Y - size.Y/2f) * M2P);

            _canvas.Children.Add(_view);
        }

        public void Remove()
        {
            _canvas.Children.Remove(_view);
            _body.World.Remove(_body);
        }
    }
}