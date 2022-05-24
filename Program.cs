namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    static class Program
    {
        public static void Main()
        {
            // Read credentials from file system
            var readConfigFileOk = Utils.LoadFromDotEnv("TestConfig.env");
            if (!readConfigFileOk)
            {

            }
        }
    }
}
