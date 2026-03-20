using Follow;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System.Security.Policy;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal class PhRat : Rat
{
    internal World _world;
    private Queue<Collectable> _removalQueue = new Queue<Collectable>();
    bool left = false;
    private Body _playerBody;
    private const float M2P = 20f;
    private Canvas _canvas;
    private MediaPlayer Boing = new();
    public PhRat(Canvas canvas) : base(canvas, 3)
    {
        Boing.Open(new Uri("C:\\Users\\skuec\\source\\repos\\Follow\\Follow\\Boing.mp3"));
        _canvas = canvas;
        _world = new(new Vector2(0, 9f));
        Vector2 playerPosition = new Vector2(0, 0);
        _playerBody = _world.CreateBody(playerPosition, 0, BodyType.Dynamic);
        Fixture pfixture = _playerBody.CreateRectangle(0.8f, 0.8f, .1f, Vector2.Zero);
        pfixture.Restitution = 0.4f;
        pfixture.Friction = 0.6f;
        _playerBody.FixedRotation = true;
        Pos(1, 1);
        PhysicStart();

    }
    public void PhysicStart()
    {
        _ = StartPhysicThread();
    }

    public override void Pos(double x, double y)
    {
        ResetMovement();
        _playerBody.Position = new((float)x, (float)y);
    }
    public void MarkForDeletion(Collectable item)
    {
        // Verhindert, dass ein Objekt mehrfach in die Queue wandert
        if (!_removalQueue.Contains(item))
        {
            _removalQueue.Enqueue(item);
        }
    }
    new public void DreheRechts(double _)
    {
        DreheRechts();
    }
    new public void DreheLinks(double _)
    {
        DreheLinks();
    }
    public void DreheRechts()
    {
        left = false;
        this.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        double scaleX = left ? 1 : 1;
        this.RenderTransform = new ScaleTransform(scaleX, 1);
    }
    public void DreheLinks()
    {
        left = true;
        this.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        double scaleX = left ? -1 : 1;
        this.RenderTransform = new ScaleTransform(scaleX, 1);
    }
    public void Jump(float x, float y)
    {
        if (left) x = -x;
        _playerBody.ApplyForce(new(x, y));
    }
    new public void Vor(double Distanz)
    {
        if (left) Distanz = -Distanz;
        _playerBody.ApplyForce(new((float)Distanz, 0));
    }
    private async Task StartPhysicThread()
    {
        float dt = .05f;
        while (this.Parent != null && _playerBody.Position.Y < 50) 
        {
            HandleInput();
            await Task.Run(() => { _world.Step(dt); });
            this.Dispatcher.Invoke(() =>
            {
                _posX = _playerBody.Position.X * M2P;
                _posY = _playerBody.Position.Y * M2P;
                while (_removalQueue.Count > 0)
                {
                    var body = _removalQueue.Dequeue();
                    _canvas.Children.Remove(body._sprite);
                    _world.Remove(body._body); 
                }
                Scroll();
                AktualisierePosition();
            });
            await Task.Delay(25);
        }
    }
    private void HandleInput()
    {
        float moveForce = 0.5f;
        float jumpImpulse = 1f;
        if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right))
        {
           
            _playerBody.ApplyForce(new Vector2(moveForce, 0));
            this.Dispatcher.Invoke(() => DreheRechts());
        }
        else if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left))
        {
            _playerBody.ApplyForce(new Vector2(-moveForce, 0));
            this.Dispatcher.Invoke(() => DreheLinks());
        }
        if (Keyboard.IsKeyDown(Key.Space) || Keyboard.IsKeyDown(Key.W))
        {
            // Einfacher Check: Nur springen, wenn die vertikale Geschwindigkeit fast 0 ist
            if (Math.Abs(_playerBody.LinearVelocity.Y) < 0.1f)
            {
                _playerBody.ApplyLinearImpulse(new Vector2(moveForce, jumpImpulse));
                Boing.Position = new(0);
                Boing.Play();
            }
        }
    }
    private void Scroll()
    {
        double kameraX = (_playerBody.Position.X * M2P) - 200;
        _canvas.RenderTransform = new TranslateTransform(-kameraX, 0);
    }
    public void InitMap()
    {
        Pos(14, 5);
        Boden(6, 3, 12);
        Boden(17, 4, 12);
        Boden(29, 3, 12);
        Boden(37, 5, 3);
        Boden(41, 7, 3);
        Boden(36, 9, 4);
        Boden(27, 10, 12);
        Boden(18, 8, 12);
        Kaese k = new(this, _canvas, new(12, 2f));
        k = new(this, _canvas, new(30, 2f));
        k = new(this, _canvas, new(41, 6f));
        k = new(this, _canvas, new(33, 9f));
    }
    public void Boden(float x, float y, float length)
    {
        while (_world.IsLocked) ;
        GroundObject Ground = new(_world, _canvas, new(x, y), new(length, 0.5f));
    }

    public void ResetMovement()
    {
        
        _playerBody.LinearVelocity = Vector2.Zero;
        _playerBody.AngularVelocity = 0f;
    }
}
internal abstract class Collectable
{
    internal Image _sprite;
    internal Body _body;
    protected Canvas _canvas;
    protected PhRat rat;
    protected World _world;
    private const float M2P = 20f;

    public event Action OnCollected;

    public Collectable(PhRat rat, Canvas canvas, Vector2 position, string imagePath)
    {
        _world = rat._world;
        this.rat = rat;
        _canvas = canvas;
        _sprite = new Image
        {
            Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
            Width = 32,
            Height = 32
        };
        _canvas.Children.Add(_sprite);
        while (_world.IsLocked) ;
        _body = _world.CreateBody(position, 0, BodyType.Static);
        var fixture = _body.CreateRectangle(1f, 1f, 1f, Vector2.Zero);
        fixture.IsSensor = true;
        _body.OnCollision += HandleCollision;
        UpdateVisualPosition();
    }

    private bool HandleCollision(Fixture sender, Fixture other, Contact contact)
    {
        if (other.Body.BodyType == BodyType.Dynamic)
        {
            rat.MarkForDeletion(this);
        }
        return true;
    }

    public virtual void Collect()
    {
        _canvas.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_body != null)
            {
                _world.Remove(_body);
                _body = null; 
                _canvas.Children.Remove(_sprite);
                OnCollected?.Invoke();
            }
        }));
    }

    public void UpdateVisualPosition()
    {
        Canvas.SetLeft(_sprite, (_body.Position.X * M2P) - (_sprite.Width / 2));
        Canvas.SetTop(_sprite, (_body.Position.Y * M2P) - (_sprite.Height / 2));
    }
}
internal class Kaese : Collectable
{
    public int Points { get; private set; } = 10;

    public Kaese(PhRat rat, Canvas canvas, Vector2 position)
        : base(rat, canvas, position, "Cheese.png")
    {
        _sprite.Width = 25;
        _sprite.Height = 25;
    }

    public override void Collect()
    {
        base.Collect();
    }
}