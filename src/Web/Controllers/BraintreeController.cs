using Braintree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Web.Braintree;
using Web.Database;
using Web.Models;
using Web.Utils;

namespace Web.Controllers
{
    public class BraintreeController : Controller
    {
        private const string Success = "success";
        private const string Failure = "failed";

        private readonly ILogger<BraintreeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBraintreeConfig _braintreeConfig;
        private readonly AppDbContext _db;

        public BraintreeController(ILogger<BraintreeController> logger, IConfiguration configuration, IBraintreeConfig braintreeConfig, AppDbContext db)
        {
            _logger = logger;
            _configuration = configuration;
            _braintreeConfig = braintreeConfig;
            _db = db;
        }

        [HttpGet]
        public IActionResult Payment(string invoiceNo, string paykey)
        {
            PaymentDetails paymentDetails = _db.GetDetails(paykey);
            if (paymentDetails == null)
            {
                _logger.LogWarning("Payment details are null.");
                return RedirectToArcadier(invoiceNo);
            }

            IBraintreeGateway gateway = _braintreeConfig.GetGateway();

            var model = new PaymentViewModel
            {
                InvoiceNo = paymentDetails.InvoiceNo,
                PayKey = paykey,
                Amount = paymentDetails.Amount,
                Currency = paymentDetails.Currency,
                ClientToken = gateway.ClientToken.Generate()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(CreatePaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            PaymentDetails paymentDetails = _db.GetDetails(model.PayKey);
            if (paymentDetails == null)
            {
                _logger.LogWarning("Payment details are null.");
                return RedirectToArcadier(model.InvoiceNo);
            }

            IBraintreeGateway gateway = _braintreeConfig.GetGateway();
            var request = new TransactionRequest
            {
                Amount = paymentDetails.Amount,
                PaymentMethodNonce = model.PaymentMethodNonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            PaymentStatus status = PaymentStatus.Success;
            Result<Transaction> result = gateway.Transaction.Sale(request);
            
            if (!result.IsSuccess() && result.Transaction == null)
            {
                status = PaymentStatus.Failure;
            }

            paymentDetails.PaymentStatus = status;
            _db.SaveDetails(model.PayKey, paymentDetails);

            string statusText = status == PaymentStatus.Success ? Success : Failure;
            await SetArcadierTransactionStatus(statusText, paymentDetails);
            return RedirectToArcadier(paymentDetails.InvoiceNo);
        }

        private async Task<bool> SetArcadierTransactionStatus(string status, PaymentDetails paymentDetails)
        {
            try
            {
                string marketplaceUrl = _configuration.GetSection("Arcadier").GetSection("MarketplaceUrl").Value;

                using (var httpClient = new HttpClient())
                {
                    string url = UrlHelper.GetTransactionStatusUrl(marketplaceUrl);
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(url, new
                    {
                        invoiceno = paymentDetails.InvoiceNo,
                        hashkey = paymentDetails.Hashkey,
                        gateway = paymentDetails.Gateway,
                        paykey = paymentDetails.PayKey,
                        status = status
                    });

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occured while calling Arcadier to update the invoice.");
            }

            return false;
        }

        private RedirectResult RedirectToArcadier(string invoiceNo)
        {
            string marketplaceUrl = _configuration.GetSection("Arcadier").GetSection("MarketplaceUrl").Value;
            string url = UrlHelper.GetCurrentStatusUrl(marketplaceUrl, invoiceNo);
            return Redirect(url);
        }
    }
}
