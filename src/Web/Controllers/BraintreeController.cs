using Braintree;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
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
        public async Task<ActionResult<string>> Payment([FromBody] CreatePaymentViewModel model)
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
                using (var httpClient = new HttpClient())
                {
                    string url = "https://eventbooking.sandbox.arcadier.io/user/checkout/transaction-status";
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(url, new
                    {
                        invoiceno = paymentDetails.InvoiceNo,
                        hashkey = paymentDetails.Hashkey,
                        gateway = paymentDetails.Gateway,
                        paykey = paymentDetails.PayKey,
                        status = "success"
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        return Redirect($"https://eventbooking.sandbox.arcadier.io/user/checkout/current-status?invoiceNo={paymentDetails.InvoiceNo}");
                        //return Ok(result.Target.Id);
                    }

                    return StatusCode(500, "Error");
                }
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
