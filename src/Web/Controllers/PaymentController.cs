using Microsoft.AspNetCore.Mvc;
using Web.Database;
using Web.Models;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _db;

        public PaymentController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public ActionResult<string> GeneratePaykey([FromBody] PaymentRequest request)
        {
            string payKey = Utils.Utils.GenerateRandomId(10);
            var paymentDetails = new PaymentDetails
            {
                Amount = request.total,
                InvoiceNo = request.invoiceno,
                Currency = request.currency,
                PayKey = payKey,
                Gateway = request.gateway,
                Hashkey = request.hashkey
            };

            _db.SaveDetails(payKey, paymentDetails);

            return payKey;
        }
    }
}
