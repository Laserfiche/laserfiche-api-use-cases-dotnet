// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Laserfiche.Api.Client.OAuth;
using System;
using System.IO;

namespace Laserfiche.Api
{
    internal enum AuthorizationType
    {
        CLOUD_ACCESS_KEY,
        API_SERVER_USERNAME_PASSWORD
    }

    internal class ApiClientConfiguration
    {
        internal const string ACCESS_KEY = "ACCESS_KEY";
        internal const string SERVICE_PRINCIPAL_KEY = "SERVICE_PRINCIPAL_KEY";
        internal const string REPOSITORY_ID = "REPOSITORY_ID";
        internal const string APISERVER_USERNAME = "APISERVER_USERNAME";
        internal const string APISERVER_PASSWORD = "APISERVER_PASSWORD";
        internal const string APISERVER_REPOSITORY_API_BASE_URL = "APISERVER_REPOSITORY_API_BASE_URL";
        internal const string AUTHORIZATION_TYPE = "AUTHORIZATION_TYPE";

        public string RepositoryId { get; set; }
        public string ServicePrincipalKey { get; set; }
        public AccessKey AccessKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public AuthorizationType AuthorizationType { get; set; }

        public ApiClientConfiguration(string filename)
        {
            // Read credentials from file system
            var readConfigFileOk = LoadFromDotEnv(filename);
            if (!readConfigFileOk)
            {
                Console.WriteLine("Failed to read credentials.");
            }

            // Read credentials from environment
            if (Enum.TryParse(Environment.GetEnvironmentVariable(AUTHORIZATION_TYPE), ignoreCase: true, out AuthorizationType value))
            {
                AuthorizationType = value;
            }
            else
            {
                throw new InvalidOperationException($"Environment variable '{AUTHORIZATION_TYPE}' does not exist or has an invalid value. It must be present and its value can only be '{nameof(AuthorizationType.CLOUD_ACCESS_KEY)}' or '{nameof(AuthorizationType.API_SERVER_USERNAME_PASSWORD)}'.");
            }

            RepositoryId = Environment.GetEnvironmentVariable(REPOSITORY_ID);

            if (AuthorizationType == AuthorizationType.CLOUD_ACCESS_KEY)
            {
                ServicePrincipalKey = Environment.GetEnvironmentVariable(SERVICE_PRINCIPAL_KEY);

                var base64EncodedAccessKey = Environment.GetEnvironmentVariable(ACCESS_KEY);
                if (base64EncodedAccessKey == null)
                {
                    throw new InvalidOperationException("Cannot continue due to missing access key.");
                }
                AccessKey = AccessKey.CreateFromBase64EncodedAccessKey(base64EncodedAccessKey);
            }
            else if (AuthorizationType == AuthorizationType.API_SERVER_USERNAME_PASSWORD)
            {
                Username = Environment.GetEnvironmentVariable(APISERVER_USERNAME);
                Password = Environment.GetEnvironmentVariable(APISERVER_PASSWORD);
                BaseUrl = Environment.GetEnvironmentVariable(APISERVER_REPOSITORY_API_BASE_URL);
            }
        }

        private static bool LoadFromDotEnv(string fileName)
        {
            var binFolder = AppDomain.CurrentDomain.BaseDirectory;
            var projectDir = Directory.GetParent(binFolder)?.Parent?.Parent?.Parent?.FullName;
            var path = Path.Combine(projectDir, fileName);
            if (path == null)
            {
                Console.WriteLine($"The path is null.");
                return false;
            }
            else
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"{fileName} not found.");
                    return false;
                }
            }
            DotNetEnv.Env.Load(path, new DotNetEnv.LoadOptions(
                setEnvVars: true,
                clobberExistingVars: true,
                onlyExactPath: true
            ));
            Console.WriteLine($"{fileName} found. {fileName} file should only be used in local developer computers.");
            return true;
        }
    }
}
