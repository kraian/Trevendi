using ApplicationCore.Entities;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IPaymentRepository
    {
        Task<PaymentDetails> GetByPayKeyAsync(string payKey);
        Task AddAsync(PaymentDetails payment);
        Task UpdateAsync(PaymentDetails payment);
    }
}
