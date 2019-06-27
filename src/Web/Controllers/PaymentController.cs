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
                string payKey = await _paymentService.GeneratePayKeyAsync();
                var paymentDetails = new PaymentDetails(payKey, request.total, request.invoiceno, request.currency, request.gateway, request.hashkey);

                await _paymentService.AddAsync(paymentDetails);

                return payKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return string.Empty;
        }
    }
}
