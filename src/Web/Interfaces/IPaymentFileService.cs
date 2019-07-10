namespace Web.Interfaces
{
    public interface IPaymentFileService
    {
        string GenerateCsv(string paymentsFolderName, string invoiceNo, string payKey, decimal amount, string currency);
        void DeleteFile(string filePath);
    }
}
