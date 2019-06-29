using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Braintree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class BraintreeController : Controller
    {
        private const string Success = "success";
        private const string Failure = "failed";

        private readonly ILogger<BraintreeController> _logger;
        private readonly IBraintreeGateway _braintreeGateway;
        private readonly IPaymentService _paymentService;
        private readonly ArcadierService _arcadierService;

        public BraintreeController(
            ILogger<BraintreeController> logger,
            IBraintreeGateway braintreeGateway,
            IPaymentService paymentService,
            ArcadierService arcadierService)
        {
            _logger = logger;
            _braintreeGateway = braintreeGateway;
            _paymentService = paymentService;
            _arcadierService = arcadierService;
        }

        [HttpGet]
        public async Task<IActionResult> Payment(string invoiceNo, string paykey)
        {
            try
            {
                PaymentDetails payment = await _paymentService.GetByPayKeyAsync(paykey);
                if (payment.BraintreeStatus == ApplicationCore.Entities.TransactionStatus.Success)
                {
                    _logger.LogError($"Transaction with paykey '{paykey}' is already paid.");
                    return RedirectToArcadier(payment.InvoiceNo);
                }

                var model = new PaymentViewModel
                {
                    InvoiceNo = payment.InvoiceNo,
                    PayKey = paykey,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    ClientToken = _braintreeGateway.ClientToken.Generate()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return RedirectToArcadier(invoiceNo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(CreatePaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                PaymentDetails payment = await _paymentService.GetByPayKeyAsync(model.PayKey);
                if (payment.BraintreeStatus == ApplicationCore.Entities.TransactionStatus.Success)
                {
                    _logger.LogError($"Transaction with paykey '{model.PayKey}' is already paid.");
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

                Result<Transaction> transactionResult = await _braintreeGateway.Transaction.SaleAsync(request);

                if (transactionResult.IsSuccess())
                {
                    payment.BraintreeStatus = ApplicationCore.Entities.TransactionStatus.Success;
                }
                else
                {
                    payment.BraintreeStatus = ApplicationCore.Entities.TransactionStatus.Failure;
                }

                string statusText = payment.BraintreeStatus == ApplicationCore.Entities.TransactionStatus.Success ? Success : Failure;
                bool arcadierSuccess = await _arcadierService.SetArcadierTransactionStatusAsync(statusText, payment);

                payment.ArcadierStatus = ApplicationCore.Entities.TransactionStatus.Success;
                if (!arcadierSuccess)
                {
                    payment.ArcadierStatus = ApplicationCore.Entities.TransactionStatus.Failure;
                }

                await _paymentService.UpdateAsync(payment);

                return RedirectToArcadier(payment.InvoiceNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return RedirectToArcadier(model.InvoiceNo);
        }

        private RedirectResult RedirectToArcadier(string invoiceNo)
        {
            string redirectUrl = _arcadierService.GetRedirectUrl(invoiceNo);
            return Redirect(redirectUrl);
        }
    }
}
