namespace Web.Models
{
    public class SplitAmount
    {
        public decimal MerchantAmount { get; }
        public decimal AdminFee { get; }

        public SplitAmount(decimal merchantAmount, decimal adminFee)
        {
            MerchantAmount = merchantAmount;
            AdminFee = adminFee;
        }
    }
}
