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

namespace BankSystem {
    public class AccountsContext : DbContext {
        public DbSet<BankAccount> Accounts { get; set; }

        public AccountsContext() => Database.EnsureCreated();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<BankAccount>()
                .HasKey(b => b.Id); // Указываем Id как первичный ключ
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (optionsBuilder.IsConfigured) return;
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        }
    }

    public class BankAccount {
        public long Id { get; set; }
        public double Balance { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public static readonly List<string> Currencies = new List<string>() { "RUB", "USD", "EUR" };
        public List<double> Balances;

        public BankAccount() {
            Id = GenerateNewId();
            Balance = 0;
            Email = "";
            Password = "1234";

            Balances = new List<double>();
            for (int i = 0; i < Currencies.Count; i++) {
                Balances.Add(0);
            }
        }

        public BankAccount(double initialBalance, string email, string password, List<double> balances) {
            Id = GenerateNewId();
            Balance = initialBalance;
            Email = email;
            Password = password;

            Balances = balances;
            if (Balances.Count < Currencies.Count) {
                for (int i = 0; i < Currencies.Count; i++) {
                    Balances.Add(0);
                }
            }
        }

        //public double GetBalance() => Balance;
        public double GetBalance(string currency) => Balances[Currencies.IndexOf(currency)];

        public void Withdraw(double amount, string password, string currency) {
            if (this.Password != password)
                throw new UnauthorizedAccessException("Неверный пароль");
            else if (Balances[Currencies.IndexOf(currency)] < amount)
                throw new InsufficientFundsError("В балансе не хватает средств");
            else if (amount < 0)
                throw new InvalidAmountError("Пополняемая сумма отрицательная");

            Balances[Currencies.IndexOf(currency)] -= amount;
        }

        public void Deposit(double amount, string currency) {
            if (amount < 0)
                throw new InvalidAmountError("Пополняемая сумма отрицательная");

            Balances[Currencies.IndexOf(currency)] += amount;
        }

        public long GenerateNewId() {
            string id = "";
            for (int i = 0; i < 16; i++) {
                int result = new Random().Next(0, 10);
                if (id == "" && result == 0) i--;
                else {
                    id += result.ToString();
                }
            }
            return long.Parse(id);
        }

        public override string ToString() {
            return $"{Id},{Balance},{Email},{Password}";
        }
    }

    //public class CurrencyConverter {
    //    private readonly string apiKey = "";
    //    private readonly string apiUrl = "https://api.exchangerate-api.com/v4/latest/";

    //    public async Task<double> Convert(string fromCurrency, string toCurrency, double amount) {
    //        using (HttpClient client = new HttpClient()) {
    //            var response = await client.GetStringAsync(apiUrl + fromCurrency);
    //            var rates = JObject.Parse(response)["rates"];
    //            double convertionRate = rates[toCurrency].Value<double>();
    //            return amount * convertionRate;
    //        }
    //    }
    //}

    public class TransactionManager {
        private readonly AccountsContext context = new AccountsContext();
        private CurrencyConverter _currencyConverter;

        public TransactionManager() {
            //context.Accounts.Add(BankAccount.GetBankAccount(1111_1111_1111_1111));
            //context.SaveChanges();
            _currencyConverter = new CurrencyConverter();
        }

        public void Transfer(int fromAccountId, int toAccountId, double amount, string password, string fromCurrency = "RUB", string toCurrency = "RUB") {
            var fromAccount = context.Accounts.FirstOrDefault(acc => acc.Id == fromAccountId);
            var toAccount = context.Accounts.FirstOrDefault(acc => acc.Id == toAccountId);

            if (fromAccount == null || toAccount == null) {
                throw new AccountNotFoundError("Аккаунт не найден!");
            }

            try {
                fromAccount.Withdraw(amount, password, fromCurrency);
                toAccount.Deposit(amount, toCurrency);
                Console.WriteLine("Деньги успешно переведены");
            } catch (Exception ex) {
                Console.WriteLine($"Error: ${ex.Message}");
            }
        }

        public void AddAccount(BankAccount account) {
            context.Accounts.Add(account);
            context.SaveChanges();
        }

        public void RemoveAccount(BankAccount account) {
            context.Accounts.Remove(account);
            context.SaveChanges();
        }

        public bool Deposit(long accountId, double amount, string currency) {
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
            var rate = rates[toCurrency].Value<decimal>();
            return amount * rate;
        }
    }
}
