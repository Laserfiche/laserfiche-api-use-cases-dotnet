using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        public static async Task Main()
        {
            // Get credentials
            var config = new ServiceConfig("TestConfig.env");

            // Create the client
            IRepositoryApiClient client;

            if (config.AuthorizationType.Equals("LfdsUsernamePassword", StringComparison.OrdinalIgnoreCase))
            {
                client = RepositoryApiClient.CreateFromLfdsUsernamePassword(config.Username, config.Password, config.Organization, config.RepositoryId, config.BaseUrl);
            }
            else if (config.AuthorizationType.Equals("AccessKey", StringComparison.OrdinalIgnoreCase))
            {
                client = RepositoryApiClient.CreateFromAccessKey(config.ServicePrincipalKey, config.AccessKey);
            }
            else
            {
                Trace.TraceWarning("Invalid value for authorization type.");
                return;
            }

            // Get a list of repository names (currently not available)
            var repoNames = await GetRepoNames(client);

            // Get root entry
            var root = await GetRootFolder(client, config.RepositoryId);

            // Get folder children
            var children = await GetFolderChildren(client, config.RepositoryId, root.Id);

            // Report results
            Console.WriteLine("Repositories:");
            foreach (var repoName in repoNames)
            {
                Console.WriteLine($"  {repoName}");
            }

            Console.WriteLine($"Number of children of root: {children.Count}");
            foreach (var child in children)
            {
                Console.WriteLine($"Child name: ${child.Name}\nChild type: ${child.EntryType}\n");
            }
        }

        public static async Task<List<string>> GetRepoNames(IRepositoryApiClient client)
        {
            var repoInfoCollection = await client.RepositoriesClient.GetRepositoryListAsync();
            var repoNames = new List<string>();
            foreach (var repoInfo in repoInfoCollection)
            {
                repoNames.Add(repoInfo.RepoName);
            }
            return repoNames;
        }

        public static async Task<Entry> GetRootFolder(IRepositoryApiClient client, string repoId)
        {
            return await client.EntriesClient.GetEntryAsync(repoId, 1);
        }

        public static async Task<ICollection<Entry>> GetFolderChildren(IRepositoryApiClient client, string repoId, int entryId)
        {
            var children = await client.EntriesClient.GetEntryListingAsync(repoId, entryId);
            return children.Value;
        }
    }
}
