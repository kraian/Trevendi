using ApplicationCore.Common;
using System;

namespace ApplicationCore.Entities
{
    public class PaymentDetails
    {
        private const decimal MaxAmount = 1_000_000;

        public string PayKey { get; set; }
        public string InvoiceNo { get; set; }
        public string AmountInternal { get; set; }
        public decimal Amount
        {
            get { return Convert.ToDecimal(AmountInternal); }
            set { AmountInternal = value.ToString(); }
        }
        public string Currency { get; set; }
        public string Gateway { get; set; }
        public string Hashkey { get; set; }
        public TransactionStatus BraintreeStatus { get; set; }
        public TransactionStatus ArcadierStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        public PaymentDetails()
        {
        }

        public PaymentDetails(string payKey, decimal amount, string invoiceNo, string currency, string gateway, string hashkey)
        {
            Contract.Require(!string.IsNullOrWhiteSpace(payKey), "PayKey is required.");
            Contract.Require(amount >= 0, "Amount cannot be negative.");
            Contract.Require(amount <= MaxAmount, $"Amount cannot be greater than {MaxAmount}.");

            PayKey = payKey;
            Amount = amount;
            InvoiceNo = invoiceNo;
            Currency = currency;
            Gateway = gateway;
            Hashkey = hashkey;
            BraintreeStatus = TransactionStatus.New;
            ArcadierStatus = TransactionStatus.New;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
