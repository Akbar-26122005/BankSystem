using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Windows.Markup;
using System.Net;
using System.Net.Mail;


namespace BankSystem {
    public class AccountsContext : DbContext {
        public DbSet<BankAccount> Accounts { get; set; }
        public DbSet<Currency> Currencies { get; set; }

        public AccountsContext() => Database.EnsureCreated();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Указываем Id как первичный ключ
            modelBuilder.Entity<BankAccount>()
                .HasKey(b => b.Id);
            modelBuilder.Entity<Currency>()
                .HasKey(c => c.Id);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (optionsBuilder.IsConfigured) return;
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder.UseLazyLoadingProxies()
                .UseNpgsql(config.GetConnectionString("DefaultConnection"));
        }
    }

    public class Currency {
        public static List<string> Currencies { get; private set; } = new List<string>() { "RUB", "USD", "EUR" };

        public int Id { get; set; }
        public string? Name { get; private set; }
        public decimal Amount { get; set; }

        public Guid BankAccountId { get; set; }
        public virtual BankAccount? BankAccount { get; set; }

        public Currency() {
            Name = "RUB";
            Amount = 0;
        }

        public Currency(int currency, decimal amount) {
            Name = currency < 0 || currency >= Currencies.Count ? Currencies[0] : Currencies[currency];
            Amount = amount;
        }
    }

    public class BankAccount {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual List<Currency> Currencies { get; set; }

        public BankAccount() {
            Id = Guid.NewGuid();
            Email = "";
            Password = "1234";
            Currencies = new List<Currency>();
        }

        public BankAccount(double initialBalance, string email, string password, List<double> balances) {
            Id = Guid.NewGuid();
            Email = email;
            Password = password;
            Currencies = new List<Currency>();
        }

        public decimal GetBalance(string currencyName) {
            AccountsContext context = new AccountsContext();
            foreach (var currency in context.Currencies) {
                if (currency.BankAccountId == this.Id && currency.Name == currencyName) {
                    return currency.Amount;
                }
            }
            throw new Exception($"Такой валюты не существует! ({Currencies.Count})");
        }

        public void Withdraw(decimal amount, string password, string currency) {
            if (this.Password != password)
                throw new UnauthorizedAccessException("Неверный пароль");

            Currency Currency = Currencies.FirstOrDefault(c => c.Name == currency)!;
            if (Currency.Amount < amount)
                throw new InsufficientFundsError("В балансе не хватает средств");
            else if (amount < 0)
                throw new InvalidAmountError("Пополняемая сумма отрицательная");

            Currency.Amount -= amount;
        }

        public void Deposit(decimal amount, string currency) {
            if (amount < 0)
                throw new InvalidAmountError("Пополняемая сумма отрицательная");

            Currency Currency = Currencies.FirstOrDefault(c => c.Name == currency)!;
            Currency.Amount += amount;
        }

        public override string ToString() {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class TransactionManager {
        private readonly AccountsContext context = new AccountsContext();
        private CurrencyConverter _currencyConverter;

        public TransactionManager() {
            _currencyConverter = new CurrencyConverter();
        }

        public async Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount, string password, string fromCurrency = "RUB", string toCurrency = "RUB") {
            var fromAccount = context.Accounts.FirstOrDefault(acc => acc.Id == fromAccountId);
            var toAccount = context.Accounts.FirstOrDefault(acc => acc.Id == toAccountId);

            if (fromAccount == null || toAccount == null) {
                throw new AccountNotFoundError("Аккаунт не найден!");
            }

            try {
                fromAccount.Withdraw(amount, password, fromCurrency);
                var currencyConverter = new CurrencyConverter();

                decimal finalAmount = 0;
                if (fromCurrency == toCurrency) {
                    finalAmount = amount;
                } else {
                    finalAmount = await currencyConverter.Convert(fromCurrency, toCurrency, amount);
                }
                toAccount.Deposit(finalAmount, toCurrency);

                //var smtpServer = "smtp.example.com";
                //var smtpPort = 587;
                //var smtpUser = ""; // Нужно ввести email
                //var smtpPass = ""; // Пароль

                //var emailNotifier = new EmailNotifier(smtpServer, smtpPort, smtpUser, smtpPass);

                //string toEmail = fromAccount.Email;
                //string subject = "Текстовое письмо";
                //string body = "<h1>Средства переведены</h1><p>С вашего счета были переведены денежные средства</p>";

                //emailNotifier.SendEmail(toEmail, subject, body);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void AddAccount(BankAccount account) {
            context.Accounts.Add(account);
            context.SaveChanges();
        }

        public void RemoveAccount(BankAccount account) {
            foreach (var currency in context.Currencies) {
                if (currency.BankAccount == account) context.Currencies.Remove(currency);
            }
            context.Accounts.Remove(account);
            context.SaveChanges();
        }

        public bool Deposit(Guid accountId, decimal amount, string currency) {
            BankAccount? account = context.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return false;
            account.Deposit(amount, currency);
            context.SaveChanges();
            return true;
        }

        public BankAccount? FindAccount(Func<BankAccount, bool> predicate) {
            return context.Accounts.FirstOrDefault(predicate);
        }

        public List<BankAccount> GetAllAccounts() => context.Accounts.ToList();

        public void SaveChanges() => context.SaveChanges();
    }

    // Модуль конвертации валют
    public class CurrencyConverter {
        private readonly HttpClient _httpClient;

        public CurrencyConverter() {
            _httpClient = new HttpClient();
        }

        public async Task<decimal> Convert(string fromCurrency, string toCurrency, decimal amount) {
            var response = await _httpClient.GetStringAsync($"https://api.exchangerate-api.com/v4/latest/{fromCurrency}");
            var rates = JObject.Parse(response)["rates"];
            var rate = rates![toCurrency]!.Value<decimal>();
            return amount * rate;
        }
    }

    // Модуль нотификации пользователей
    public class EmailNotifier {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailNotifier(string smtpServer, int smtpPort, string smtpUser, string smtpPass) {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
        }

        public void SendEmail(string toEmail, string subject, string body) {
            try {
                using (var message = new MailMessage()) {
                    message.From = new MailAddress(_smtpUser);
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var client = new SmtpClient(_smtpServer, _smtpPort)) {
                        client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                        client.EnableSsl = true;
                        client.Send(message);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
            }
        }
    }
}
