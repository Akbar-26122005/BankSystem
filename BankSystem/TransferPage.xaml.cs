using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Windows.Threading;

namespace BankSystem {
    public partial class TransferPage : Page {
        private TransactionManager? transactionManager = null;
        public TransferPage(TransactionManager transactionManager) {
            InitializeComponent();
            this.transactionManager = transactionManager;
        }

        private void StateButton_Click(object sender, RoutedEventArgs e) {
            if (sender == nextStateButton) {
                border32.Visibility = Visibility.Visible;
                DoubleAnimation anim = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
                Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
                anim.Completed += Animation_Completed1;
                border32.BeginAnimation(Border.OpacityProperty, anim);
            } else {
                DoubleAnimation anim = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
                Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
                anim.Completed += Anim_Completed2;
                previousStateButton.BeginAnimation(Button.OpacityProperty, anim);
            }
        }

        private void Animation_Completed1(object? sender, EventArgs e) {
            state3.Visibility = Visibility.Collapsed;

            Storyboard storyboard = new Storyboard();
            ThicknessAnimation anim1 = new ThicknessAnimation { To = new Thickness(0, 0, 380, 0), Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            ThicknessAnimation anim2 = new ThicknessAnimation { To = new Thickness(380, 0, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            Storyboard.SetTarget(anim1, state1);
            Storyboard.SetTarget(anim2, state2);
            Storyboard.SetTargetProperty(anim1, new PropertyPath("Margin"));
            Storyboard.SetTargetProperty(anim2, new PropertyPath("Margin"));

            DoubleAnimation anim3 = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            DoubleAnimation anim4 = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            Storyboard.SetTarget(anim3, border12);
            Storyboard.SetTarget(anim4, border22);
            Storyboard.SetTargetProperty(anim3, new PropertyPath("Opacity"));
            Storyboard.SetTargetProperty(anim4, new PropertyPath("Opacity"));

            storyboard.Children.Add(anim1);
            storyboard.Children.Add(anim2);
            storyboard.Children.Add(anim3);
            storyboard.Children.Add(anim4);
            storyboard.Begin();

            previousStateButton.Visibility = Visibility.Visible;
            DoubleAnimation anim5 = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(1)) };
            Storyboard.SetTargetProperty(anim5, new PropertyPath("Opacity"));
            previousStateButton.BeginAnimation(Grid.OpacityProperty, anim5);
        }

        private void Anim_Completed2(object? sender, EventArgs e) {
            Storyboard storyboard = new Storyboard();
            ThicknessAnimation anim1 = new ThicknessAnimation { To = new Thickness(0), Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            ThicknessAnimation anim2 = new ThicknessAnimation { To = new Thickness(0), Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            Storyboard.SetTarget(anim1, state1);
            Storyboard.SetTarget(anim2, state2);
            Storyboard.SetTargetProperty(anim1, new PropertyPath("Margin"));
            Storyboard.SetTargetProperty(anim2, new PropertyPath("Margin"));

            DoubleAnimation anim3 = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            DoubleAnimation anim4 = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.4)) };
            Storyboard.SetTarget(anim3, border12);
            Storyboard.SetTarget(anim4, border22);
            Storyboard.SetTargetProperty(anim3, new PropertyPath("Opacity"));
            Storyboard.SetTargetProperty(anim4, new PropertyPath("Opacity"));

            storyboard.Children.Add(anim1);
            storyboard.Children.Add(anim2);
            storyboard.Children.Add(anim3);
            storyboard.Children.Add(anim4);
            storyboard.Completed += Storyboard_Completed3;
            storyboard.Begin();
        }

        private void Storyboard_Completed3(object? sender, EventArgs e) {
            state3.Visibility = Visibility.Visible;
            DoubleAnimation anim = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
            anim.Completed += Anim_Completed4;
            border32.BeginAnimation(Border.OpacityProperty, anim);
        }

        private void Anim_Completed4(object? sender, EventArgs e) {
            border32.Visibility = Visibility.Collapsed;
        }
    }
}
