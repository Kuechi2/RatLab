using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Follow
{
    internal class AdditionalStuff
    {
        Canvas MainCanvas;
        public AdditionalStuff(Canvas canvas) {
            MainCanvas = canvas;
        }
        public async void Schlange()
        {
            Rat rat = new(MainCanvas);
            rat.Pos(50, 200);
            for (int i = 0; i < 136; i++)
            {
                rat.Winkel(Math.Sin(i / 13.0) * 100.4);
                rat.Vor(6);
            }
            await rat.Warte();
            rat.Dispose();
            rat = null;
        }
        private async void SssTriangle(int a, int b, int c, Point start)
        {
            double alpha = BerechneWinkelGegenueber(a, b, c);
            double beta = BerechneWinkelGegenueber(b, a, c);
            double gamma = 180 - alpha - beta;
            Point A = new(0, 0);
            Point B = new(c, 0);
            Point C = new(Math.Cos(alpha * (Math.PI / 180)) * a, Math.Sin(alpha * (Math.PI / 180)) * a);
            C.Y += B.Y;
            Rat rat = new(MainCanvas);
            rat.Pos(A.X + start.X, A.Y + start.Y);
            rat.Vor(c);
            rat.Pos(10, 10);
            Rat rat2 = new(MainCanvas);
            rat2.Pos(B.X + start.X, B.Y + start.Y);
            rat2.Winkel(180 - beta);
            rat2.Vor(a);
            Rat rat3 = new(MainCanvas);
            rat3.Pos(start.X, start.Y);
            rat3.Winkel(alpha);
            rat3.Vor(b);
            rat2.Pos(10, 10);
            rat3.Pos(10, 10);
            await rat2.Warte();
            rat2.Dispose();
            rat2 = null!;
            rat3.Dispose();
            rat3 = null!;
            rat.Dispose();
            rat = null!;
        }

        /// <summary>
        /// Berrechnet den Winkel gegenüber der ersten Seite (a). Wenn du die Seiten vertauschst, bekommst du auch andere Winkel!
        /// </summary>
        /// <param name="a">Länge der Seite a -> Winkel ALPHA wird berrechnet!</param>
        /// <param name="b">Länge der Seite b</param>
        /// <param name="c">Länge der Seite c</param>
        /// <returns></returns>
        private static double BerechneWinkelGegenueber(int a, int b, int c)
        {
            return Math.Acos((b * b + c * c - a * a) / (double)(2 * b * c)) * 57.27577;
        }

        public async void Koordinator()
        {
            int i;
            List<Rat> rats = new List<Rat>();
            for (i = 0; i < 30; i++)
            {
                rats.Add(new(MainCanvas));
            }
            i = 0;
            foreach (var rat in rats)
            {
                i++;
                if(i%2==0)
                rat.Pos(20, 20 * i + 10);
                else
                {
                    rat.Pos(20 * i + 10, 20);
                    rat.Winkel(90);
                }
                    rat.Vor(600);
            }
            foreach(var rat in rats)
            {
                await rat.Warte();
                rat.Dispose();
            }
        }
    }
}
