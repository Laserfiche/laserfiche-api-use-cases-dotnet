using System.Diagnostics;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Utils
    {
        internal static bool LoadFromDotEnv(string fileName)
        {
            var path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, fileName);
            if (path == null)
            {
                Trace.TraceWarning($"The path is null.");
                return false;
            }
            else
            {
                if (!File.Exists(path))
                {
                    Trace.TraceWarning($"{fileName} not found.");
                    return false;
                }
            }
            DotNetEnv.Env.Load(path, new DotNetEnv.LoadOptions(
                setEnvVars: true,
                clobberExistingVars: true,
                onlyExactPath: true
            ));
            Trace.TraceWarning($"{fileName} found. {fileName} file should only be used in local developer computers.");
            return true;
        }
    }
}
