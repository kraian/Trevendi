using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.IO;
using Web.Models;

namespace Web.Database
{
    public class AppDbContext
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public AppDbContext(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public GenericPayment GetDetails(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            string path = GetDataStorePath(name);
            if (File.Exists(path))
            {
                string settingsValue = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<GenericPayment>(settingsValue);
            }

            return null;
        }

        public bool SaveDetails(string name, GenericPayment payment)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            try
            {
                string path = GetDataStorePath(name);
                string output = JsonConvert.SerializeObject(payment, Formatting.None);
                File.WriteAllText(path, output);
                return true;
            }
            catch
            {
                // log the error
            }

            return false;
        }

        private string GetDataStorePath(string name)
        {
            return $"{_hostingEnvironment.ContentRootPath}\\Database\\Data\\{name}.json";
        }
    }
}
