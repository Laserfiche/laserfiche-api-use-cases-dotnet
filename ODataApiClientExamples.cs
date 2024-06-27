// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Laserfiche.Api.Client.HttpHandlers;
using Laserfiche.Api.Client.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    /// <summary>
    /// Laserfiche OData API usage examples
    /// </summary>
    static class ODataApiClientExamples
    {
        const char CSV_COMMA_SEPARATOR = ',';

        public static async Task ExecuteAsync(ApiClientConfiguration config)
        {
            try
            {
                HttpClient laserficheODataHttpClient = CreateLaserficheODataHttpClient(config);

                var tableUrls = await PrintLookupTableNamesAsync(laserficheODataHttpClient);
                Dictionary<string, Entity> entityDictionary = await GetTableMetadataAsync(laserficheODataHttpClient);

                var tableUrl = tableUrls.First();
                tableUrl = "Paolo_All_Data_Types";//"Paolo_10000_Rows";// "Paolo_All_Data_Types";  //TODO REMOVE

                Entity entity = entityDictionary[tableUrl];
                IList<string> columnNames = entity.properties.Select(r => r.name).Where(r => r != entity.key).ToList();
                await ExportLookupTableCsvAsync(laserficheODataHttpClient, tableUrl, columnNames);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
            }
        }

        /// <summary>
        /// Returns an HttpClient that knows how to get / refresh a Laserfiche API Access Token with built-in in retry.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static HttpClient CreateLaserficheODataHttpClient(ApiClientConfiguration config)
        {
            string requiredScopes = "table.Read table.Write project/Global";
            if (config.AuthorizationType == AuthorizationType.CLOUD_ACCESS_KEY)
            {
                var httpRequestHandler = new OAuthClientCredentialsHandler(config.ServicePrincipalKey, config.AccessKey, requiredScopes);
                var apiHttpMessageHandler = new ApiHttpMessageHandler(
                    httpRequestHandler,
                    (domain) => DomainUtils.GetODataApiBaseUri(domain));

                var httpClient = new HttpClient(apiHttpMessageHandler);
                httpClient.BaseAddress = new Uri("http://example.com"); //Needed to use relative URLs in http requests.
                return httpClient;
            }
            else
            {
                throw new Exception($"Invalid value for '{ApiClientConfiguration.AUTHORIZATION_TYPE}'. It can only be '{nameof(AuthorizationType.CLOUD_ACCESS_KEY)}' or '{nameof(AuthorizationType.API_SERVER_USERNAME_PASSWORD)}'.");
            }
        }

        /**
        * Prints all the Lookup Table names accessible by the user.
        */
        private static async Task<IList<string>> PrintLookupTableNamesAsync(HttpClient laserficheODataHttpClient)
        {
            Console.WriteLine($"\nRetrieving Lookup tables:");
            var httpResponse = await laserficheODataHttpClient.GetAsync($"/table");
            httpResponse.EnsureSuccessStatusCode();
            JsonDocument content = await httpResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var value = content.RootElement.GetProperty("value");

            var urls = new List<string>();
            foreach (var element in value.EnumerateArray())
            {
                string kind = element.GetStringPropertyValue("kind");
                if (kind == "EntitySet")
                {
                    string tableName = element.GetStringPropertyValue("name");
                    string url = element.GetStringPropertyValue("url");
                    urls.Add(url);
                    Console.WriteLine($"  - Lookup table name: '{tableName}', url: '{url}'");
                }
            }
            return urls;
        }

        private static async Task<Dictionary<string, Entity>> GetTableMetadataAsync(HttpClient laserficheODataHttpClient)
        {
            Console.WriteLine($"\nRetrieving Lookup tables OData $metadata document that contains column definitions.");
            var httpResponse = await laserficheODataHttpClient.GetAsync($"/table/$metadata");
            httpResponse.EnsureSuccessStatusCode();
            using var contentStream = await httpResponse.Content.ReadAsStreamAsync();
            var edmx = XDocument.Load(contentStream);

            Dictionary<string, Entity> entityDictionary = EdmxUtils.EdmxToEntityDictionary(edmx);
            return entityDictionary;
        }

        private static async Task ExportLookupTableCsvAsync(
            HttpClient laserficheODataHttpClient,
            string tableUrl,
            IList<string> columnNames)
        {
            Console.WriteLine($"\nExporting Lookup table {tableUrl}...");

            int rowCount = 0;
            var tableCsv = new StringBuilder();
            string columnsHeaders = string.Join(CSV_COMMA_SEPARATOR, columnNames);
            tableCsv.AppendLine(columnsHeaders);
            Action<JsonElement> processTableRow = (tableRow) =>
            {
                rowCount++;
                tableCsv.AppendLine(tableRow.ToCsv());
            };
            await QueryLookupTableAsync(laserficheODataHttpClient, tableUrl, processTableRow,
                new ODataQueryParameters { Select = columnsHeaders });
            var csv = tableCsv.ToString();

            Console.WriteLine(csv);
            Console.WriteLine($"\nDone Exporting Lookup table {tableUrl} with {rowCount} rows.");
        }

        private static async Task QueryLookupTableAsync(
            HttpClient laserficheODataHttpClient,
            string tableUrl,
            Action<JsonElement> processTableRow,
            ODataQueryParameters queryParameters)
        {
            Console.WriteLine($"\nQuerying Lookup table {tableUrl}:");
            string queryTableUrl = $"table/{Uri.EscapeDataString(tableUrl)}";
            var qs = queryParameters?.ToQueryString();
            if (qs != null)
                queryTableUrl += "?" + qs;

            while (!string.IsNullOrWhiteSpace(queryTableUrl))
            {
                using (var httpResponse = await laserficheODataHttpClient.GetAsync(queryTableUrl))
                {
                    httpResponse.EnsureSuccessStatusCode();
                    JsonDocument content = await httpResponse.Content.ReadFromJsonAsync<JsonDocument>();
                    foreach (var item in content.RootElement.GetProperty("value").EnumerateArray())
                    {
                        processTableRow(item);
                    };

                    queryTableUrl = content.RootElement.GetStringPropertyValue("@odata.nextLink");
                }
            }
        }

        static string GetStringPropertyValue(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement nameElement))
            {
                var value = nameElement.GetString();
                return value;
            }
            return null;
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



    public class ODataQueryParameters
    {
        /// <summary>
        /// Aggregation behavior is triggered using the query option $apply. It takes a sequence of set transformations, separated by forward slashes to express that they are consecutively applied, i.e., the result of each transformation is the input to the next transformation.
        /// </summary>
        public string Apply { get; set; }

        /// <summary>
        /// A function that must evaluate to true for a record to be returned. e.g.: '"first_name eq 'Paolo'"
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Limits the properties returned in the result.
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// Specifies the order in which items are returned.
        /// </summary>
        public string Orderby { get; set; }

        /// <summary>
        /// Escaped URL querystring parameters.
        /// </summary>
        /// <returns></returns>
        public string ToQueryString()
        {
            var qslist = new List<string>();
            if (!string.IsNullOrWhiteSpace(Apply))
                qslist.Add($"$apply={Uri.EscapeDataString(Apply)}");

            if (!string.IsNullOrWhiteSpace(Filter))
                qslist.Add($"$filter={Uri.EscapeDataString(Filter)}");

            if (!string.IsNullOrWhiteSpace(Select))
                qslist.Add($"$select={Uri.EscapeDataString(Select)}");

            if (!string.IsNullOrWhiteSpace(Orderby))
                qslist.Add($"$orderby={Uri.EscapeDataString(Orderby)}");

            return qslist.Count == 0 ? null : string.Join('&', qslist);
        }
    }
}