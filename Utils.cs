// Copyright (c) Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.IO;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Utils
    {
        internal static bool LoadFromDotEnv(string fileName)
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
