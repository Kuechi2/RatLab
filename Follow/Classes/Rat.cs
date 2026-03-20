using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Follow
{
    public struct Polar
    {
        public double Winkel;
        public double Distanz;
    }
    internal class Rat : Image, IDisposable
    {
        //public new string Name { get; set; }
        public Point Koordinate => new Point(_posX, _posY);
        internal uint Mode = 0;
        private Canvas _canvas;
        private double _richtung = 0;  // in Grad: 0 = rechts, 90 = unten, 180 = links, 270 = oben
        public double Richtung { get; }
        internal double _posX = 0;
        internal double _posY = 0;
        private TaskCompletionSource? _runTcs;
        // Animation
        private DispatcherTimer _timer;
        private Queue<Action> _befehle = new Queue<Action>();
        private bool _istAmBewegen = false;
        // Bewegungs-Animation
        private double _zielX;
        private double _zielY;
        private double _schrittGroesse = 2;
        private Polyline? _aktuelleLinie;

        // Drehungs-Animation
        private double _zielRichtung;
        private double _drehGeschwindigkeit = 15;

        public Rat(Canvas canvas, uint mode = 0)
        {
            _canvas = canvas;
            this.Source = new BitmapImage(new Uri("/Ratte.png", UriKind.Relative));
            this.Width = 40;
            this.Height = 40;
            this.Stretch = Stretch.Uniform;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.RenderTransform = new RotateTransform(0);
            _canvas.Children.Add(this);
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(5); // ca. 60 FPS
            _timer.Tick += Timer_Tick!;
            Mode = mode;
            if ((mode & 2) == 2)
            {
                Cheese.CheeseSpawned += CheeseSeek;
            }
        }
        public void Dispose()
        {
            _timer.Tick -= Timer_Tick!;
            if (_canvas.Children.Contains(this))
            {
                _canvas.Children.Remove(this);
            }
        }
        private void CheeseSeek(Cheese cheese)
        {
            if ((Mode & 2) != 2) return;
            GeheZu(cheese.X, cheese.Y);
        }
        internal virtual void Timer_Tick(object sender, EventArgs e)
        {
            if (_istAmBewegen)
            {
                double dx = _zielX - _posX;
                double dy = _zielY - _posY;
                double distanz = Math.Sqrt(dx * dx + dy * dy);

                if (distanz < _schrittGroesse)
                {
                    _posX = _zielX;
                    _posY = _zielY;
                    AktualisierePosition();
                    _istAmBewegen = false;
                    NaechsterBefehl();

                }
                else
                {
                    _posX += (dx / distanz) * _schrittGroesse;
                    _posY += (dy / distanz) * _schrittGroesse;
                    AktualisierePosition();
                    if (_aktuelleLinie != null)
                    {
                        _aktuelleLinie.Points.Add(new Point(_posX, _posY));
                    }
                }
            }
            else
            {
                double differenz = _zielRichtung - _richtung;
                while (differenz > 180) differenz -= 360;
                while (differenz < -180) differenz += 360;

                if (Math.Abs(differenz) < _drehGeschwindigkeit)
                {
                    _richtung = _zielRichtung;
                    AktualisiereRichtung();
                    NaechsterBefehl();
                }
                else
                {
                    // Weiterdrehen
                    if (differenz > 0)
                        _richtung += _drehGeschwindigkeit;
                    else
                        _richtung -= _drehGeschwindigkeit;

                    AktualisiereRichtung();
                }
            }
        }



        private void NaechsterBefehl()
        {
            if (_befehle.Count > 0)
            {
                Action befehl = _befehle.Dequeue();
                befehl();
            }
            else
            {
                // Alle Befehle abgearbeitet
                _timer.Stop();
                _runTcs?.SetResult();
                _runTcs = null;
            }
        }

        internal void AktualisierePosition()
        {
            Canvas.SetLeft(this, _posX - this.Width / 2);
            Canvas.SetTop(this, _posY - this.Height / 2);
        }

        internal void AktualisiereRichtung()
        {
            ((RotateTransform)this.RenderTransform).Angle = _richtung;
        }
        /// <summary>
        /// Positioniert die Ratte an der Position x,y
        /// </summary>
        /// <param X-Koordinate="x"></param>
        /// <param Y-Koordinate="y"></param>
        public virtual void Pos(double x, double y)
        {
            _befehle.Enqueue(() =>
            {
                _posX = x;
                _posY = y;
                AktualisierePosition();
                NaechsterBefehl();
            });
            StarteAnimation();
        }
        /// <summary>
        /// Setzt den Winkel der Ratte auf die angegebene Gradzahl. 0=Rechts
        /// </summary>
        /// <param WInkel="grad"></param>
        public void Winkel(double grad)
        {
            _befehle.Enqueue(() =>
            {
                _richtung = grad;
                _zielRichtung = grad;
                AktualisiereRichtung();
                NaechsterBefehl();
            });
            StarteAnimation();
        }
        public void Schnell()
        {
            while (_timer.IsEnabled)
            {
                Timer_Tick(new(), new());
            }
        }
        public void Vor(double distanz)
        {
            _befehle.Enqueue(() =>
            {
                double radians = _richtung * Math.PI / 180.0;
                _zielX = _posX + distanz * Math.Cos(radians);
                _zielY = _posY + distanz * Math.Sin(radians);
                _aktuelleLinie = new Polyline();
                _aktuelleLinie.Stroke = Brushes.Black;
                _aktuelleLinie.StrokeThickness = 2;
                _aktuelleLinie.Points.Add(new Point(_posX, _posY));
                _canvas.Children.Insert(0, _aktuelleLinie);

                _istAmBewegen = true;
            });

            StarteAnimation();
        }

        public void Zurueck(double distanz)
        {
            _befehle.Enqueue(() =>
            {
                double radians = _richtung * Math.PI / 180.0;
                _zielX = _posX - distanz * Math.Cos(radians);
                _zielY = _posY - distanz * Math.Sin(radians);
                _aktuelleLinie = new Polyline();
                _aktuelleLinie.Stroke = Brushes.Black;
                _aktuelleLinie.StrokeThickness = 2;
                _aktuelleLinie.Points.Add(new Point(_posX, _posY));
                _canvas.Children.Insert(0, _aktuelleLinie);

                _istAmBewegen = true;
            });

            StarteAnimation();
        }

        public void DreheRechts(double grad)
        {
            _befehle.Enqueue(() =>
            {
                _zielRichtung = _richtung + grad;
                _istAmBewegen = false;
            });

            StarteAnimation();
        }

        public void DreheLinks(double grad)
        {
            _befehle.Enqueue(() =>
            {
                _zielRichtung = _richtung - grad;
                _istAmBewegen = false;
            });

            StarteAnimation();
        }
        public Task Warte()
        {
            if (_befehle.Count == 0 && !_istAmBewegen)
                return Task.CompletedTask;
            _runTcs = new TaskCompletionSource();
            StarteAnimation();
            return _runTcs.Task;
        }
        public void SpurLoeschen()
        {
            _befehle.Enqueue(() =>
            {
                List<UIElement> zuEntfernen = new List<UIElement>();
                foreach (UIElement element in _canvas.Children)
                {
                    if (element is Polyline)
                    {
                        zuEntfernen.Add(element);
                    }
                }

                foreach (UIElement element in zuEntfernen)
                {
                    _canvas.Children.Remove(element);
                }

                NaechsterBefehl();
            });

            StarteAnimation();
        }

        internal void StarteAnimation()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        /// <summary>
        /// Wandelt einen Vektor in eine Paar Koordinaten um
        /// </summary>
        /// <param name="magnitude">Die Entfernung zum Ziel</param>
        /// <param name="angleDegrees">Der Winkel zum Zielpunkt</param>
        /// <returns></returns>
        public Point VektorKoordinate(double magnitude, double angleDegrees)
        {
            if ((Mode & 1) != 1) return new();
            // Umrechnung von Grad in Bogenmaß (Radiant)
            double angleRad = Math.PI * angleDegrees / 180.0;

            return new Point
            {
                X = (magnitude * Math.Cos(angleRad)),
                Y = (magnitude * Math.Sin(angleRad))
            };
        }
        /// <summary>
        /// Wandelt ein Koordiantenpaar in einen Vektor von deiner Ratte aus um MUSS MIT "await" benutzt werden!
        /// </summary>
        /// <param name="x">X-Achse</param>
        /// <param name="y">Y-Achse</param>
        /// <returns></returns>
        public async Task<Polar> KoordinateVektor(double x, double y)
        {
            if ((Mode & 1) != 1) return new();
            await Warte();
            double deltaX = x - _posX;
            double deltaY = y - _posY;
            double magnitude = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            double angleRad = Math.Atan2(deltaY, deltaX);
            double angleDegrees = angleRad * (180.0 / Math.PI);
            return new Polar
            {
                Distanz = magnitude,
                Winkel = angleDegrees
            };
        }
        /// <summary>
        /// Geht zu einer Koordinate auf dem MainCanvas. MUSS MIT "await" BENUTZT WEDEN!
        /// </summary>
        /// <param name="x">Die X-Position</param>
        /// <param name="y">Die Y-Position</param>
        public async void GeheZu(double x, double y)
        {
            if ((Mode & 1) != 1) return;
            Polar dir = await KoordinateVektor(x, y);
            Winkel(dir.Winkel);
            Vor(dir.Distanz);
        }
        /// <summary>
        /// Gibt die aktuelle Position der Ratte als Point zurück (zum Speichern, der Position)
        /// MUSS MIT "await" BENUTZT WERDEN!
        /// </summary>
        /// <returns>Point (Punkt, wo die Ratte gerade ist)</returns>
        public async Task<Point> GetPos()
        {
            if ((Mode & 1) != 1) return new();
            await Warte();
            return new(_posX, _posY);
        }

    }
}