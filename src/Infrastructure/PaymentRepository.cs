using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Google.Cloud.Datastore.V1;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class PaymentRepository : AppDbContext, IPaymentRepository
    {
        public const string Kind = "Payment";

        public PaymentRepository(string projectId)
            : base(projectId)
        {
        }

        public async Task<PaymentDetails> GetByPayKeyAsync(string payKey)
        {
            Entity entity = await _db.LookupAsync(payKey.ToKey());

            if (entity != null)
            {
                return entity.ToPayment();
            }

            return null;
        }

        public async Task AddAsync(PaymentDetails payment)
        {
            await _db.UpsertAsync(payment.ToEntity());
        }

        public async Task UpdateAsync(PaymentDetails payment)
        {
            await _db.UpdateAsync(payment.ToEntity());
        }
    }
}
