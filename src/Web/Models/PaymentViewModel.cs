namespace Web.Models
{
    public class PaymentViewModel
    {
        public string InvoiceNo { get; set; }
        public string PayKey { get; set; }
        public decimal Amount { get; set; }
        public string ClientToken { get; set; }
    }
}
