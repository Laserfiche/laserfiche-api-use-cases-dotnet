using Laserfiche.Api.Client.OAuth;
using System;

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
            AuthorizationType = Environment.GetEnvironmentVariable("AUTHORIZATION_TYPE");
            if (string.IsNullOrEmpty(AuthorizationType))
            {
                throw new InvalidOperationException("Environment variable 'AUTHORIZATION_TYPE' does not exist. It must be present and its value can only be 'CloudAccessKey' or 'APIServerUsernamePassword'.");
            }

            RepositoryId = Environment.GetEnvironmentVariable("REPOSITORY_ID");

            if (AuthorizationType.Equals("CloudAccessKey", StringComparison.OrdinalIgnoreCase))
            {
                ServicePrincipalKey = Environment.GetEnvironmentVariable("SERVICE_PRINCIPAL_KEY");

                var base64EncodedAccessKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
                if (base64EncodedAccessKey == null)
                {
                    throw new InvalidOperationException("Cannot continue due to missing access key.");
                }
                AccessKey = AccessKey.CreateFromBase64EncodedAccessKey(base64EncodedAccessKey);
            }
            else if (AuthorizationType.Equals("APIServerUsernamePassword", StringComparison.OrdinalIgnoreCase))
            {
                Username = Environment.GetEnvironmentVariable("APISERVER_USERNAME");
                Password = Environment.GetEnvironmentVariable("APISERVER_PASSWORD");
                BaseUrl = Environment.GetEnvironmentVariable("APISERVER_REPOSITORY_API_BASE_URL");
            }
        }
    }
}
