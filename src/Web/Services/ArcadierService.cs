using ApplicationCore.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Web.Configuration;
using Web.Interfaces;
using Web.Models;
using Web.Utils;

namespace Web.Services
{
    public class ArcadierService : IArcadierService
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

        //public async Task<object> GetOrder(string invoiceNo)
        //{
        //    try
        //    {
        //        ArcadierToken token = await GetToken();
        //        if (token == null)
        //        {
        //            return null;
        //        }

        //        using (var httpClient = new HttpClient())
        //        {
        //            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Token}");
        //            string url = $"{_arcadierSettings.Value.MarketplaceUrl}/api/v2/merchants/{token.UserId}/transactions/{invoiceNo}";

        //            HttpResponseMessage response = await httpClient.GetAsync(url);
        //            if (!response.IsSuccessStatusCode)
        //            {
        //                _logger.LogWarning("An error has occured while reading the token from Arcadier.");
        //                return null;
        //            }

        //            string text = await response.Content.ReadAsStringAsync();
        //            //return JsonConvert.DeserializeObject<ArcadierToken>(text);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //    }

        //    return null;
        //}

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

        //private async Task<ArcadierToken> GetToken()
        //{
        //    try
        //    {
        //        using (var httpClient = new HttpClient())
        //        {
        //            string url = $"{_arcadierSettings.Value.MarketplaceUrl}/token";
        //            var content = new FormUrlEncodedContent(new[]
        //            {
        //                new KeyValuePair<string, string>("client_id", "WdibwumTdxHJcG4TiADRtTnhmYdL02vdoB5X1zDu"),
        //                new KeyValuePair<string, string>("client_secret", "KfBuezssxy_q5d0rMt0590vj2wwnHos2G9zmen2bMofrzGN5TWJ"),
        //                new KeyValuePair<string, string>("grant_type", "client_credentials"),
        //                new KeyValuePair<string, string>("scope", "admin")
        //            });

        //            HttpResponseMessage response = await httpClient.PostAsync(url, content);
        //            if (!response.IsSuccessStatusCode)
        //            {
        //                _logger.LogWarning("An error has occured while reading the token from Arcadier.");
        //                return null;
        //            }

        //            string text = await response.Content.ReadAsStringAsync();
        //            return JsonConvert.DeserializeObject<ArcadierToken>(text);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //    }

        //    return null;
        //}
    }
}
