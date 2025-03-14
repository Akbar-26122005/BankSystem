using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem {
    public class InsufficientFundsError : Exception {
        public InsufficientFundsError(string message) : base(message) { }
    }

    public class InvalidAmountError : Exception {
        public InvalidAmountError(string message) : base(message) { }
    }

    public class AccountNotFoundError : Exception {
        public AccountNotFoundError(string message) : base(message) { }
    }
}
