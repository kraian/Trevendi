using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        [HttpPost]
        public ActionResult<string> GeneratePaykey([FromBody] PaymentRequest request)
        {
            string payKey = Utils.Utils.GenerateRandomId(10);
            var cashPayment = new GenericPayment
            {
                Total = request.total,
                InvoiceNo = request.invoiceno,
                Currency = request.currency,
                PayKey = payKey,
                Gateway = request.gateway,
                Hashkey = request.hashkey
            };

            return payKey;
        }
    }
}
