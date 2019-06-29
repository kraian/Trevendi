using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
            try
            {
                PayKeyResult payKeyResult = await _paymentService.GeneratePayKeyAsync();
                if (payKeyResult.Collisions > 0)
                {
                    _logger.LogWarning($"There were {payKeyResult.Collisions} collisions while generating paykey.");
                }

                var paymentDetails = new PaymentDetails(payKeyResult.Value, request.total, request.invoiceno, request.currency, request.gateway, request.hashkey);

                await _paymentService.AddAsync(paymentDetails);

                return payKeyResult.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return string.Empty;
        }
    }
}
