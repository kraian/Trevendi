using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<PaymentDetails>> GetByPayKeyAsync(string payKey)
        {
            if (string.IsNullOrWhiteSpace(payKey))
            {
                return Result.Fail<PaymentDetails>("PayKey is null.");
            }

            PaymentDetails payment = await _paymentRepository.GetByPayKeyAsync(payKey);
            return Result.Ok(payment);
        }

        public async Task<Result<string>> AddAsync(PaymentDetails payment)
        {
            if (payment == null)
            {
                return Result.Fail<string>("PayKey is null.");
            }

            payment.PayKey = await GeneratePayKeyAsync();
            await _paymentRepository.AddAsync(payment);

            return Result.Ok(payment.PayKey);
        }

        public async Task<Result> UpdateAsync(PaymentDetails payment)
        {
            if (payment == null)
            {
                return Result.Fail("Payment is null.");
            }

            await _paymentRepository.UpdateAsync(payment);
            return Result.Ok();
        }

        private async Task<string> GeneratePayKeyAsync()
        {
            string payKey = Utils.Utils.GenerateRandomId(10);
            int collisions = 0;

            PaymentDetails payment = await _paymentRepository.GetByPayKeyAsync(payKey);

            while (payment != null)
            {
                payKey = Utils.Utils.GenerateRandomId(10);
                payment = await _paymentRepository.GetByPayKeyAsync(payKey);
                collisions++;
            }

            if (collisions > 0)
            {
                //_logger.LogWarning($"Generate paykey with {collisions} collisions.");
            }

            return payKey;
        }
    }
}
