using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BankSystem {
    public class AccountsContext : DbContext {
        public DbSet<BankAccount> Accounts {  get; set; }

        public AccountsContext() => Database.EnsureCreated();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<BankAccount>()
                .HasKey(b => b.Id); // Указываем, что Id является первичным ключом
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
        public long Id { get; }
        private double Balance;
        public string Email { get; set; }
        public string Password { get; set; }

        public BankAccount() {
            Id = GenerateNewId();
            Balance = 0;
            Email = "";
            Password = "1234";
        }

        public BankAccount(double initialBalance, string email, string password) {
            Id = GenerateNewId();
            Balance = initialBalance;
            Email = email;
            Password = password;
        }

        public double GetBalance() => Balance;

        public void Withdraw(double amount, string password) {
            if (this.Password != password)
                throw new UnauthorizedAccessException("Неверный пароль");

            if (Balance < amount)
                throw new InsufficientFundsError("В балансе не хватает средств");

            Balance -= amount;
        }

        public void Deposit(double amount) {
            if (amount < 0)
                throw new InvalidAmountError("Пополняемая сумма отрицательная");

            Balance += amount;
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

    public class TransactionManager {
        private readonly AccountsContext context = new AccountsContext(
            
        );

        public void Transfer(int fromAccountId, int toAccountId, double amount, string password) {
            var fromAccount = context.Accounts.FirstOrDefault(acc => acc.Id == fromAccountId);
            var toAccount = context.Accounts.FirstOrDefault(acc => acc.Id == toAccountId);

            if (fromAccount == null || toAccount == null) {
                throw new AccountNotFoundError("Аккаунт не найден!");
            }

            fromAccount.Withdraw(amount, password);
            toAccount.Deposit(amount);
            Console.WriteLine("Деньги успешно переведены");
        }

        public void AddAccount(BankAccount account) {
            context.Accounts.Add(account);
            context.SaveChanges();
        }

        public void RemoveAccount(BankAccount account) {
            context.Accounts.Remove(account);
            context.SaveChanges();
        }

        public BankAccount? FindAccount(Func<BankAccount, bool> predicate) {
            return context.Accounts.FirstOrDefault(predicate);
        }

        public List<BankAccount> GetAllAccounts() => context.Accounts.ToList();
    }
}
