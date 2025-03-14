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
    public class MyDbContext : DbContext {
        public DbSet<BankAccount> Accounts {  get; set; }

        public MyDbContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }

    public class BankAccount {
        public int Id { get; }
        private double balance;
        public string Email { get; set; }
        public string Password { get; set; }

        public BankAccount(int id, double initialBalance, string email, string password) {
            Id = id;
            balance = initialBalance;
            Email = email;
            Password = password;
        }

        public double GetBalance() => balance;

        public void Withdraw(double amount, string password) {
            if (this.Password != password)
                throw new UnauthorizedAccessException("Неверный пароль");

            if (balance < amount)
                throw new InsufficientFundsError("В балансе не хватает средств");

            balance -= amount;
        }

        public void Deposit(double amount) {
            if (amount < 0)
                throw new InvalidAmountError("Пополняемая сумма отрицательная");

            balance += amount;
        }

        public override string ToString() {
            return $"{Id},{balance},{Email},{Password}";
        }
    }

    public class TransactionManager {
        private readonly MyDbContext context = new MyDbContext();

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
    }
}
