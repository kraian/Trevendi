using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;

        public PaymentController(ILogger<PaymentController> logger, IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<string>> GeneratePaykey([FromBody] PaymentRequest request)
        {
            var paymentDetails = new PaymentDetails
            {
                Amount = request.total,
                InvoiceNo = request.invoiceno,
                Currency = request.currency,
                Gateway = request.gateway,
                Hashkey = request.hashkey,
                TransactionStatus = TransactionStatus.New,
                ArcadierTransactionStatus = TransactionStatus.New
            };

            var result = await _paymentService.AddAsync(paymentDetails);
            if (result.IsFailure)
            {
                _logger.LogError(result.Error);
                return BadRequest(result.Error);
            }

            return result.Value;
        }
    }
}
