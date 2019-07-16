using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Braintree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Web.Interfaces;
using Web.Models;

namespace Web.Controllers
{
    public class BraintreeController : Controller
    {
        private const string Success = "success";
        private const string Failure = "failed";
        private const string ShareEmail = "marianciortea86@gmail.com";

        private readonly ILogger<BraintreeController> _logger;
        private readonly IBraintreeGateway _braintreeGateway;
        private readonly IPaymentService _paymentService;
        private readonly IArcadierService _arcadierService;
        private readonly IPaymentFileUploadService _paymentFileUploadService;

        public BraintreeController(
            ILogger<BraintreeController> logger,
            IBraintreeGateway braintreeGateway,
            IPaymentService paymentService,
            IArcadierService arcadierService,
            IPaymentFileUploadService paymentFileUploadService)
        {
            _logger = logger;
            _braintreeGateway = braintreeGateway;
            _paymentService = paymentService;
            _arcadierService = arcadierService;
            _paymentFileUploadService = paymentFileUploadService;
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

                var request = new TransactionRequest
                {
                    Amount = payment.Amount,
                    PaymentMethodNonce = model.PaymentMethodNonce,
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                Result<Transaction> transactionResult = await _braintreeGateway.Transaction.SaleAsync(request);

                Task uploadTask = null;
                if (transactionResult.IsSuccess())
                {
                    SplitAmount splitAmount = _arcadierService.GetSplitAmount(payment.Amount);
                    uploadTask = Task.Run(() => _paymentFileUploadService.UploadAsync(payment.InvoiceNo, payment.PayKey, splitAmount.MerchantAmount, payment.Currency, ShareEmail));

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

                if (uploadTask != null)
                {
                    try
                    {
                        uploadTask.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        foreach (var exception in ae.InnerExceptions)
                        {
                            _logger.LogError(exception, exception.Message);
                        }
                    }
                }

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
