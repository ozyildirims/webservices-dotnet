namespace HappyCode.NetCoreBoilerplate.Db.Options
{
    public class UpgradeOptions
    {
        public int CommandExecutionTimeoutSeconds { get; set; }
        public required string ScriptsAndCodePattern { get; set; }
    }
}
