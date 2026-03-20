using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Follow
{
    internal class Labyrinth
    {
        int Richtung = 0;
        public bool Stop = true;
        private Canvas _canvas;
        private int _sizeX, _sizeY;
        Point GridPos = new(0, 0);
        Stack<Point> LastPos = new();
        private FieldBlock[,] _fields;
        private Rat rat;
        public Labyrinth(Canvas canvas, int sizeX, int sizeY)
        {
            rat = new(canvas,3);
            rat.Pos(25, 25);
            _canvas = canvas;
            _sizeX = sizeX;
            _sizeY = sizeY;
            _fields = new FieldBlock[sizeX, sizeY];
            GenerateGrid();
        }
        private Point GridToCanvas()
        {
            Point CPos = new Point();
            CPos.X = GridPos.X*50+25;
            CPos.Y = GridPos.Y*50+25;
            return CPos;
        }
        private void GenerateGrid()
        {
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    FieldBlock block = new FieldBlock(x, y, 50);
                    Canvas.SetLeft(block, x * 50);
                    Canvas.SetTop(block, y * 50);

                    _canvas.Children.Add(block);
                    _fields[x, y] = block; // Im internen Grid speichern
                }
            }
        }
        public bool Vor()
        {
            Point APos = GridPos;
            switch (Richtung)
                {
                case 0:
                    if(IstWand((int)GridPos.X+1, (int)GridPos.Y)) return false;
                    GridPos.X++; break;
                case 1:
                    if(IstWand((int)GridPos.X, (int)GridPos.Y-1)) return false;
                    GridPos.Y--; break;
                case 2:
                    if(IstWand((int)GridPos.X-1, (int)GridPos.Y)) return false;
                    GridPos.X--; break;
                case 3:
                    if(IstWand((int)GridPos.X, (int)GridPos.Y+1)) return false;
                    GridPos.Y++; break;
                
                default: Richtung = 0; break;
            }
            LastPos.Push(APos);
            
            _fields[(int)APos.X, (int)APos.Y].IsVisited();
            
            Point CPos = GridToCanvas();
            rat.GeheZu(CPos.X, CPos.Y);
            return true;
        }
        public void DreheRechts()
        {
            Richtung--;
            if (Richtung < 0) Richtung = 3; 
            rat.Winkel(90*Richtung);
        }
        public void DreheLinks()
        {
            Richtung++;
            if (Richtung > 3) Richtung = 0; 
            rat.Winkel(90*Richtung);
        }
        public void SetzeRichtung(int richtung)
        {
            Richtung = richtung;
            rat.Winkel(richtung*90);
        }
        public int LeseRichtung() { return Richtung; }
        public bool IstWand(int x, int y)
        {
            if (x >= 0 && x < _sizeX && y >= 0 && y < _sizeY)
                return _fields[x, y].IsWall||_fields[x, y].Visited;
            return true; 
        }
        public void Zurueck()
        {
            if(LastPos.Count == 0) return;
            _fields[(int)GridPos.X, (int)GridPos.Y].IsVisited();
            GridPos=LastPos.Pop();
            Point CPos = GridToCanvas();
            rat.GeheZu(CPos.X, CPos.Y);

        }
        public async Task<bool> SucheAusweg()
        {
            for (int i = 0; i < 4; i++)
            {
                if(Stop) return false;
                if (Vor())
                {
                    if(GridPos == new Point(_sizeX-1, _sizeY-1))
                    {
                        MessageBox.Show("Ausgang gefunden!","Ziel");
                        return true;
                    } 
                    await Task.Delay(500);
                    if (await SucheAusweg())
                    {
                        return true;
                    }
                    Zurueck();
                    await Task.Delay(500);
                }
                DreheRechts();
            }
            return false;
        }
        public void Reset()
        {
            Stop = true;
            rat.Schnell();
            rat.Pos(25, 25);
            GridPos = new(0,0);
            rat.SpurLoeschen();
            foreach (var item in _fields) { item.Visited = false; item._rect.Fill = item.IsWall ? Brushes.DarkGray : Brushes.Transparent; }
        }
    }
    internal class FieldBlock : UserControl
    {
        public Rectangle _rect;
        public bool IsWall { get; set; } = false;
        public bool Visited { get; set; } = false;
        public int GridX { get; }
        public int GridY { get; }

        public FieldBlock(int x, int y, double size)
        {
            GridX = x;
            GridY = y;
            this.Width = size;
            this.Height = size;

            _rect = new Rectangle
            {
                Fill = Brushes.Transparent,
                Stroke = Brushes.LightBlue,
                StrokeThickness = 1
            };
            Canvas.SetZIndex(this, -5);
            this.Content = _rect;
            this.MouseDown += (s, e) => ToggleWall();
        }
        public void IsVisited()
        {
            Visited = !Visited;
            _rect.Fill = Visited? Brushes.Salmon : Brushes.Transparent;
        }
        public void ToggleWall()
        {
            IsWall = !IsWall;
            _rect.Fill = IsWall ? Brushes.DarkGray : Brushes.Transparent;
            _rect.Stroke = IsWall ? Brushes.Black : Brushes.LightBlue;
        }

    }
}
