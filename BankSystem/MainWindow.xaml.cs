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

namespace BankSystem {
    public partial class MainWindow : Window {
        private Button? selectedButton = null;
        private Dictionary<Button, Page> pages = new Dictionary<Button, Page>();
        private TransactionManager transactionManager = new TransactionManager();

        public MainWindow() {
            InitializeComponent();
            pages.Add((Button)sideMenu_StackPanel.Children[0], new AccountsPage(transactionManager));
            pages.Add((Button)sideMenu_StackPanel.Children[1], new TransferPage(transactionManager));
        }

        private void Window_Closed(object sender, EventArgs e) {
            transactionManager.SaveChanges();
        }

        private void sideMenu_MouseEnter(object sender, MouseEventArgs e) {
            Storyboard storyboard = (Storyboard)FindResource("sideMenuHoverLeaveStoryBoard");
            storyboard.Stop();

            storyboard = (Storyboard)FindResource("sideMenuHoverEnterStoryBoard");
            storyboard.Begin();

            DoubleAnimation animation = new DoubleAnimation { To = 1.0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            foreach (Button button in sideMenu_StackPanel.Children) {
                FindVisualChild<TextBlock>(button).BeginAnimation(TextBlock.OpacityProperty, animation);
            }
        }

        private void sideMenu_MouseLeave(object sender, MouseEventArgs e) {
            Storyboard storyboard = (Storyboard)FindResource("sideMenuHoverEnterStoryBoard");
            storyboard.Stop();

            storyboard = (Storyboard)FindResource("sideMenuHoverLeaveStoryBoard");
            storyboard.Begin();

            DoubleAnimation animation = new DoubleAnimation { To = 0.0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            foreach (Button button in sideMenu_StackPanel.Children) {
                FindVisualChild<TextBlock>(button).BeginAnimation(TextBlock.OpacityProperty, animation);
            }
        }

        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) {
                    return typedChild;
                }

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) {
                    return childOfChild;
                }
            }
            return null!;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) {
            if (((Button)sender) == selectedButton) return;
            SolidColorBrush brush = new SolidColorBrush(Colors.Transparent);
            ((Button)sender).Background = brush;
            ColorAnimation animation = new ColorAnimation { To = Colors.Gray, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            ((Button)sender).Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e) {
            ColorAnimation animation = new ColorAnimation { To = sender == selectedButton ? (Color)ColorConverter.ConvertFromString("#4A90E2") : Colors.Transparent,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            ((Button)sender).Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (selectedButton != null)
                selectedButton.Background.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation { To = Colors.Transparent, Duration = new Duration(TimeSpan.FromSeconds(0.3)) });
            selectedButton = (Button)sender;
            ColorAnimation animation = new ColorAnimation { To = (Color)ColorConverter.ConvertFromString("#4A90E2"), Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            ((Button)sender).Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            pages[(Button)sideMenu_StackPanel.Children[0]] = new AccountsPage(transactionManager);
            foreach (var page in pages)
                if (page.Key == sender) {
                    mainFrame.Content = page.Value;
                }
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e) {
            ColorAnimation animation = new ColorAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.1)) };
            animation.To = sender == selectedButton ? (Color)ColorConverter.ConvertFromString("#444") : Colors.Gray;
            ((Button)sender).Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
    }
}