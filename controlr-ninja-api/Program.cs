using Loggly;
using Loggly.Config;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using controlr.ninja.api.Settings;

namespace controlr.ninja.api
{
    public class Program
    {
        private static string _environmentName;

        public static void Main(string[] args)
        {
            var webhost = CreateWebHostBuilder(args);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{_environmentName}.json", true, true)
                .Build();

            var logglySettings = new LogglySettings();
            configuration.GetSection("Serilog:Loggly").Bind(logglySettings);

            SetupLogglyConfiguration(logglySettings);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            webhost.Build().Run();

          //  comment test
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, config) =>
                {
                    config.ClearProviders();
                    _environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                })
                .UseStartup<Startup>();

        private static void SetupLogglyConfiguration(LogglySettings logglySettings)
        {
            //Configure Loggly
            var config = LogglyConfig.Instance;
            config.CustomerToken = logglySettings.CustomerToken;
            config.ApplicationName = logglySettings.ApplicationName;
            config.Transport = new TransportConfiguration()
            {
                EndpointHostname = logglySettings.EndpointHostname,
                EndpointPort = logglySettings.EndpointPort,
                LogTransport = logglySettings.LogTransport
            };
            config.ThrowExceptions = logglySettings.ThrowExceptions;

            //Define Tags sent to Loggly
            config.TagConfig.Tags.AddRange(new ITag[]{
                new ApplicationNameTag {Formatter = "Application-{0}"},
                new HostnameTag { Formatter = "Host-{0}" }
            });
        }
    }
}
