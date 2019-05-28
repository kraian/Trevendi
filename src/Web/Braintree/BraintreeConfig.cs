using Microsoft.Extensions.Configuration;
using Braintree;

namespace Web.Braintree
{
    public class BraintreeConfig : IBraintreeConfig
    {
        public string Environment { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        private readonly IConfiguration _configuration;
        private IBraintreeGateway _braintreeGateway;

        public BraintreeConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IBraintreeGateway GetGateway()
        {
            if (_braintreeGateway == null)
            {
                _braintreeGateway = CreateGateway();
            }

            return _braintreeGateway;
        }

        private IBraintreeGateway CreateGateway()
        {
            Environment = System.Environment.GetEnvironmentVariable("BraintreeEnvironment");
            MerchantId = System.Environment.GetEnvironmentVariable("BraintreeMerchantId");
            PublicKey = System.Environment.GetEnvironmentVariable("BraintreePublicKey");
            PrivateKey = System.Environment.GetEnvironmentVariable("BraintreePrivateKey");

            if (Environment == null || MerchantId == null || PublicKey == null || PrivateKey == null)
            {
                Environment = _configuration.GetSection("Braintree").GetSection("Environment").Value;
                MerchantId = _configuration.GetSection("Braintree").GetSection("MerchantId").Value;
                PublicKey = _configuration.GetSection("Braintree").GetSection("PublicKey").Value;
                PrivateKey = _configuration.GetSection("Braintree").GetSection("PrivateKey").Value;
            }

            return new BraintreeGateway(Environment, MerchantId, PublicKey, PrivateKey);
        }
    }
}
