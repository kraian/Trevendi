using Braintree;
using Microsoft.AspNetCore.Mvc;
using Web.Braintree;
using Web.Database;
using Web.Models;

namespace Web.Controllers
{
    public class BraintreeController : Controller
    {
        private readonly IBraintreeConfig _braintreeConfig;
        private readonly AppDbContext _db;

        public BraintreeController(IBraintreeConfig braintreeConfig, AppDbContext db)
        {
            _braintreeConfig = braintreeConfig;
            _db = db;
        }

        [HttpGet]
        public IActionResult Payment(string invoiceNo, string paykey)
        {
            GenericPayment paymentDetails = _db.GetDetails(paykey);
            if (paymentDetails != null)
            {
                IBraintreeGateway gateway = _braintreeConfig.GetGateway();

                var model = new PaymentViewModel
                {
                    InvoiceNo = paymentDetails.InvoiceNo,
                    PayKey = paykey,
                    Amount = paymentDetails.Amount,
                    ClientToken = gateway.ClientToken.Generate()
                };

                return View(model);
            }

            return null;
        }

        [HttpPost]
        public ActionResult<string> Payment([FromBody] CreatePaymentViewModel model)
        {
            GenericPayment paymentDetails = _db.GetDetails(model.PayKey);
            if (paymentDetails == null)
            {
                return BadRequest("Cannot find payment details.");
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

            Result<Transaction> result = gateway.Transaction.Sale(request);
            if (result.IsSuccess())
            {
                // post to https://eventbooking.sandbox.arcadier.io/user/checkout/transaction-status
                // redirect to https://eventbooking.sandbox.arcadier.io/user/checkout/current-status?invoiceNo={invoiceNo}
                return Ok(result.Target.Id);
            }
            else if (result.Transaction != null)
            {
                return Ok(result.Transaction.Id);
            }
            else
            {
                string errorMessages = "";
                foreach (ValidationError error in result.Errors.DeepAll())
                {
                    errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                }

                return StatusCode(500, errorMessages);
            }
        }
    }
}
