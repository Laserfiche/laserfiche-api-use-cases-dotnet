using Laserfiche.Api.Client.OAuth;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        public static async Task Main()
        {
            // Read credentials from file system
            var readConfigFileOk = Utils.LoadFromDotEnv("TestConfig.env");
            if (!readConfigFileOk)
            {
                Trace.TraceWarning("Failed to read credentials.");
                return;
            }

            // Read credentials from envrionment
            var servicePrincipalKey = Environment.GetEnvironmentVariable("DEV_CA_PUBLIC_USE_TESTOAUTHSERVICEPRINCIPAL_SERVICE_PRINCIPAL_KEY");
            var base64EncodedAccessKey = Environment.GetEnvironmentVariable("DEV_CA_PUBLIC_USE_INTEGRATION_TEST_ACCESS_KEY");
            if (base64EncodedAccessKey != null)
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedAccessKey));
                Environment.SetEnvironmentVariable("DEV_CA_PUBLIC_USE_INTEGRATION_TEST_ACCESS_KEY", decoded);
            } else
            {
                throw new InvalidOperationException("Cannot continue due to missing access key.");
            }
            var accessKey = JsonConvert.DeserializeObject<AccessKey>(Environment.GetEnvironmentVariable("DEV_CA_PUBLIC_USE_INTEGRATION_TEST_ACCESS_KEY"));
            var repositoryId = Environment.GetEnvironmentVariable("DEV_CA_PUBLIC_USE_REPOSITORY_ID_1");

            // Create the client
            var repoClient = RepositoryApiClient.Create(servicePrincipalKey, accessKey);

            var rootEntryId = 1;
            var entryListing = await repoClient.EntriesClient.GetEntryListingAsync(repositoryId, rootEntryId);

            foreach (var entry in entryListing.Value)
            {
                // Do something with the returned data.
                Console.WriteLine(entry.Name);
            }
        }
    }
}
