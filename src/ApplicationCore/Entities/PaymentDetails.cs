using System;

namespace ApplicationCore.Entities
{
    public class PaymentDetails
    {
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
        public string Note { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public TransactionStatus ArcadierTransactionStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        public PaymentDetails()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
