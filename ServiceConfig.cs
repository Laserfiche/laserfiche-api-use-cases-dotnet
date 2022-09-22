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
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public string TestEnvironment { get; set; }

        public ServiceConfig(string filename)
        {
            // Read credentials from file system
            var readConfigFileOk = Utils.LoadFromDotEnv(filename);
            if (!readConfigFileOk)
            {
                Console.WriteLine("Failed to read credentials.");
            }

            // Read credentials from envrionment
            TestEnvironment = Environment.GetEnvironmentVariable("API_ENVIRONMENT_UNDER_TEST");
            if (string.IsNullOrEmpty(TestEnvironment))
            {
                throw new InvalidOperationException("Environment variable 'API_ENVIRONMENT_UNDER_TEST' does not exist. It must be present and its value can only be 'CloudClientCredentials' or 'APIServerUsernamePassword'.");
            }

            RepositoryId = Environment.GetEnvironmentVariable("REPOSITORY_ID");

            if (TestEnvironment.Equals("CloudClientCredentials", StringComparison.OrdinalIgnoreCase))
            {
                ServicePrincipalKey = Environment.GetEnvironmentVariable("SERVICE_PRINCIPAL_KEY");

                var base64EncodedAccessKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
                if (base64EncodedAccessKey == null)
                {
                    throw new InvalidOperationException("Cannot continue due to missing access key.");
                }
                AccessKey = AccessKey.CreateFromBase64EncodedAccessKey(base64EncodedAccessKey);
            }
            else if (TestEnvironment.Equals("APIServerUsernamePassword", StringComparison.OrdinalIgnoreCase))
            {
                Username = Environment.GetEnvironmentVariable("APISERVER_USERNAME");
                Password = Environment.GetEnvironmentVariable("APISERVER_PASSWORD");
                BaseUrl = Environment.GetEnvironmentVariable("APISERVER_REPOSITORY_API_BASE_URL");
                TestEnvironment = Environment.GetEnvironmentVariable("API_ENVIRONMENT_UNDER_TEST");
            }
        }
    }
}
