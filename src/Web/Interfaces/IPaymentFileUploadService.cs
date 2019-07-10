using System.Threading.Tasks;

namespace Web.Interfaces
{
    public interface IPaymentFileUploadService
    {
        Task UploadAsync(string invoiceNo, string payKey, decimal amount, string currency, string shareWithEmail);
    }
}
