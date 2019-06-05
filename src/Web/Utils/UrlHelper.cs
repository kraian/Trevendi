namespace Web.Utils
{
    public static class UrlHelper
    {
        public static string GetTransactionStatusUrl(string marketplace)
        {
            return $"{marketplace}/user/checkout/transaction-status";
        }

        public static string GetCurrentStatusUrl(string marketplace, string invoiceNo)
        {
            return $"{marketplace}/user/checkout/current-status?invoiceNo={invoiceNo}";
        }
    }
}
