// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    /// <summary>
    /// Laserfiche OData API usage examples
    /// </summary>
    static class ODataApiClientExamples
    {
        const char CSV_COMMA_SEPARATOR = ',';
        const string ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name = "ALL_DATA_TYPES_TABLE_SAMPLE";


        public static async Task ExecuteAsync(ApiClientConfiguration config)
        {
            try
            {
                string scopes = "table.Read table.Write project/Global";

                // Create the http client
                LaserficheODataHttpClient laserficheODataHttpClient = ODataApiUtils.CreateLaserficheODataHttpClient(config, scopes);

                // Get lookup table names
                await PrintLookupTableNamesAsync(laserficheODataHttpClient);

                // Get lookup tables definitions
                Dictionary<string, Entity> entityDictionary = await laserficheODataHttpClient.GetTableMetadataAsync();

                // Get ALL_DATA_TYPES_TABLE_SAMPLE lookup table entity definition
                if (!entityDictionary.TryGetValue(ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name, out Entity allDataTypesEntity))
                {
                    throw new Exception($"Lookup table '{ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name}' not found. Please go to 'Process Automation / Data Management' and create a lookup table named '{ALL_DATA_TYPES_TABLE_SAMPLE_lookup_table_name}' using 'ALL_DATA_TYPES_TABLE_SAMPLE_DATA.csv' file in this project.");
                };

                // Export ALL_DATA_TYPES_TABLE_SAMPLE lookup table as csv.
                await ExportLookupTableCsvAsync(laserficheODataHttpClient, allDataTypesEntity);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
            }
        }

        /**
        * Prints all the Lookup Table names accessible by the user.
        */
        private static async Task<IList<string>> PrintLookupTableNamesAsync(LaserficheODataHttpClient laserficheODataHttpClient)
        {
            Console.WriteLine($"\nRetrieving Lookup tables:");
            var tableNames = await laserficheODataHttpClient.GetLookupTableNamesAsync();
            foreach (var tableName in tableNames)
            {
                Console.WriteLine($"  - {tableName}");
            }
            return tableNames;
        }


        private static async Task ExportLookupTableCsvAsync(
            LaserficheODataHttpClient laserficheODataHttpClient,
            Entity allDataTypesEntity)
        {
            Console.WriteLine($"\nExporting Lookup table {allDataTypesEntity.name}...");

            // Get ALL_DATA_TYPES_TABLE_SAMPLE lookup table columns names without the '_key' column.
            IList<string> columnNames = allDataTypesEntity.properties.Select(r => r.name).Where(r => r != allDataTypesEntity.keyName).ToList();

            int rowCount = 0;
            var tableCsv = new StringBuilder();
            string columnsHeaders = string.Join(CSV_COMMA_SEPARATOR, columnNames);
            tableCsv.AppendLine(columnsHeaders);
            Action<JsonElement> processTableRow = (tableRow) =>
            {
                rowCount++;
                tableCsv.AppendLine(tableRow.ToCsv());
            };
            await laserficheODataHttpClient.QueryLookupTableAsync(allDataTypesEntity.name, processTableRow,
                new ODataQueryParameters { Select = columnsHeaders });
            var csv = tableCsv.ToString();

            Console.WriteLine(csv);
            Console.WriteLine($"\nDone Exporting Lookup table {allDataTypesEntity.name} with {rowCount} rows.");
        }

        static string ToCsv(this JsonElement element)
        {
            StringBuilder sb = new();
            if (element.ValueKind != JsonValueKind.Object)
                throw new ArgumentException(nameof(element.ValueKind));

            bool first = true;
            foreach (var property in element.EnumerateObject())
            {
                if (!first)
                {
                    sb.Append(CSV_COMMA_SEPARATOR);
                }
                first = false;
                string strValue = property.Value.ToString();
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        strValue = property.Value.GetString();
                        if (strValue != null && strValue.Any(r => r == CSV_COMMA_SEPARATOR || r == '"'))
                            strValue = "\"" + strValue.Replace("\"", "\"\"") + "\"";
                        break;
                    case JsonValueKind.Number:
                        strValue = property.Value.GetDecimal().ToString(CultureInfo.InvariantCulture);
                        break;
                    case JsonValueKind.False:
                        strValue = "false";
                        break;
                    case JsonValueKind.True:
                        strValue = "true";
                        break;
                    default:
                        strValue = "";
                        break;
                }
                sb.Append(strValue);
            }
            return sb.ToString();
        }
    }
}