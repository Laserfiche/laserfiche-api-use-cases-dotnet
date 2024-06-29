// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Threading.Tasks;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        public static async Task Main()
        {
            ApiClientConfiguration config = new(".env");
            if (config.AuthorizationType != AuthorizationType.CLOUD_ACCESS_KEY)
            {
                throw new Exception("'Laserfiche.Repository.Api.Client.V2' is not supported with self-hosted API Server. Please use 'Laserfiche.Repository.Api.Client' NuGet package");
            }


            await ODataApiClientExamples.ExecuteAsync(config);
            await RepositoryApiClientExamples.ExecuteAsync(config);
        }
    }
}
