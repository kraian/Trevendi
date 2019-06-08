using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using Web.Models;

namespace Web.Database
{
    public class AppDbContext
    {
        private readonly ILogger<AppDbContext> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AppDbContext(ILogger<AppDbContext> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public PaymentDetails GetDetails(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _logger.LogWarning("Name is required.");
                return null;
            }

            string path = GetDataStorePath(name);
            if (!File.Exists(path))
            {
                _logger.LogWarning($"Cannot find payment details for file with name '{name}'.");
                return null;
            }

            string settingsValue = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<PaymentDetails>(settingsValue);
        }

        public bool SaveDetails(string name, PaymentDetails paymentDetails)
        {
            if (string.IsNullOrEmpty(name))
            {
                _logger.LogWarning("Name is required.");
                return false;
            }

            try
            {
                string path = GetDataStorePath(name);
                string output = JsonConvert.SerializeObject(paymentDetails, Formatting.None);
                File.WriteAllText(path, output);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occured while saving the details.");
            }

            return false;
        }

        private string GetDataStorePath(string name)
        {
            return $"{_hostingEnvironment.ContentRootPath}\\Database\\Data\\{name}.json";
        }
    }
}
