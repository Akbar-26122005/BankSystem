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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BankSystem {
    public partial class AccountsPage : Page {
        private Border? selectedAccountItem = null;
        private TransactionManager? transactionManager = null;

        public AccountsPage(TransactionManager transactionManager) {
            InitializeComponent();
            this.transactionManager = transactionManager;
            UpdateAccountsItem();
        }

        private void UpdateAccountsItem() {
            if (transactionManager == null) return;
            AccountItems.Items.Clear();
            foreach (var account in transactionManager.GetAllAccounts()) {
                AccountItems.Items.Add(CreateNewAccountItem(account));
            }
        }

        private Border CreateNewAccountItem(BankAccount account) {
            Border border = new Border {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E8B57")),
                CornerRadius = new CornerRadius(8), Width = 460, Height = 60,
                BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Colors.Black),
                Cursor = Cursors.Hand,
                Child = new Grid {
                    Children = {
                        new TextBlock {
                            Text = $"{account.Id}", FontSize = 20, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC3A0")),
                            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 100, 0), TextDecorations = TextDecorations.Underline
                        },
                        new TextBlock {
                            Text = $"{account.GetBalance()}", FontSize = 15, Foreground = new SolidColorBrush(Colors.DarkGray), Margin = new Thickness(0, 0, 10, 3),
                            HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom,
                        }
                    }
                }
            };

            return border;
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e) {
            if (transactionManager == null) return;
            transactionManager.AddAccount(new BankAccount());
            UpdateAccountsItem();
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e) {
            if (AccountItems.SelectedItem == null || transactionManager == null) return;
            string id = MainWindow.FindVisualChild<TextBlock>((Border)AccountItems.SelectedItem).Text;
            BankAccount? account = transactionManager.FindAccount(account => account.Id == long.Parse(id));
            if (account == null) return;
            transactionManager.RemoveAccount(account);
            UpdateAccountsItem();
        }
    }
}
