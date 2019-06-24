using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Braintree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Web.Braintree;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class BraintreeController : Controller
    {
        private const string Success = "success";
        private const string Failure = "failed";

        private readonly ILogger<BraintreeController> _logger;
        private readonly IBraintreeConfig _braintreeConfig;
        private readonly IPaymentService _paymentService;
        private readonly ArcadierService _arcadierService;

        public BraintreeController(
            ILogger<BraintreeController> logger,
            IBraintreeConfig braintreeConfig,
            IPaymentService paymentService,
            ArcadierService arcadierService)
        {
            _logger = logger;
            _braintreeConfig = braintreeConfig;
            _paymentService = paymentService;
            _arcadierService = arcadierService;
        }

        [HttpGet]
        public async Task<IActionResult> Payment(string invoiceNo, string paykey)
        {
            var result = await _paymentService.GetByPayKeyAsync(paykey);
            if (result.IsFailure)
            {
                _logger.LogError(result.Error);
                return RedirectToArcadier(invoiceNo);
            }

            PaymentDetails payment = result.Value;
            IBraintreeGateway gateway = _braintreeConfig.GetGateway();

            var model = new PaymentViewModel
            {
                InvoiceNo = payment.InvoiceNo,
                PayKey = paykey,
                Amount = payment.Amount,
                Currency = payment.Currency,
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

            var result = await _paymentService.GetByPayKeyAsync(model.PayKey);
            if (result.IsFailure)
            {
                _logger.LogError(result.Error);
                return RedirectToArcadier(model.InvoiceNo);
            }
            
            PaymentDetails payment = result.Value;
            if (payment.TransactionStatus == ApplicationCore.Entities.TransactionStatus.Success)
            {
                _logger.LogError("Transaction already paid.");
                return RedirectToArcadier(model.InvoiceNo);
            }

            SplitAmount splitAmount = _arcadierService.GetSplitAmount(payment.Amount);

            var request = new TransactionRequest
            {
                Amount = splitAmount.MerchantAmount,
                PaymentMethodNonce = model.PaymentMethodNonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            IBraintreeGateway gateway = _braintreeConfig.GetGateway();
            Result<Transaction> transactionResult = await gateway.Transaction.SaleAsync(request);

            payment.TransactionStatus = ApplicationCore.Entities.TransactionStatus.Success;
            if (!transactionResult.IsSuccess() && transactionResult.Transaction == null)
            {
                payment.TransactionStatus = ApplicationCore.Entities.TransactionStatus.Failure;
            }

            string statusText = payment.TransactionStatus == ApplicationCore.Entities.TransactionStatus.Success ? Success : Failure;
            bool arcadierSuccess = await _arcadierService.SetArcadierTransactionStatusAsync(statusText, payment);

            payment.ArcadierTransactionStatus = ApplicationCore.Entities.TransactionStatus.Success;
            if (!arcadierSuccess)
            {
                payment.ArcadierTransactionStatus = ApplicationCore.Entities.TransactionStatus.Failure;
            }

            await _paymentService.UpdateAsync(payment);

            return RedirectToArcadier(payment.InvoiceNo);
        }

        private RedirectResult RedirectToArcadier(string invoiceNo)
        {
            string redirectUrl = _arcadierService.GetRedirectUrl(invoiceNo);
            return Redirect(redirectUrl);
        }
    }
}
