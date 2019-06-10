using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Database;
using Web.Models;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly AppDbContext _db;

        public PaymentController(ILogger<PaymentController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public ActionResult<string> GeneratePaykey([FromBody] PaymentRequest request)
        {
            string payKey = Utils.Utils.GenerateRandomId(10);

            int collisions = 0;
            while (_db.Exists(payKey))
            {
                payKey = Utils.Utils.GenerateRandomId(10);
                collisions++;
            }

            if (collisions > 0)
            {
                _logger.LogWarning($"Generate paykey with {collisions} collisions.");
            }

            var paymentDetails = new PaymentDetails
            {
                Amount = request.total,
                InvoiceNo = request.invoiceno,
                Currency = request.currency,
                PayKey = payKey,
                Gateway = request.gateway,
                Hashkey = request.hashkey,
                PaymentStatus = PaymentStatus.New
            };

            _db.SaveDetails(payKey, paymentDetails);

            return payKey;
        }
    }
}
