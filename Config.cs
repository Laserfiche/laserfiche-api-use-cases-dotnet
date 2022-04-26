namespace Laserfiche.Repository.Api.Client.Sample.Service.Config
{
    public class Configuration
    {
        public int Csid { get; set; }
        
        public string ClientId { get; set; } = string.Empty;
        
        public string ServicePrincipalKey { get; set; } = string.Empty;
        
        public string AccessKey { get; set; } = string.Empty;
        
        public string Domain { get; set; } = string.Empty;
        
        public string RepoId { get; set; } = string.Empty;
        
        public int ApprovalSenarioFolderId { get; set; }
        
        public int CarSortingScenarioFolderId { get; set; }
    }
}