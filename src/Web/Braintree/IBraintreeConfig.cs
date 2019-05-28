using Braintree;

namespace Web.Braintree
{
    public interface IBraintreeConfig
    {
        string Environment { get; set; }
        string MerchantId { get; set; }
        string PublicKey { get; set; }
        string PrivateKey { get; set; }
        IBraintreeGateway GetGateway();
    }
}
