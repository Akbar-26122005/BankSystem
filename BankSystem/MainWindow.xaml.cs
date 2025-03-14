using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BankSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Storyboard storyboard = (Storyboard)FindResource("AccountsBorderHoverLeaveStoryBoard");
            storyboard.Stop();

            storyboard = (Storyboard)FindResource("AccountsBorderHoverEnterStoryBoard");
            storyboard.Begin();

            CornerRadius currentRadius = AccountsBorder.CornerRadius;
            DoubleAnimation topLeftAnimation = new DoubleAnimation {
                From = currentRadius.TopLeft,
                To = 12,
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            AccountsBorder.CornerRadius = new CornerRadius(0, 10, 10, 0);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Storyboard storyboard = (Storyboard)FindResource("AccountsBorderHoverEnterStoryBoard");
            storyboard.Stop();

            storyboard = (Storyboard)FindResource("AccountsBorderHoverLeaveStoryBoard");
            storyboard.Begin();
            AccountsBorder.CornerRadius = new CornerRadius(0);
        }
    }
}