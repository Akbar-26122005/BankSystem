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
        private TransactionManager? transactionManager = null;

        public AccountsPage(TransactionManager transactionManager) {
            InitializeComponent();
            this.transactionManager = transactionManager;
            UpdateAccountsItem();
        }

        public void UpdateAccountsItem() {
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
                            Text = $"{account.Id}", FontSize = 18,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC3A0")),
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 100, 0)
                        }
                    }
                }
            };

            return border;
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e) {
            if (transactionManager == null) return;
            BankAccount account = new BankAccount();
            account.Currencies = new List<Currency>();

            for (int i = 0; i < Currency.Currencies.Count; i++) {
                account.Currencies.Add(new Currency(i, 0));
            }

            transactionManager.AddAccount(account);
            UpdateAccountsItem();
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e) {
            if (AccountItems.SelectedItem == null || transactionManager == null) return;
            string id = MainWindow.FindVisualChild<TextBlock>((Border)AccountItems.SelectedItem).Text;
            BankAccount? account = transactionManager.FindAccount(account => account.Id == new Guid(id));
            if (account == null) return;
            transactionManager.RemoveAccount(account);
            UpdateAccountsItem();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e) {
            if (AccountItems.SelectedItem is null) return;
            else if (AccountItems.SelectedItem is Border border) {
                Clipboard.SetText(
                    MainWindow.FindVisualChild<TextBlock>(
                        MainWindow.FindVisualChild<Grid>(border)
                    ).Text.ToString()
                );
            }
        }

        private Border CreatePropertyElement(string content, string text) {
            Border border = new Border {
                Style = FindResource("InfoTileStyle") as Style,
                Child = new StackPanel {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Children = {
                        new Label {
                            Content = content,
                            Style = FindResource("InfoTileLabelStyle") as Style
                        },
                        new TextBox {
                            Width = 140,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            Text = text
                        }
                    }
                }
            };

            return border;
        }

        private void AccountItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (AccountItems.SelectedItem is Border border) {
                var id = MainWindow.FindVisualChild<TextBlock>(MainWindow.FindVisualChild<Grid>(border)).Text.ToString();
                BankAccount account = transactionManager!.FindAccount(a => a.Id == new Guid(id))!;
                propertyId.Text = account.Id.ToString();
                propertyEmail.Text = account.Email;
                propertyPassword.Text = account.Password;

                PropertiesCurrenciesBallances.Children.Clear();
                foreach (var currency in account.Currencies) {
                    PropertiesCurrenciesBallances.Children.Add(CreatePropertyElement(currency.Name!, account.GetBalance(currency.Name!).ToString()));
                }
            }
        }
    }
}