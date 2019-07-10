using CsvHelper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Web.Interfaces;
using Web.Models;

namespace Web.Services
{
    public class PaymentFileService : IPaymentFileService
    {
        private readonly ILogger<PaymentFileService> _logger;

        public PaymentFileService(ILogger<PaymentFileService> logger)
        {
            _logger = logger;
        }

        public string GenerateCsv(string paymentsFolderName, string invoiceNo, string payKey, decimal amount, string currency)
        {
            // Add model for parameters
            var records = new List<PaymentFile>
            {
                new PaymentFile
                {
                    Name = "Marian",
                    RecipientType = "type",
                    IBAN = "5555555555554444",
                    BIC = "234234534",
                    RecipientBankCountry = "Romania",
                    Currency = currency,
                    Amount = amount,
                    PaymentReference = "Payment",
                    RecipientCountry = "Romania",
                    AddressLine1 = "34 af",
                    AddressLine2 = "ddd dd",
                    City = "Timisoara",
                    PostalCode = "3434"
                }
            };


            if (!Directory.Exists(paymentsFolderName))
            {
                Directory.CreateDirectory(paymentsFolderName);
            }

            string filename = $"{invoiceNo}-{payKey}.csv";
            string filePath = Path.Combine(paymentsFolderName, filename);

            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(records);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return string.Empty;
        }

        public void DeleteFile(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
