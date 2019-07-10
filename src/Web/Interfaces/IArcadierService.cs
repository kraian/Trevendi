using ApplicationCore.Entities;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Interfaces
{
    public interface IArcadierService
    {
        Task<bool> SetArcadierTransactionStatusAsync(string status, PaymentDetails paymentDetails);
        SplitAmount GetSplitAmount(decimal totalAmount);
        string GetRedirectUrl(string invoiceNo);
    }
}
