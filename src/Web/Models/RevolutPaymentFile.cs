using CsvHelper.Configuration.Attributes;

namespace Web.Models
{
    public class RevolutPaymentFile
    {
        [Name("Name")]
        public string Name { get; set; }

        [Name("Recipient type")]
        public string RecipientType { get; set; }

        [Name("IBAN")]
        public string IBAN { get; set; }

        [Name("BIC")]
        public string BIC { get; set; }

        [Name("Recipient bank country")]
        public string RecipientBankCountry { get; set; }

        [Name("Currency")]
        public string Currency { get; set; }

        [Name("Amount")]
        public decimal Amount { get; set; }

        [Name("Payment reference")]
        public string PaymentReference { get; set; }

        [Name("Recipient country")]
        public string RecipientCountry { get; set; }

        [Name("Address line 1")]
        public string AddressLine1 { get; set; }

        [Name("Address line 2")]
        [Optional]
        public string AddressLine2 { get; set; }

        [Name("City")]
        public string City { get; set; }

        [Name("Postal code")]
        public string PostalCode { get; set; }
    }
}
