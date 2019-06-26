using ApplicationCore.Entities;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IPaymentService
    {
        Task<string> GeneratePayKeyAsync();
        Task<Result<PaymentDetails>> GetByPayKeyAsync(string payKey);
        Task<Result> AddAsync(PaymentDetails payment);
        Task<Result> UpdateAsync(PaymentDetails payment);
    }
}
