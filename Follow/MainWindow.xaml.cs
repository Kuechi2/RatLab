using System.Windows;
using System.Windows.Controls;
namespace Follow
{
    public partial class MainWindow : Window
    {
        private Labyrinth Lab;

        public MainWindow()
        {
            InitializeComponent();
            Menueintrag("Laby", () => Laby());
        }
        private async void Click_Gruen(object sender, RoutedEventArgs e)
        {
            
        }

        private async void Click_Rot(object sender, RoutedEventArgs e)
        {

        }
        void Laby()
        {
            if (Lab != null) { return; }
            Lab = new(MainCanvas, 8, 8);
        }

        private void Click_Blau(object sender, RoutedEventArgs e)
        {
            Lab.Stop = false;
            Lab.SucheAusweg();
        }
        private async void Click_Gelb(object sender, RoutedEventArgs e)
        {
          Lab.Reset();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Kommt Zeit, kommt Rat - kommen Zeiten, kommen Ratten.\n\nLizenz: MIT\nhttps://opensource.org/license/mit" +
                "\nDieses Produkt wurde ohne Tierversuche entwickelt", "Küchis Rattenlabor 0.3",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private MenuItem Menueintrag(string header, Action action)
        {
            var mi = new MenuItem { Header = header };
            mi.Click += (_, __) => action();
            Hauptmenu.Items.Add(mi);
            return mi;
        }
    }
}