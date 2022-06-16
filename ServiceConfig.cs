using Laserfiche.Api.Client.OAuth;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    internal class ServiceConfig
    {
        public string RepositoryId { get; set; }

        public string ServicePrincipalKey { get; set; }

        public AccessKey AccessKey { get; set; }

        public ServiceConfig(string filename)
        {
            // Read credentials from file system
            var readConfigFileOk = Utils.LoadFromDotEnv(filename);
            if (!readConfigFileOk)
            {
                Console.WriteLine("Failed to read credentials.");
            }

            // Read credentials from envrionment
            RepositoryId = Environment.GetEnvironmentVariable("REPOSITORY_ID");
            ServicePrincipalKey = Environment.GetEnvironmentVariable("SERVICE_PRINCIPAL_KEY");
            var base64EncodedAccessKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
            if (base64EncodedAccessKey == null)
            {
                throw new InvalidOperationException("Cannot continue due to missing access key.");
            }
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedAccessKey));
            AccessKey = JsonConvert.DeserializeObject<AccessKey>(decoded);
        }
    }
}
