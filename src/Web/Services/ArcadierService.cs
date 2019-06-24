using ApplicationCore.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Web.Models;
using Web.Utils;

namespace Web.Services
{
    public class ArcadierService
    {
        private readonly ILogger<ArcadierService> _logger;
        private readonly IOptions<ArcadierSettings> _arcadierSettings;

        public ArcadierService(ILogger<ArcadierService> logger, IOptions<ArcadierSettings> arcadierSettings)
        {
            _logger = logger;
            _arcadierSettings = arcadierSettings;
        }

        public async Task<bool> SetArcadierTransactionStatusAsync(string status, PaymentDetails paymentDetails)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = UrlHelper.GetTransactionStatusUrl(_arcadierSettings.Value.MarketplaceUrl);
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(url, new
                    {
                        invoiceno = paymentDetails.InvoiceNo,
                        hashkey = paymentDetails.Hashkey,
                        gateway = paymentDetails.Gateway,
                        paykey = paymentDetails.PayKey,
                        status = status
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Error posting to {url}.");
                        return false;
                    }

                    string text = await response.Content.ReadAsStringAsync();
                    ArcadierResponse arcadierResponse = JsonConvert.DeserializeObject<ArcadierResponse>(text);
                    if (arcadierResponse == null)
                    {
                        _logger.LogWarning($"Arcadier response is null.");
                        return false;
                    }

                    if (!arcadierResponse.Success)
                    {
                        _logger.LogWarning(arcadierResponse.Error);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occured while calling Arcadier to update the invoice.");
                return false;
            }
        }

        public SplitAmount GetSplitAmount(decimal totalAmount)
        {
            decimal adminFee = totalAmount * _arcadierSettings.Value.Commission / 100;
            decimal merchantAmount = totalAmount - adminFee;
            return new SplitAmount(merchantAmount, adminFee);
        }

        public string GetRedirectUrl(string invoiceNo)
        {
            return UrlHelper.GetCurrentStatusUrl(_arcadierSettings.Value.MarketplaceUrl, invoiceNo);
        }
    }
}
