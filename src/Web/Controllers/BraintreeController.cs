using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Braintree;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Web.Adapters;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class BraintreeController : Controller
    {
        private const string Success = "success";
        private const string Failure = "failed";
        private const string PaymentsFolderName = "Payments";

        private readonly ILogger<BraintreeController> _logger;
        private readonly IBraintreeGateway _braintreeGateway;
        private readonly IPaymentService _paymentService;
        private readonly ArcadierService _arcadierService;
        private readonly DriveServiceAdapter _driveServiceAdapter;

        public BraintreeController(
            ILogger<BraintreeController> logger,
            IBraintreeGateway braintreeGateway,
            IPaymentService paymentService,
            ArcadierService arcadierService,
            DriveServiceAdapter driveServiceAdapter)
        {
            _logger = logger;
            _braintreeGateway = braintreeGateway;
            _paymentService = paymentService;
            _arcadierService = arcadierService;
            _driveServiceAdapter = driveServiceAdapter;
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

                if (transactionResult.IsSuccess())
                {
                    SplitAmount splitAmount = _arcadierService.GetSplitAmount(payment.Amount);
                    await GenerateAndUploadPaymentFileAsync(payment.InvoiceNo, payment.PayKey, splitAmount.MerchantAmount, payment.Currency, "marianciortea86@gmail.com");

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

        private string GeneratePaymentFile(string invoiceNo, string payKey, decimal amount, string currency)
        {
            var records = new List<RevolutPaymentFile>
            {
                new RevolutPaymentFile
                {
                    Name = "Marian",
                    RecipientType = "type",
                    IBAN = "5555555555554444",
                    BIC = "234234534",
                    RecipientBankCountry = "Romania",
                    Currency = currency,
                    Amount = amount,
                    PaymentReference = "Payment",
                    RecipientCountry = "Romania",
                    AddressLine1 = "34 af",
                    AddressLine2 = "ddd dd",
                    City = "Timisoara",
                    PostalCode = "3434"
                }
            };


            if (!Directory.Exists(PaymentsFolderName))
            {
                Directory.CreateDirectory(PaymentsFolderName);
            }

            string filename = $"{invoiceNo}-{payKey}.csv";
            string filePath = Path.Combine(PaymentsFolderName, filename);
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(records);
            }

            return filePath;
        }

        private async Task GenerateAndUploadPaymentFileAsync(string invoiceNo, string payKey, decimal amount, string currency, string shareWithEmail)
        {
            string filePath = GeneratePaymentFile(invoiceNo, payKey, amount, currency);
            string folderId = await _driveServiceAdapter.GetFolderIdByNameAsync(PaymentsFolderName);

            if (!string.IsNullOrWhiteSpace(shareWithEmail))
            {
                bool alreadyShared = await _driveServiceAdapter.AlreadyShared(folderId);
                if (!alreadyShared)
                {
                    await _driveServiceAdapter.ShareFile(folderId, shareWithEmail);
                }
            }

            string fileId = await _driveServiceAdapter.UploadFileToFolder(filePath, folderId, "text/csv");
            if (!string.IsNullOrWhiteSpace(fileId))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
