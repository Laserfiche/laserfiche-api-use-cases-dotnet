// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Laserfiche.Api.Client.HttpHandlers;
using Laserfiche.Api.Client.OAuth;
using Laserfiche.Api.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    /// <summary>
    /// Laserfiche OData API Client. See https://api.laserfiche.com/odata4/swagger/index.html?urls.primaryName=v1
    /// </summary>
    public class LaserficheODataHttpClient
    {
        private readonly HttpClient _httpClient;
        private LaserficheODataHttpClient(IHttpRequestHandler httpRequestHandler)
        {
            if (httpRequestHandler == null)
                throw new ArgumentNullException(nameof(httpRequestHandler));


            var apiHttpMessageHandler = new ApiHttpMessageHandler(
                httpRequestHandler,
                (domain) => DomainUtils.GetODataApiBaseUri(domain));

            _httpClient = new HttpClient(apiHttpMessageHandler);
            _httpClient.BaseAddress = new Uri("http://example.com"); //Needed to use relative URLs in http requests.
        }

        public static LaserficheODataHttpClient CreateFromServicePrincipalKey(string servicePrincipalKey, AccessKey accessKey, string scope)
        {
            var httpRequestHandler = new OAuthClientCredentialsHandler(servicePrincipalKey, accessKey, scope);
            return new LaserficheODataHttpClient(httpRequestHandler);
        }

        public async Task<IList<string>> GetLookupTableNamesAsync()
        {
            var httpResponse = await _httpClient.GetAsync($"/table");
            httpResponse.EnsureSuccessStatusCode();
            JsonDocument content = await httpResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var value = content.RootElement.GetProperty("value");

            var tableNames = new List<string>();
            foreach (var element in value.EnumerateArray())
            {
                string kind = element.GetStringPropertyValue("kind");
                if (kind == "EntitySet")
                {
                    string name = element.GetStringPropertyValue("name");
                    string url = element.GetStringPropertyValue("url");
                    tableNames.Add(name);
                }
            }
            return tableNames;
        }

        public async Task<Dictionary<string, Entity>> GetTableMetadataAsync()
        {
            var httpResponse = await _httpClient.GetAsync($"/table/$metadata");
            httpResponse.EnsureSuccessStatusCode();
            using var contentStream = await httpResponse.Content.ReadAsStreamAsync();
            var edmXml = XDocument.Load(contentStream);
            Dictionary<string, Entity> entityDictionary = Utils.EdmxToEntityDictionary(edmXml);
            return entityDictionary;
        }

        public async Task QueryLookupTableAsync(
           string tableName,
           Action<JsonElement> processTableRow,
           ODataQueryParameters queryParameters)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            string queryTableUrl = $"table/{Uri.EscapeDataString(tableName)}";
            var qs = queryParameters?.ToQueryString();
            if (qs != null)
                queryTableUrl += "?" + qs;

            while (!string.IsNullOrWhiteSpace(queryTableUrl))
            {
                using (var httpResponse = await _httpClient.GetAsync(queryTableUrl))
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
    }

    public class Entity
    {
        public string name { get; set; }
        public string keyName { get; set; }
        public List<Property> properties { get; set; }
    }
    public class Property
    {
        public string name { get; set; }
        public Type _type { get; set; }
        public Boolean? nullable { get; set; }
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

    internal static class Utils
    {
        public static string GetStringPropertyValue(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement nameElement))
            {
                var value = nameElement.GetString();
                return value;
            }
            return null;
        }

        static public Dictionary<string, Entity> EdmxToEntityDictionary(XDocument edmx)
        {
            XElement schema = edmx.Descendants().Where(x => x.Name.LocalName == "Schema").FirstOrDefault();
            int v = 0;
            string z = v.GetType().ToString();
            XNamespace ns = schema.GetDefaultNamespace();
            Dictionary<string, Entity> entityTypes = schema.Descendants(ns + "EntityType")
                .Select(x => new Entity()
                {
                    name = x.Attribute("Name")?.Value,
                    keyName = x.Descendants(ns + "PropertyRef").FirstOrDefault()?.Attribute("Name")?.Value,
                    properties = x.Elements(ns + "Property").Select(y => new Property()
                    {
                        name = y.Attribute("Name")?.Value,
                        _type = Type.GetType("System." + ((string)y.Attribute("Type")).Split(new char[] { '.' }).Last()),
                        nullable = (y.Attribute("Nullable") == null) ? (Boolean?)null : ((string)y.Attribute("Nullable") == "false") ? false : true
                    }).ToList()
                })
                .ToDictionary(x => x.name, x => x);

            return entityTypes;
        }
    }

}
