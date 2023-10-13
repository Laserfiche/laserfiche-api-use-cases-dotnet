using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        private const int rootFolderEntryId = 1;
        private const string sampleProjectDocumentName = ".Net Sample Document";
        public static async Task Main()
        {
            // Get credentials
            var config = new ServiceConfig(".env");

            // Create the client
            IRepositoryApiClient client;

            // Scope(s) requested by the app
            string scope = "repository.Read repository.Write";
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
            Entry sampleFolderEntry = null;
            try
            {
                await PrintAllRepositoryNames(client);

                Entry root = await GetFolder(client, config.RepositoryId, rootFolderEntryId);

                await PrintFolderChildrenInformation(client, config.RepositoryId, root.Id);

                sampleFolderEntry = await CreateSampleProjectFolder(client, config.RepositoryId);

                int importedEntryId = await ImportDocument(client,config.RepositoryId, sampleFolderEntry.Id, sampleProjectDocumentName);

                await SetEntryFields(client, config.RepositoryId, sampleFolderEntry.Id);

                Entry sampleProjectRootFolder = await GetFolder(client, config.RepositoryId, sampleFolderEntry.Id);

                await PrintFolderChildrenInformation(client, config.RepositoryId, sampleProjectRootFolder.Id);

                await PrintEntryFields(client,config.RepositoryId, sampleFolderEntry.Id);

                await PrintEntryContentType(client, config.RepositoryId, importedEntryId);

                await SearchForImportedDocument(client, config.RepositoryId, sampleProjectDocumentName);

            }
            catch (Exception e)
            {
                Console.Error.Write(e);
            }
            finally
            {
                if (sampleFolderEntry != null)
                {
                    await DeleteSampleProjectFolder(client, config.RepositoryId, sampleFolderEntry.Id);
                }
            }
        }

        public static async Task PrintAllRepositoryNames(IRepositoryApiClient client)
        {
            var collectionResponse = await client.RepositoriesClient.ListRepositoriesAsync().ConfigureAwait(false);
            foreach (var repository in collectionResponse.Value)
            {
                Console.WriteLine($"Repository Name: '{repository.Name}' Repository ID: {repository.Id}");
            }
        }

        public static async Task<Entry> GetFolder(IRepositoryApiClient client, string repositoryId, int folderEntryId)
        {
            var entry = await client.EntriesClient.GetEntryAsync(new GetEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = folderEntryId
            }).ConfigureAwait(false);
            Console.WriteLine($"\nRoot Folder Path: '{entry.FullPath}'");
            return entry;
        }

        public static async Task PrintFolderChildrenInformation(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            var collectionResponse = await client.EntriesClient.ListEntriesAsync(new ListEntriesParameters()
            {
                RepositoryId = repositoryId,
                EntryId = entryId,
                Orderby = "name",
                GroupByEntryType = true
            }).ConfigureAwait(false);
            var children = collectionResponse.Value;
            Console.WriteLine($"\nNumber of entries returned: {children.Count}");
            foreach (var child in children)
            {
                Console.WriteLine($"Child Name: '{child.Name}' Child ID: {child.Id} Child Type: {child.EntryType}");
            }
        }

        public static async Task<Entry> CreateSampleProjectFolder(IRepositoryApiClient client, string repositoryId)
        {
            const string newEntryName = ".Net sample project folder";
            CreateEntryRequest request = new CreateEntryRequest();
            request.EntryType = CreateEntryRequestEntryType.Folder;
            request.Name = newEntryName;
            request.AutoRename = true;
            Console.WriteLine("\nCreating sample project folder...");
            Entry newEntry = await client.EntriesClient.CreateEntryAsync(new CreateEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = rootFolderEntryId,
                Request = request
            }).ConfigureAwait(false);
            Console.WriteLine($"Done! Entry Id: {newEntry.Id}");

            return newEntry;
        }

        public static async Task<int> ImportDocument(IRepositoryApiClient client, string repositoryId, int folderEntryId, string sampleProjectFileName)
        {
            int parentEntryId = folderEntryId;
            string fileName = sampleProjectFileName;
            var electronicDocument = GetFileParameter();
            var request = new ImportEntryRequest();
            request.Name = fileName;
            request.AutoRename = true;
            Console.WriteLine("\nImporting a document into the sample project folder...");

            var newEntry = await client.EntriesClient.ImportEntryAsync(new ImportEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = parentEntryId,
                File = electronicDocument,
                Request = request
            }).ConfigureAwait(false);
            Console.WriteLine($"Done! Entry Id: {newEntry.Id}");
            return newEntry.Id;
        }

        public static async Task SetEntryFields(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            FieldDefinition field = null;
            const string fieldValue = ".Net sample project set entry value";
            var collectionResponse = await client.FieldDefinitionsClient.ListFieldDefinitionsAsync(new ListFieldDefinitionsParameters()
            {
                RepositoryId = repositoryId
            }).ConfigureAwait(false);

            var fieldDefinitions = collectionResponse.Value;
            for (int i = 0; i < fieldDefinitions.Count; i++) {
                if (fieldDefinitions[i].FieldType == FieldType.String &&
                  (fieldDefinitions[i].Constraint == "" || fieldDefinitions[i].Constraint == null) &&
                  (fieldDefinitions[i].Length >= 1)) {
                    field = fieldDefinitions[i];
                    break;
                }
            }
            if (field?.Name == null) {
                Console.WriteLine("No field is available.");
            }
            var request = new SetFieldsRequest()
            {
                Fields = new List<FieldToUpdate>
                {
                    new FieldToUpdate()
                    {
                        Name = field.Name,
                        Values = new List<string> { fieldValue }
                    }
                }
            };
            Console.WriteLine("\nSetting Entry Fields in the sample project folder...");
            var fieldCollectionResponse = await client.EntriesClient.SetFieldsAsync(new SetFieldsParameters()
            {
                RepositoryId = repositoryId,
                EntryId = entryId,
                Request = request
            }).ConfigureAwait(false);
            Console.WriteLine($"Number of fields set on the entry: {fieldCollectionResponse.Value.Count}");
        }

        public static async Task PrintEntryFields(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            var collectionResponse = await client.EntriesClient.ListFieldsAsync(new ListFieldsParameters()
            {
                RepositoryId = repositoryId,
                EntryId = entryId
            }).ConfigureAwait(false);
            foreach (var field in collectionResponse.Value)
            {
                Console.WriteLine($"Field Id: {field.Id} Field Name: {field.Name} Field Type: {field.FieldType} Field Value: {string.Join(", ", field.Values)}");
            }
        }

        public static async Task PrintEntryContentType(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            ExportEntryRequest request = new ExportEntryRequest();
            request.Part = ExportEntryRequestPart.Edoc;
            int exportAuditReasonId = await GetAuditReasonIdForExport(client, repositoryId);
            if (exportAuditReasonId != -1)
            {
                request.AuditReasonId = exportAuditReasonId;
            }
            var response = await client.EntriesClient.ExportEntryAsync(new ExportEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = entryId,
                Request = request
            }).ConfigureAwait(false);
            var uri = response.Value;
            var httpClient = new HttpClient();
            using HttpResponseMessage httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (httpResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"\nElectronic Document Content Type: {httpResponse.Content.Headers.ContentType}");
            }
        }

        public static async Task SearchForImportedDocument(IRepositoryApiClient client, string repositoryId, string sampleProjectFileName)
        {
            var request = new SearchEntryRequest();
            request.SearchCommand = "({LF:Basic ~= \"" + sampleProjectFileName + "\", option=\"DFANLT\"})";
            Console.WriteLine("\nSearching for imported document...");
            var collectionResponse = await client.SimpleSearchesClient.SearchEntryAsync(new SearchEntryParameters()
            {
                RepositoryId = repositoryId,
                Request = request
            }).ConfigureAwait(false);
            Console.WriteLine("\nSearch Results:");
            var searchResults = collectionResponse.Value;
            for (int i = 0; i < searchResults.Count; i++) {
              Entry child = searchResults[i];
              Console.WriteLine($"{i+1} Entry ID: {child.Id} Entry Name: '{child.Name}' Entry Type: {child.EntryType}"); 
            }
        }

        public static async Task DeleteSampleProjectFolder(IRepositoryApiClient client, string repositoryId, int sampleProjectFolderEntryId)
        {
            Console.WriteLine("\nDeleting all sample project entries...");
            var taskResponse = await client.EntriesClient.StartDeleteEntryAsync(new StartDeleteEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = sampleProjectFolderEntryId,
            }).ConfigureAwait(false);
            var taskId = taskResponse.TaskId;
            Console.WriteLine($"Task ID: {taskId}");
            var collectionResponse = await client.TasksClient.ListTasksAsync(new ListTasksParameters()
            {
                RepositoryId = repositoryId,
                TaskIds = new List<string> { taskId }
            });
            var taskProgress = collectionResponse.Value[0];
            Console.WriteLine($"Task Status: {taskProgress.Status}");
        }

        private static FileParameter GetFileParameter()
        {
            string fileLocation = @"TestFiles/test.pdf";
            Stream fileStream = File.OpenRead(fileLocation);
            return new FileParameter(fileStream, "test", "application/pdf");
        }

        private async static Task<int> GetAuditReasonIdForExport(IRepositoryApiClient client, string repositoryId)
        {
            var collectionResponse = await client.AuditReasonsClient.ListAuditReasonsAsync(new ListAuditReasonsParameters()
            { 
                RepositoryId = repositoryId
            }).ConfigureAwait(false);
            var exportAuditReason = collectionResponse.Value.FirstOrDefault(auditReason => auditReason.AuditEventType == AuditEventType.ExportDocument);
            return exportAuditReason != null ? exportAuditReason.Id : -1;
        }
    }
}
