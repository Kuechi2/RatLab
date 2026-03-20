using System.Windows;
namespace Follow
{
    public partial class MainWindow : Window
    {
        private Rat rat;
        public MainWindow()
        {
            InitializeComponent();
            rat = new(MainCanvas,3);
        }
        private async void Click_Gruen(object sender, RoutedEventArgs e)
        {
            for(int n=0; n<1;n++)
            {
                for (int i = 0; i < 1; i++)
                {
                    rat.Pos(100 + i * 60, 100+n*60);
                    Vieleck(50,6);
                }
            }
        }
        void Vieleck(int Kantenlaenge, int Eckenzahl)
        {
            for (int i = 0; i < Eckenzahl; i++)
            {
            rat.Vor(Kantenlaenge);
            rat.DreheLinks(360.0/Eckenzahl);
                
            }
        }
        private async void Click_Rot(object sender, RoutedEventArgs e)
        {
            RatImagePainter gesicht = new(MainCanvas, 12);
                gesicht.PaintImage("C:\\Users\\skuec\\Desktop\\WIN_20260305_17_05_49_Pro.jpg");
            gesicht.fastdraw = true;
        }

        private void Haus(Point pos)
        {
            rat.Pos(pos.X,pos.Y);
            Vieleck(50, 3);
            rat.Pos(pos.X, pos.Y+50);
            Vieleck(50, 4);
        }

        private void Click_Blau(object sender, RoutedEventArgs e)
        {
            rat.Schnell();

        }
        private async void Click_Gelb(object sender, RoutedEventArgs e)
        {
          
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Kommt Zeit, kommt Rat - kommen Zeiten, kommen Ratten.\n\nLizenz: MIT\nhttps://opensource.org/license/mit" +
                "\nDieses Produkt wurde ohne Tierversuche entwickelt", "Küchis Rattenlabor 0.3",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}