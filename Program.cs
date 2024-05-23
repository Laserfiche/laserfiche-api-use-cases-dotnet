// Copyright (c) Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Laserfiche.Api.Client;
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
        private const int ROOT_ENTRY_ID = 1;
        private static readonly HttpClient _httpClient = new HttpClient();
        public static async Task Main()
        {
            ServiceConfig config = new ServiceConfig(".env");
            IRepositoryApiClient repositoryApiClient = CreateRepositoryApiClient(config);
            Entry sampleFolderEntry = null;
            try
            {
                await PrintAllRepositoryNames(repositoryApiClient);

                Entry root = await GetFolder(repositoryApiClient, config.RepositoryId, ROOT_ENTRY_ID);

                await PrintFolderChildrenInformation(repositoryApiClient, config.RepositoryId, root.Id);

                sampleFolderEntry = await CreateSampleProjectFolder(repositoryApiClient, config.RepositoryId);

                Entry importedPdfEntry = await ImportDocument(repositoryApiClient, config.RepositoryId, sampleFolderEntry.Id);

                await SetEntryFields(repositoryApiClient, config.RepositoryId, sampleFolderEntry.Id);

                await PrintEntryFields(repositoryApiClient, config.RepositoryId, sampleFolderEntry.Id);

                await ExportEntryExampleAsync(repositoryApiClient, config.RepositoryId, importedPdfEntry.Id);

                await SearchForImportedDocument(repositoryApiClient, config.RepositoryId, importedPdfEntry.Name);

                await ImportLargeDocument(repositoryApiClient, config.RepositoryId, sampleFolderEntry.Id);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
            }
            finally
            {
                if (sampleFolderEntry != null)
                {
                    await DeleteSampleProjectFolder(repositoryApiClient, config.RepositoryId, sampleFolderEntry.Id);
                }
            }
        }

        private static IRepositoryApiClient CreateRepositoryApiClient(ServiceConfig config)
        {
            string requiredScopes = "repository.Read repository.Write";
            if (config.AuthorizationType == AuthorizationType.CLOUD_ACCESS_KEY)
            {
                IRepositoryApiClient client = RepositoryApiClient.CreateFromAccessKey(config.ServicePrincipalKey, config.AccessKey, requiredScopes);
                return client;
            }
            else if (config.AuthorizationType == AuthorizationType.API_SERVER_USERNAME_PASSWORD)
            {
                IRepositoryApiClient client = RepositoryApiClient.CreateFromUsernamePassword(config.RepositoryId, config.Username, config.Password, config.BaseUrl);
                return client;
            }
            else
            {
                throw new Exception($"Invalid value for '{ServiceConfig.AUTHORIZATION_TYPE}'. It can only be '{nameof(AuthorizationType.CLOUD_ACCESS_KEY)}' or '{nameof(AuthorizationType.API_SERVER_USERNAME_PASSWORD)}'.");
            }
        }

        /**
        * Prints the information of all the available repositories.
        */
        public static async Task PrintAllRepositoryNames(IRepositoryApiClient client)
        {
            var collectionResponse = await client.RepositoriesClient.ListRepositoriesAsync();
            foreach (var repository in collectionResponse.Value)
            {
                Console.WriteLine($"Repository Name: '{repository.Name}' Repository ID: {repository.Id}");
            }
        }

        /**
         * Returns the entry for the given folder's entry Id.
         */
        public static async Task<Entry> GetFolder(IRepositoryApiClient client, string repositoryId, int folderEntryId)
        {
            var entry = await client.EntriesClient.GetEntryAsync(new GetEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = folderEntryId
            });
            Console.WriteLine($"\nRoot Folder Path: '{entry.FullPath}'");
            return entry;
        }

        /**
         * Prints the information of the child entries of the given folder's entry Id.
         */
        public static async Task PrintFolderChildrenInformation(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            int count = 0;
            await client.EntriesClient.ListEntriesForEachAsync(
               (entryCollectionResponse) =>
               {
                   foreach (var child in entryCollectionResponse.Value)
                   {
                       count++;
                       Console.WriteLine($"Child Name: '{child.Name}' Child ID: {child.Id} Child Type: {child.EntryType}");
                   }
                   return Task.FromResult(true); //True means keep fetching the next page of entries
               },

               new ListEntriesParameters()
               {
                   RepositoryId = repositoryId,
                   EntryId = entryId,
                   Orderby = "name",
                   GroupByEntryType = true
               });

            Console.WriteLine($"Folder: '{entryId}' contains {count} items.");
        }

        /**
         * Creates a sample folder in the root folder.
         */
        public static async Task<Entry> CreateSampleProjectFolder(IRepositoryApiClient client, string repositoryId)
        {
            const string newEntryName = ".Net sample project folder. CAN BE DELETED.";
            CreateEntryRequest request = new CreateEntryRequest();
            request.EntryType = CreateEntryRequestEntryType.Folder;
            request.Name = newEntryName;
            request.AutoRename = true;
            Console.WriteLine("\nCreating sample project folder...");
            Entry newEntry = await client.EntriesClient.CreateEntryAsync(new CreateEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = ROOT_ENTRY_ID,
                Request = request
            });
            Console.WriteLine($"Done! Entry Id: {newEntry.Id}");

            return newEntry;
        }

        /**
         * Imports a document into the folder specified by the given entry Id.
         */
        public static async Task<Entry> ImportDocument(IRepositoryApiClient client, string repositoryId, int parentEntryId)
        {
            string fileLocation = @"TestFiles/test.pdf";
            using Stream fileStream = File.OpenRead(fileLocation);
            Console.WriteLine("\nImporting a document into the sample project folder...");

            var newEntry = await client.EntriesClient.ImportEntryAsync(new ImportEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = parentEntryId,
                File = new FileParameter(fileStream, "file"),
                Request = new ImportEntryRequest
                {
                    Name = "newTestPdfFileName.pdf",
                    AutoRename = true,
                    ImportAsElectronicDocument = true,
                    PdfOptions = new ImportEntryRequestPdfOptions
                    {
                        GeneratePages = false,
                        GenerateText = false
                    }
                }
            });
            Console.WriteLine($"Done! Entry Id: {newEntry.Id}");
            return newEntry;
        }

        /**
         * Sets a string field on the entry specified by the given entry Id.
         */
        public static async Task SetEntryFields(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            const string fieldValue = $"DotNet SetFieldsAsync test";
            var fieldsDefinitionsCollectionResponse = await client.FieldDefinitionsClient.ListFieldDefinitionsAsync(new ListFieldDefinitionsParameters()
            {
                RepositoryId = repositoryId
            });

            FieldDefinition stringField = fieldsDefinitionsCollectionResponse.Value.FirstOrDefault(field =>
                field.FieldType == FieldType.String && string.IsNullOrEmpty(field.Constraint) && field.Length >= 1);

            if (stringField?.Name == null)
            {
                throw new Exception("No suitable field definition found.");
            }

            Console.WriteLine($"\nSetting Field '{stringField.Name}' on Entry {entryId}.");
            var fieldCollectionResponse = await client.EntriesClient.SetFieldsAsync(new SetFieldsParameters()
            {
                RepositoryId = repositoryId,
                EntryId = entryId,
                Request = new SetFieldsRequest()
                {
                    Fields = new List<FieldToUpdate>
                    {
                        new FieldToUpdate
                        {
                            Name = stringField.Name,
                            Values = new List<string> { fieldValue }
                        }
                    }
                }
            });

            Console.WriteLine($"Successfully set {fieldCollectionResponse.Value.Count} fields.");
        }

        /**
         * Prints the fields assigned to the entry specified by the given entry Id.
         */
        public static async Task PrintEntryFields(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            var collectionResponse = await client.EntriesClient.ListFieldsAsync(new ListFieldsParameters()
            {
                RepositoryId = repositoryId,
                EntryId = entryId
            });
            Console.WriteLine($"\nFields on Entry {entryId}:");
            foreach (var field in collectionResponse.Value)
            {
                Console.WriteLine($"  >Field Id: {field.Id}, Field Name: '{field.Name}', Field Type: '{field.FieldType}', Field Value: '{string.Join(", ", field.Values)}'");
            }
        }

        /**
         * Exports the electronic document part of an entry and prints its content-type.
         */
        public static async Task ExportEntryExampleAsync(IRepositoryApiClient client, string repositoryId, int entryId)
        {
            AuditReason exportDocumentAuditReason = await GetExportDocumentAuditReason(client, repositoryId);

            var response = await client.EntriesClient.ExportEntryAsync(new ExportEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = entryId,
                Request = new ExportEntryRequest
                {
                    Part = ExportEntryRequestPart.Edoc,
                    AuditReasonId = exportDocumentAuditReason?.Id ?? 0
                }
            });

            string documentDownloadUri = response.Value;
            using HttpResponseMessage httpResponse = await _httpClient.GetAsync(documentDownloadUri, HttpCompletionOption.ResponseHeadersRead);
            if (httpResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"\nExported Electronic Document '{httpResponse.Content.Headers.ContentDisposition?.FileName}', Content Type: {httpResponse.Content.Headers.ContentType}.");
            }
        }

        /**
         * Performs a simple search for the given file name, and prints out the search results.
         */
        public static async Task SearchForImportedDocument(IRepositoryApiClient client, string repositoryId, string sampleProjectFileName)
        {
            Console.WriteLine("\nSearching for imported document...");
            var collectionResponse = await client.SimpleSearchesClient.SearchEntryAsync(new SearchEntryParameters()
            {
                RepositoryId = repositoryId,
                Request = new SearchEntryRequest
                {
                    SearchCommand = "({LF:Basic ~= \"" + sampleProjectFileName + "\", option=\"DFANLT\"})"
                }
            });

            Console.WriteLine("Search Results:");
            var searchResults = collectionResponse.Value;
            for (int i = 0; i < searchResults.Count; i++)
            {
                Entry child = searchResults[i];
                Console.WriteLine($"{i + 1} Entry ID: {child.Id}, Entry Name: '{child.Name}', Entry Type: '{child.EntryType}'");
            }
        }

        /**
         * Deletes the sample project folder.
         */
        public static async Task DeleteSampleProjectFolder(IRepositoryApiClient client, string repositoryId, int sampleProjectFolderEntryId)
        {
            Console.WriteLine($"\nDeleting sample project folder: '{sampleProjectFolderEntryId}'");
            var taskResponse = await client.EntriesClient.StartDeleteEntryAsync(new StartDeleteEntryParameters()
            {
                RepositoryId = repositoryId,
                EntryId = sampleProjectFolderEntryId,
            });

            var taskId = taskResponse.TaskId;
            Console.WriteLine($"StartDeleteEntryAsync returned Task ID: {taskId}");
            TaskProgress taskProgress = null;
            while (taskProgress == null || taskProgress.Status == TaskStatus.NotStarted || taskProgress.Status == TaskStatus.InProgress)
            {
                var collectionResponse = await client.TasksClient.ListTasksAsync(new ListTasksParameters()
                {
                    RepositoryId = repositoryId,
                    TaskIds = new List<string> { taskId }
                });
                taskProgress = collectionResponse.Value.First(r => r.Id == taskId);
            }

            var errTxtx = taskProgress.Errors != null && taskProgress.Errors.Count > 0 ? (" Errors: " + Newtonsoft.Json.JsonConvert.SerializeObject(taskProgress.Errors)) : "";
            Console.WriteLine($"{taskProgress.TaskType} Status: {taskProgress.Status}.{errTxtx}");
        }

        /**
         * Searches for the audit reason for export operation, and if found, returns its Id. Otherwise, returns -1.
         */
        private static async Task<AuditReason> GetExportDocumentAuditReason(IRepositoryApiClient client, string repositoryId)
        {
            var collectionResponse = await client.AuditReasonsClient.ListAuditReasonsAsync(new ListAuditReasonsParameters()
            {
                RepositoryId = repositoryId
            });
            AuditReason exportDocumentAuditReason = collectionResponse.Value.FirstOrDefault(auditReason => auditReason.AuditEventType == AuditEventType.ExportDocument);
            return exportDocumentAuditReason;
        }

        /**
         * Uses the asynchronous import API to import a large file into the specified folder.
         */
        public static async Task ImportLargeDocument(IRepositoryApiClient client, string repositoryId, int folderEntryId)
        {
            var file = new FileInfo(@"TestFiles/sample.pdf");
            var mimeType = "application/pdf";

            // Step 1: Get upload URLs
            int parts = 2;
            int partSizeInMB = 5;
            CreateMultipartUploadUrlsRequest requestBody = new CreateMultipartUploadUrlsRequest();
            requestBody.FileName = file.Name;
            requestBody.MimeType = mimeType;
            requestBody.NumberOfParts = parts;

            Console.WriteLine("\nRequesting upload URLs...");
            var response = await client.EntriesClient.CreateMultipartUploadUrlsAsync(new CreateMultipartUploadUrlsParameters()
            {
                RepositoryId = repositoryId,
                Request = requestBody
            });

            var uploadId = response.UploadId;

            // Step 2: Write file part into upload URLs
            Console.WriteLine("Writing file parts to upload URLs...");
            var eTags = await WriteFile(file.FullName, response.Urls, partSizeInMB);

            // Step 3: Call ImportUploadedParts API
            Console.WriteLine("Starting the import task...");
            StartImportUploadedPartsRequest requestBody2 = new StartImportUploadedPartsRequest();
            requestBody2.UploadId = uploadId;
            requestBody2.AutoRename = true;
            requestBody2.PartETags = eTags;
            requestBody2.Name = file.Name;
            ImportEntryRequestPdfOptions pdfOptions = new ImportEntryRequestPdfOptions();
            pdfOptions.GeneratePages = true;
            pdfOptions.KeepPdfAfterImport = true;
            requestBody2.PdfOptions = pdfOptions;
            StartTaskResponse response2 = await client.EntriesClient.StartImportUploadedPartsAsync(new StartImportUploadedPartsParameters()
            {
                RepositoryId = repositoryId,
                EntryId = folderEntryId,
                Request = requestBody2
            });

            var taskId = response2.TaskId;
            Console.WriteLine($"Task Id: {taskId}");

            // Check/print the status of the import task.
            var collectionResponse = await client.TasksClient.ListTasksAsync(new ListTasksParameters()
            {
                RepositoryId = repositoryId,
                TaskIds = new List<string> { taskId }
            });
            var taskProgress = collectionResponse.Value[0];
            Console.WriteLine($"Task Status: {taskProgress.Status}");
            switch (taskProgress.Status)
            {
                case TaskStatus.Completed:
                    Console.WriteLine($"Entry Id: {taskProgress.Result.EntryId}");
                    break;
                case TaskStatus.Failed:
                    foreach (var problemDetails in taskProgress.Errors)
                    {
                        PrintProblemDetails(problemDetails);
                    }
                    break;
            }
        }

        /**
         * Splits the given file into parts of the given size, and writes them into the given URLs. Finally, returns the eTags of the written parts.
         */
        private static async Task<string[]> WriteFile(string filePath, IList<string> urls, int partSizeInMB)
        {
            List<string> eTags = new List<string>();
            using (FileStream fs = File.OpenRead(filePath))
            {
                int BUFFER_SIZE = partSizeInMB * 1024 * 1024;
                byte[] buffer = new byte[BUFFER_SIZE];

                int partNumber = 1;
                while (true)
                {
                    int effectiveLength = fs.Read(buffer, 0, BUFFER_SIZE);
                    var part = new byte[effectiveLength];
                    Array.Copy(buffer, part, effectiveLength);
                    var eTag = await WriteAsync(partNumber, urls[partNumber - 1], part);
                    eTags.Add(eTag);
                    if (effectiveLength != BUFFER_SIZE)
                        break;
                    partNumber++;
                }
            }
            return eTags.ToArray();
        }

        /**
         * Writes the given file part into the given URL, and returns the eTags of the written part.
         */
        private static async Task<string> WriteAsync(int partNumber, string url, byte[] part)
        {
            string eTag = null;
            Console.WriteLine($"Writing part #{partNumber} ...");
            var content = new ByteArrayContent(part);
            var httpResponse = await _httpClient.PutAsync(url, content);
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var headerFound = httpResponse.Headers.TryGetValues("ETag", out IEnumerable<string> values);
                if (headerFound)
                {
                    eTag = values.First();
                }
            }
            return eTag;
        }

        /**
         * Prints the information of the given ProblemDetails object.
         */
        private static void PrintProblemDetails(ProblemDetails problemDetails)
        {
            Console.WriteLine($"ProblemDetails: (Title: {problemDetails.Title}, Status: {problemDetails.Status}, Detail: {problemDetails.Detail}, Type: {problemDetails.Type}, Instance: {problemDetails.Instance}, ErrorCode: {problemDetails.ErrorCode}, ErrorSource: {problemDetails.ErrorSource})");
        }
    }
}
