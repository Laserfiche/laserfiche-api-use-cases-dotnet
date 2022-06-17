using Laserfiche.Api.Client.OAuth;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    internal class ServiceConfig
    {
        public string RepositoryId { get; set; }

        public string ServicePrincipalKey { get; set; }

        public AccessKey AccessKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public string Organization { get; set; }
        public string AuthorizationType { get; set; }

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

            Username = Environment.GetEnvironmentVariable("LFDS_USERNAME");
            Password = Environment.GetEnvironmentVariable("LFDS_PASSWORD");
            BaseUrl = Environment.GetEnvironmentVariable("SELFHOSTED_REPOSITORY_API_BASE_URI");
            Organization = Environment.GetEnvironmentVariable("LFDS_ORGANIZATION");
            AuthorizationType = Environment.GetEnvironmentVariable("AUTHORIZATION_TYPE");
        }
    }
}
