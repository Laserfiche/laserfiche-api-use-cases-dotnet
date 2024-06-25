// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Threading.Tasks;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        public static async Task Main()
        {
            ApiClientConfiguration config = new(".env");
            await ODataApiClientExamples.ExecuteAsync(config);
            await RepositoryApiClientExamples.ExecuteAsync(config);
        }
    }
}
