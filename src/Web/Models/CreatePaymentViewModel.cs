namespace Web.Models
{
    public class CreatePaymentViewModel
    {
        public string PayKey { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentMethodNonce { get; set; }
    }
}
