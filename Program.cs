using System;
using System.IO;
using System.Threading.Tasks;
using Foxpaws.Wizlif.Commands;
using Foxpaws.Wizlif.Listeners;
using Foxpaws.Wizlif.Scanners;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Foxpaws.Wizlif
{
    class Program
    {

        public static readonly string SETTINGS_FILE = "appsettings.json";
        public static readonly string SETTINGS_TEMPLATE = "appsettings.{0}.json";
        public static readonly string ENVIRONMENT_VARIABLE = "DOTNETCORE_ENVIRONMENT";

        static async Task Main(string[] args)
        {
            var config = BuildConfiguration(args);
            var services = ConfigureServices(new ServiceCollection(), config);
            var app = CommandLineParser.Configure(config, services);

            await app.Parse(args);

        }

        public static IConfigurationRoot BuildConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(SETTINGS_FILE, false, true)
                .AddJsonFile(String.Format(SETTINGS_TEMPLATE, Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE)), true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
        }

        public static ServiceProvider ConfigureServices(ServiceCollection services, IConfiguration configuration)
        {
            // Add Options support
            services.AddOptions();

            // Configure logging
            services.AddLogging(c =>
            {
                c.AddConfiguration(configuration.GetSection("Logging"));
                c.AddConsole();
            });

            // Command line tools
            services.AddCommandLineParser();
            services.AddTransient<ICommand, Commands.WizScanner>();
            services.AddSingleton<IScanner, Scanners.WizScanner>();
            services.AddSingleton<IWizListener, WizListener>();

            return services.BuildServiceProvider();
        }
    }
}
