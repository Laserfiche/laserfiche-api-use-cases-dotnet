using System;
using System.Collections.Generic;
using System.Linq;
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
                IRepositoryApiClient client;

                if (config.AuthorizationType.Equals("AccessKey", StringComparison.OrdinalIgnoreCase))
                {
                    client = RepositoryApiClient.CreateFromAccessKey(config.ServicePrincipalKey, config.AccessKey);
                }
                else if (config.AuthorizationType.Equals("SelfHostedUsernamePassword", StringComparison.OrdinalIgnoreCase))
                {
                    client = RepositoryApiClient.CreateFromSelfHostedUsernamePassword(config.Username, config.Password, config.GrantType, config.RepositoryId, config.BaseUrl);
                }
                else
                {
                    Console.WriteLine("Invalid value for 'AUTHORIZATION_TYPE'. It can only be 'AccessKey' or 'SelfHostedUsernamePassword'.");
                    return;
                }

                // Get the name of the first repository
                var repoNames = await GetRepositoryName(client);

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

        public static async Task<string> GetRepositoryName(IRepositoryApiClient client)
        {
            var repositoryInfoCollection = await client.RepositoriesClient.GetRepositoryListAsync();
            var firstRepository = repositoryInfoCollection.First();
            Console.WriteLine($"Repository Name: '{firstRepository.RepoName} [{firstRepository.RepoId}]'");
            return firstRepository.RepoName;
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
