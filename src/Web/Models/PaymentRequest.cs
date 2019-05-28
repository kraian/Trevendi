namespace Web.Models
{
    public class PaymentRequest
    {
        public string invoiceno { get; set; }
        public string total { get; set; }
        public string currency { get; set; }
        public string gateway { get; set; }
        public string hashkey { get; set; }
    }
}
