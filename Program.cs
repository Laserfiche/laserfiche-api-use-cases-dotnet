using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        private const int rootFolderEntryId = 1;
        private const string sampleProjectEdocName = ".Net Sample Project ImportDocument";
        public static async Task Main()
        {
            try
            {
                // Get credentials
                var config = new ServiceConfig(".env");

                // Create the client
                IRepositoryApiClient client;

                // Scope(s) requested by the app
                string scope = "repository.Read,repository.Write";
                if (config.AuthorizationType == AuthorizationType.CLOUD_ACCESS_KEY)
                {
                    client = RepositoryApiClient.CreateFromAccessKey(config.ServicePrincipalKey, config.AccessKey, scope);
                }
                else if (config.AuthorizationType == AuthorizationType.API_SERVER_USERNAME_PASSWORD)
                {
                    client = RepositoryApiClient.CreateFromUsernamePassword(config.RepositoryId, config.Username, config.Password, config.BaseUrl);
                }
                else
                {
                    Console.WriteLine($"Invalid value for '{ServiceConfig.AUTHORIZATION_TYPE}'. It can only be '{nameof(AuthorizationType.CLOUD_ACCESS_KEY)}' or '{nameof(AuthorizationType.API_SERVER_USERNAME_PASSWORD)}'.");
                    return;
                }

                // Get the name of the first repository
                string repositoryName = await GetRepositoryName(client);

                // Get root entry
                Entry root = await GetFolder(client, config.RepositoryId, rootFolderEntryId);

                // Get folder children
                ICollection<Entry> children = await GetFolderChildren(client, config.RepositoryId, root.Id);

                // Creates a sample project folder
                Entry createFolder = await CreateFolder(client, config.RepositoryId);

                // Imports a document inside the sample project folder
                int tempEdocEntryId = await ImportDocument(client,config.RepositoryId, createFolder.Id, sampleProjectEdocName);

                // Set Entry Fields
                Entry setEntryFields = await SetEntryFields(client, config.RepositoryId, createFolder.Id);

                // Print root folder name
                Entry sampleProjectRootFolder = await GetFolder(client, config.RepositoryId, createFolder.Id);

                // Print root folder children
                ICollection<Entry> sampleProjectRootFolderChildren = await GetFolderChildren(client, config.RepositoryId, sampleProjectRootFolder.Id);

                // Print entry fields
                ODataValueContextOfIListOfFieldValue entryFields = await GetEntryFields(client,config.RepositoryId, setEntryFields.Id);

                // Print Edoc Information
                HttpResponseHead entryContentType = await GetEntryContentType(client, config.RepositoryId, tempEdocEntryId);

                // Search for the imported document inside the sample project folder
                await SearchForImportedDocument(client, config.RepositoryId, sampleProjectEdocName);

                // Deletes sample project folder and its contents inside it
                await DeleteSampleProjectFolder(client, config.RepositoryId, createFolder.Id);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
            }
        }

        public static async Task<string> GetRepositoryName(IRepositoryApiClient client)
        {
            var repositoryInfoCollection = await client.RepositoriesClient.GetRepositoryListAsync();
            var firstRepository = repositoryInfoCollection.FirstOrDefault();
            if (firstRepository != null)
            {
                Console.WriteLine($"Repository Name: '{firstRepository.RepoName} [{firstRepository.RepoId}]'");
            }
            return firstRepository?.RepoName;
        }

        public static async Task<Entry> GetFolder(IRepositoryApiClient client, string repoId, int folderEntryId)
        {
            var entry = await client.EntriesClient.GetEntryAsync(repoId, folderEntryId);
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

        public static async Task<Entry> CreateFolder(IRepositoryApiClient client, string repoId)
        {
            const string newEntryName = ".Net sample project folder";
            PostEntryChildrenRequest request = new PostEntryChildrenRequest();
            request.EntryType = PostEntryChildrenEntryType.Folder;
            request.Name = newEntryName;
            Console.WriteLine("\nCreating sample project folder...");
            Entry result = await client.EntriesClient.CreateOrCopyEntryAsync(repoId, rootFolderEntryId, request, true);
            return result;
        }

        public static async Task<int> ImportDocument(IRepositoryApiClient client, string repoId, int folderEntryId, string sampleProjectFileName)
        {
            int parentEntryId = folderEntryId;
            string fileName = sampleProjectFileName;
            var electronicDocument = GetFileParameter();
            var request = new PostEntryWithEdocMetadataRequest();
            Console.WriteLine("\nImporting a document into the sample project folder...");
            var result = await client.EntriesClient.ImportDocumentAsync(repoId, parentEntryId, fileName, autoRename: true, electronicDocument: electronicDocument, request: request).ConfigureAwait(false);
            int edocEntryId = result.Operations.EntryCreate.EntryId;
            return edocEntryId;
        }

        public static async Task<Entry> SetEntryFields(IRepositoryApiClient client, string repoId, int sampleProjectFolderEntryId)
        {
            WFieldInfo field = null;
            const string fieldValue = ".Net sample project set entry value";
            ODataValueContextOfIListOfWFieldInfo fieldDefinitionsResponse = await client.FieldDefinitionsClient.GetFieldDefinitionsAsync(repoId);
            WFieldInfo[] fieldDefinitions = fieldDefinitionsResponse.Value.ToArray();
            for (int i = 0; i < fieldDefinitions.Length; i++) {
            if (
              fieldDefinitions[i].FieldType == WFieldType.String &&
              (fieldDefinitions[i].Constraint == "" || fieldDefinitions[i].Constraint == null) &&
              (fieldDefinitions[i].Length >= 1)
            ) {
              field = fieldDefinitions[i];
              break;
            }
          }
          if (field?.Name == null) {
            throw new Exception("field is undefined");
          }
          var requestBody = new Dictionary<string, FieldToUpdate>()
          {
              [field.Name] = new FieldToUpdate()
              {
                  Values = new List<ValueToUpdate>()
                  {
                      new ValueToUpdate() { Value = fieldValue, Position = 1 }
                  }
              }
          };
          Entry entry = await CreateEntry(client, repoId, entryName: ".Net Sample Project SetFields", sampleProjectFolderEntryId);
          int num = entry.Id;
          Console.WriteLine("\nSetting Entry Fields in the sample project folder...\n");
          await client.EntriesClient.AssignFieldValuesAsync(repoId, num, requestBody);
          return entry;
        }

        public static async Task<ODataValueContextOfIListOfFieldValue> GetEntryFields(IRepositoryApiClient client, string repoId, int setFieldsEntryId)
        {
            ODataValueContextOfIListOfFieldValue entryFieldResponse = await client.EntriesClient.GetFieldValuesAsync(repoId, setFieldsEntryId);
            FieldValue[] fieldDefinitions = entryFieldResponse.Value.ToArray();
            Console.WriteLine($"Entry Field Name: {fieldDefinitions[0].FieldName}");
            Console.WriteLine($"Entry Field Type: {fieldDefinitions[0].FieldType}");
            Console.WriteLine($"Entry Field ID: {fieldDefinitions[0].FieldId}");
            Console.WriteLine($"Entry Field Value: {fieldDefinitions[0].Values.First()["value"]}");
            return entryFieldResponse;
        }

        public static async Task<HttpResponseHead> GetEntryContentType(IRepositoryApiClient client, string repoId, int tempEdocEntryId)
        {
            HttpResponseHead documentContentTypeResponse = await client.EntriesClient.GetDocumentContentTypeAsync(repoId, tempEdocEntryId);
            Console.WriteLine($"Electronic Document Content Type: {documentContentTypeResponse.Headers["Content-Type"].ElementAt(0)}");
            Console.WriteLine($"Electronic Document Content Length: {documentContentTypeResponse.Headers["Content-Length"].ElementAt(0)}");
            return documentContentTypeResponse;
        }

        public static async Task SearchForImportedDocument(IRepositoryApiClient client, string repoId, string sampleProjectFileName)
        {
            SimpleSearchRequest searchRequest = new SimpleSearchRequest();
            searchRequest.SearchCommand = "({LF:Basic ~= \"" + sampleProjectFileName + "\", option=\"DFANLT\"})";
            Console.WriteLine("\nSearching for imported document...");
            ODataValueContextOfIListOfEntry simpleSearchResponse = await client.SimpleSearchesClient.CreateSimpleSearchOperationAsync(repoId, request:searchRequest);
            Console.WriteLine("\nSearch Results");
            Entry[] searchResults = simpleSearchResponse.Value.ToArray();
            for (int i = 0; i < searchResults.Length; i++) {
              Entry child = searchResults[i];
              Console.WriteLine($"{i}:[{child.EntryType} id:{child.Id}] '{child.Name}'"); 
            }
        }

        public static async Task DeleteSampleProjectFolder(IRepositoryApiClient client, string repoId, int sampleProjectFolderEntryId)
        {
            Console.WriteLine("\nDeleting all sample project entries...");
            await client.EntriesClient.DeleteEntryInfoAsync(repoId, sampleProjectFolderEntryId);
            Console.WriteLine("\nDeleted all sample project entries\n");
        }

        private static async Task<Entry> CreateEntry(IRepositoryApiClient client, string repoId, string entryName, int parentEntryId, bool autoRename = true)
        {
            PostEntryChildrenRequest request = new PostEntryChildrenRequest();
            request.EntryType = PostEntryChildrenEntryType.Folder;
            request.Name = entryName;
            var newEntry = await client.EntriesClient.CreateOrCopyEntryAsync(repoId, parentEntryId, request, autoRename);
            return newEntry;
        }

        private static FileParameter GetFileParameter()
        {
            Stream fileStream = null;
            string fileLocation = @"TestFiles/test.pdf";
            fileStream = File.OpenRead(fileLocation);
            return new FileParameter(fileStream, "test", "application/pdf");
        }
    }
}
