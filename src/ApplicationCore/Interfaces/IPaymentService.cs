using ApplicationCore.Entities;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IPaymentService
    {
        Task<PayKeyResult> GeneratePayKeyAsync();
        Task<PaymentDetails> GetByPayKeyAsync(string payKey);
        Task AddAsync(PaymentDetails payment);
        Task UpdateAsync(PaymentDetails payment);
    }
}
