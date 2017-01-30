using System;

namespace Payments.Model {
    public class Payment {
        public int Id { get; set; }

        public string Name { get; set; }

        public int WalletId { get; set; }

        public Wallet Wallet { get; set; }

        public Category[] Categories { get; set; }

        public PaymentType Type { get; set; }

        public DateTime Date { get; set; }

        public String DateString => Date.ToString("dd.MM.yyyy");

        public decimal Amount { get; set; }

    }
}