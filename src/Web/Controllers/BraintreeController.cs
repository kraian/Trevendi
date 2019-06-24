using Braintree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly IOptions<ArcadierSettings> _arcadierSettings;
        private readonly AppDbContext _db;

        public BraintreeController(
            ILogger<BraintreeController> logger,
            IConfiguration configuration,
            IBraintreeConfig braintreeConfig,
            IOptions<ArcadierSettings> arcadierSettings,
            AppDbContext db)
        {
            _logger = logger;
            _configuration = configuration;
            _braintreeConfig = braintreeConfig;
            _arcadierSettings = arcadierSettings;
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

            decimal adminFee = paymentDetails.Amount * _arcadierSettings.Value.Commission / 100;
            decimal amount = paymentDetails.Amount - adminFee;

            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodNonce = model.PaymentMethodNonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            Result<Transaction> result = await gateway.Transaction.SaleAsync(request);

            paymentDetails.TransactionStatus = Models.TransactionStatus.Success;
            if (!result.IsSuccess() && result.Transaction == null)
            {
                paymentDetails.TransactionStatus = Models.TransactionStatus.Failure;
            }

            string statusText = paymentDetails.TransactionStatus == Models.TransactionStatus.Success ? Success : Failure;
            bool arcadierSuccess = await SetArcadierTransactionStatusAsync(statusText, paymentDetails);

            paymentDetails.ArcadierTransactionStatus = Models.TransactionStatus.Success;
            if (!arcadierSuccess)
            {
                paymentDetails.ArcadierTransactionStatus = Models.TransactionStatus.Failure;
            }

            _db.SaveDetails(model.PayKey, paymentDetails);
            return RedirectToArcadier(paymentDetails.InvoiceNo);
        }

        private async Task<bool> SetArcadierTransactionStatusAsync(string status, PaymentDetails paymentDetails)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = UrlHelper.GetTransactionStatusUrl(_arcadierSettings.Value.MarketplaceUrl);
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(url, new
                    {
                        invoiceno = paymentDetails.InvoiceNo,
                        hashkey = paymentDetails.Hashkey,
                        gateway = paymentDetails.Gateway,
                        paykey = paymentDetails.PayKey,
                        status = status
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Error posting to {url}.");
                        return false;
                    }

                    string text = await response.Content.ReadAsStringAsync();
                    ArcadierResponse arcadierResponse = JsonConvert.DeserializeObject<ArcadierResponse>(text);
                    if (arcadierResponse == null)
                    {
                        _logger.LogWarning($"Arcadier response is null.");
                        return false;
                    }

                    if (!arcadierResponse.Success)
                    {
                        _logger.LogWarning(arcadierResponse.Error);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occured while calling Arcadier to update the invoice.");
                return false;
            }
        }

        private RedirectResult RedirectToArcadier(string invoiceNo)
        {
            string url = UrlHelper.GetCurrentStatusUrl(_arcadierSettings.Value.MarketplaceUrl, invoiceNo);
            return Redirect(url);
        }
    }
}
