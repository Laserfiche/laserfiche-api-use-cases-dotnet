using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        public static async Task Main()
        {
            try
            {
                // Get credentials
                var config = new ServiceConfig(".env");

                // Create the client
                var client = RepositoryApiClient.CreateFromAccessKey(config.ServicePrincipalKey, config.AccessKey);

                // Get a list of repository names
                var repoNames = await GetRepoNames(client);

                // Get root entry
                var root = await GetRootFolder(client, config.RepositoryId);

                // Get folder children
                var children = await GetFolderChildren(client, config.RepositoryId, root.Id);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
            }
        }

        public static async Task<List<string>> GetRepoNames(IRepositoryApiClient client)
        {
            var repoInfoCollection = await client.RepositoriesClient.GetRepositoryListAsync();
            var repoNames = new List<string>();
            Console.WriteLine("Repositories:");
            foreach (var repoInfo in repoInfoCollection)
            {
                repoNames.Add(repoInfo.RepoName);
                Console.WriteLine($"  {repoInfo.RepoName}");
            }
            return repoNames;
        }

        public static async Task<Entry> GetRootFolder(IRepositoryApiClient client, string repoId)
        {
            var entry = await client.EntriesClient.GetEntryAsync(repoId, 1);
            Console.WriteLine($"Root Folder Path: '{entry.FullPath}'");
            return entry;
        }

        public static async Task<ICollection<Entry>> GetFolderChildren(IRepositoryApiClient client, string repoId, int entryId)
        {
            var children = await client.EntriesClient.GetEntryListingAsync(repoId, entryId, orderby: "name", groupByEntryType: true);
            Console.WriteLine($"Number of entries returned: {children.Value.Count}");
            foreach (var child in children.Value)
            {
                Console.WriteLine($"Child name: {child.Name}\nChild type: {child.EntryType}\n");
            }
            return children.Value;
        }
    }
}
