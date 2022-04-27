using Laserfiche.Repository.Api.Client.Sample.Service.Config;

public class Program
{
    static void Main(string[] args)
    {
        var configPath = Directory.GetCurrentDirectory() + "\\Config.env";

        // Load environment variables
        var env = DotNetEnv.Env.Load(configPath);
        var config = new Configuration();
        foreach (var kv in env)
        {
            switch (kv.Key)
            {
                case "CSID":
                    var csid = 0;
                    if (Int32.TryParse(kv.Value, out csid))
                    {
                        config.Csid = csid;
                    }
                    else
                    {
                        throw new ArgumentException("The CSID isn't valid.");
                    }
                    break;
                case "CLIENT_ID":
                    config.ClientId = kv.Value;
                    break;
                case "SERVICE_PRINCIPAL_KEY":
                    config.ServicePrincipalKey = kv.Value;
                    break;
                case "ACCESS_KEY":
                    config.AccessKey = kv.Value;
                    break;
                case "DOMAIN":
                    config.Domain = kv.Value;
                    break;
                case "REPO_ID":
                    config.RepoId = kv.Value;
                    break;
                case "APPROVAL_SCENARIO_FOLDER_ID":
                    var approvalScenarioFolderId = 0;
                    if (Int32.TryParse(kv.Value, out approvalScenarioFolderId))
                    {
                        config.ApprovalSenarioFolderId = approvalScenarioFolderId;
                    }
                    else
                    {
                        throw new ArgumentException("The APPROVAL_SCENARIO_FOLDER_ID isn't valid.");
                    }
                    break;
                case "CAR_SORTING_SCENARIO_FOLDER_ID":
                    var carSortingScenarioFolderId = 0;
                    if (Int32.TryParse(kv.Value, out carSortingScenarioFolderId))
                    {
                        config.CarSortingScenarioFolderId = carSortingScenarioFolderId;
                    }
                    else
                    {
                        throw new ArgumentException("The APPROVAL_SCENARIO_FOLDER_ID isn't valid.");
                    }
                    break;
                default:
                    throw new ArgumentException("Unexpected environment variable.");
            }
        }

        // Initialize OAuthClient

        // Initialize RepositoryApiClient

        // Run Scenario 1

        // Run Scenario 2
    }
}