using System.Threading.Tasks;
using Web.Interfaces;

namespace Web.Services
{
    public class PaymentFileUploadService : IPaymentFileUploadService
    {
        private const string PaymentsFolderName = "Payments";

        private readonly IPaymentFileService _paymentFileService;
        private readonly IDriveServiceAdapter _driveServiceAdapter;

        public PaymentFileUploadService(IPaymentFileService paymentFileService, IDriveServiceAdapter driveServiceAdapter)
        {
            _paymentFileService = paymentFileService;
            _driveServiceAdapter = driveServiceAdapter;
        }

        public async Task UploadAsync(string invoiceNo, string payKey, decimal amount, string currency, string shareWithEmail)
        {
            string filePath = _paymentFileService.GenerateCsv(PaymentsFolderName, invoiceNo, payKey, amount, currency);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            string folderId = await _driveServiceAdapter.GetFolderIdByNameAsync(PaymentsFolderName);
            if (string.IsNullOrWhiteSpace(folderId))
            {
                folderId = await _driveServiceAdapter.CreateFolderAsync(PaymentsFolderName);
                if (string.IsNullOrWhiteSpace(folderId))
                {
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(shareWithEmail))
            {
                bool alreadyShared = await _driveServiceAdapter.AlreadyShared(folderId);
                if (!alreadyShared)
                {
                    await _driveServiceAdapter.ShareFile(folderId, shareWithEmail);
                }
            }

            string fileId = await _driveServiceAdapter.UploadFileToFolder(filePath, folderId, "text/csv");
            if (!string.IsNullOrWhiteSpace(fileId))
            {
                _paymentFileService.DeleteFile(filePath);
            }
        }
    }
}
