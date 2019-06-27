using ApplicationCore.Common;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
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

        public async Task<string> GeneratePayKeyAsync()
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

        public async Task<PaymentDetails> GetByPayKeyAsync(string payKey)
        {
            Contract.Require(!string.IsNullOrWhiteSpace(payKey), "Paykey is null.");

            return await _paymentRepository.GetByPayKeyAsync(payKey);
        }

        public async Task AddAsync(PaymentDetails payment)
        {
            Contract.Require(payment != null, "Payment is null.");

            await _paymentRepository.AddAsync(payment);
        }

        public async Task UpdateAsync(PaymentDetails payment)
        {
            Contract.Require(payment != null, "Payment is null.");

            await _paymentRepository.UpdateAsync(payment);
        }
    }
}
