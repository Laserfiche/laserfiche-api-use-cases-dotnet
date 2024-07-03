// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Laserfiche.Api.ODataApi
{
    /// <summary>
    /// Laserfiche OData API usage examples
    /// </summary>
    static class ODataApiClientExamples
    {
        const string ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name = "ALL_DATA_TYPES_TABLE_SAMPLE";


        public static async Task ExecuteAsync(ApiClientConfiguration config)
        {
            try
            {
                string scope = "table.Read table.Write project/Global";

                // Create the http client
                ODataApiClient oDataApiClient = ODataApiClient.CreateFromServicePrincipalKey(config.ServicePrincipalKey, config.AccessKey, scope);

                // Get lookup table names
                await PrintLookupTableNamesAsync(oDataApiClient);

                // Get lookup tables definitions
                Dictionary<string, Entity> entityDictionary = await oDataApiClient.GetTableMetadataAsync();

                // Get ALL_DATA_TYPES_TABLE_SAMPLE lookup table entity definition
                if (!entityDictionary.TryGetValue(ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name, out Entity allDataTypesEntity))
                {
                    throw new Exception($"Lookup table '{ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name}' not found. Please go to 'Process Automation / Data Management' and create a lookup table named '{ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name}' using '{ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name}.csv' file in this project.");
                };



                // Export ALL_DATA_TYPES_TABLE_SAMPLE lookup table as csv.
                string csv = await ExportLookupTableCsvAsync(oDataApiClient, allDataTypesEntity);

                // Replace ALL_DATA_TYPES_TABLE_SAMPLE lookup table as csv.
                var csvWithAdditionalRow = csv + csv.Trim().Split(Environment.NewLine).Last(); //Append a duplicate of the last row
                var taskId = await ReplaceLookupTableAsync(oDataApiClient, allDataTypesEntity, csvWithAdditionalRow);

                // Monitor replace operation task progress
                await MonitorReplaceLookupTableTaskAsync(oDataApiClient, taskId);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
            }
        }

        private static async Task<IList<string>> PrintLookupTableNamesAsync(ODataApiClient oDataApiClient)
        {
            Console.WriteLine($"\nRetrieving Lookup tables:");
            var tableNames = await oDataApiClient.GetLookupTableNamesAsync();
            foreach (var tableName in tableNames)
            {
                Console.WriteLine($"  - {tableName}");
            }
            return tableNames;
        }

        public static async Task<string> ExportLookupTableCsvAsync(
            ODataApiClient oDataApiClient,
            Entity allDataTypesEntity)
        {
            Console.WriteLine($"\nExporting Lookup table {allDataTypesEntity.Name}...");

            // Get ALL_DATA_TYPES_TABLE_SAMPLE lookup table columns names without the '_key' column.
            IList<string> columnNames = allDataTypesEntity.Properties.Select(r => r.Name).Where(r => r != allDataTypesEntity.KeyName).ToList();

            int rowCount = 0;
            var tableCsv = new StringBuilder();
            string columnsHeaders = string.Join(ODataUtilities.CSV_COMMA_SEPARATOR, columnNames);
            tableCsv.AppendLine(columnsHeaders);
            Action<JsonElement> processTableRow = (tableRow) =>
            {
                rowCount++;
                var rowCsv = tableRow.ToCsv();
                if (!string.IsNullOrWhiteSpace(rowCsv))
                    tableCsv.AppendLine(rowCsv);
            };
            await oDataApiClient.QueryLookupTableAsync(allDataTypesEntity.Name, processTableRow,
                new ODataQueryParameters { Select = columnsHeaders });
            var csv = tableCsv.ToString();

            Console.WriteLine(csv);
            Console.WriteLine($"\nDone Exporting Lookup table {allDataTypesEntity.Name} with {rowCount} rows.");
            return csv;
        }

        private static async Task<string> ReplaceLookupTableAsync(
           ODataApiClient oDataApiClient,
           Entity allDataTypesEntity,
           string csv)
        {
            Console.WriteLine($"\nReplacing Lookup table {allDataTypesEntity.Name}...");

            var taskId = await oDataApiClient.ReplaceAllRowsAsync(
                allDataTypesEntity.Name,
                new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            return taskId;
        }
        private static async Task MonitorReplaceLookupTableTaskAsync(
            ODataApiClient oDataApiClient,
            string taskId)
        {
            await oDataApiClient.MonitorTaskAsync(taskId,
                (taskProgress) =>
                {
                    Console.WriteLine($" > Task with id '{taskId}' {taskProgress.Status}." +
                        (taskProgress.Result != null ? " " + System.Text.Json.JsonSerializer.Serialize(taskProgress.Result) : "") +
                        (taskProgress.Errors != null && taskProgress.Errors.Count > 0 ? " " + System.Text.Json.JsonSerializer.Serialize(taskProgress.Errors) : ""));
                });
        }
    }
}