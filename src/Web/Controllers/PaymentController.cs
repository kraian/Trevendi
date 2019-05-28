using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var payment = new GenericPayment
            {
                Total = request.total,
                InvoiceNo = request.invoiceno,
                Currency = request.currency,
                PayKey = payKey,
                Gateway = request.gateway,
                Hashkey = request.hashkey
            };

            _db.SaveDetails(payKey, payment);

            return payKey;
        }

        [HttpGet]
        public ActionResult Pay(string paykey)
        {
            return null;
        }
    }
}
