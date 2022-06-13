namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        public static async Task Main()
        {
            // Get credentials
            var config = new ServiceConfig("TestConfig.env");

            // Create the client
            var repoClient = RepositoryApiClient.Create(config.ServicePrincipalKey, config.AccessKey);

            var rootEntryId = 1;
            var entryListing = await repoClient.EntriesClient.GetEntryListingAsync(config.RepositoryId, rootEntryId);

            foreach (var entry in entryListing.Value)
            {
                // Do something with the returned data.
                Console.WriteLine(entry.Name);
            }
        }
    }
}
