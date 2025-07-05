using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using DbUp;
using HappyCode.NetCoreBoilerplate.Db.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace HappyCode.NetCoreBoilerplate.Db
{
    class Program
    {
        public static int Main(string[] args)
        {
            var configuration = LoadAppConfiguration();
            var upgradeOptions = configuration.GetSection("UpgradeOptions").Get<UpgradeOptions>() 
                ?? throw new InvalidOperationException("UpgradeOptions section is missing in configuration");
            var scriptsAndCodePatternPattern = new Regex(upgradeOptions.ScriptsAndCodePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var connectionString = configuration.GetConnectionString("MsSqlDb") 
                ?? throw new InvalidOperationException("Connection string 'MsSqlDb' is missing in configuration");

            // First, ensure the database exists
            EnsureDatabaseExists(connectionString);

            // Now run the migrations
            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly(), 
                    (fileName) => scriptsAndCodePatternPattern.IsMatch(fileName) && !fileName.EndsWith("S001_CreateDatabase.sql"))
                .WithExecutionTimeout(TimeSpan.FromSeconds(upgradeOptions.CommandExecutionTimeoutSeconds))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            return 0;
        }

        private static void EnsureDatabaseExists(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            var masterConnectionString = builder.ToString();

            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
BEGIN
    CREATE DATABASE [{databaseName}]
END";
                    command.ExecuteNonQuery();
                }
            }
        }

        private static IConfigurationRoot LoadAppConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
