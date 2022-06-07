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
            var servicePrincipalKey = Environment.GetEnvironmentVariable("SERVICE_PRINCIPAL_KEY");
            var base64EncodedAccessKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
            if (base64EncodedAccessKey == null)
            {
                throw new InvalidOperationException("Cannot continue due to missing access key.");
            }
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedAccessKey));
            var accessKey = JsonConvert.DeserializeObject<AccessKey>(decoded);
            var repositoryId = Environment.GetEnvironmentVariable("REPOSITORY_ID");

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
