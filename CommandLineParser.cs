using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.CommandLine.NamingConventionBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace Foxpaws.Wizlif
{
    public class CommandLineParser
    {
        private readonly ILogger<CommandLineParser> _logger;
        protected RootCommand rootCommand;

        public CommandLineParser(ILogger<CommandLineParser> logger, IServiceProvider services)
        {
            _logger = logger;
            rootCommand = new RootCommand("Root command description")
            {
                new Option<string>("--name", "Test Name")
            };
            rootCommand.Handler = CommandHandler.Create<string>(Run);

            foreach (var command in services.GetServices<Wizlif.Commands.ICommand>())
            {
                command.Configure(rootCommand);
            }
        }

        public async Task Parse(string[] args)
        {
            await rootCommand.InvokeAsync(args);
        }

        public async Task Run(string name)
        {
            _logger.LogInformation($"Testing! '{name}'");
            await Task.CompletedTask;
        }

        public static CommandLineParser Configure(IConfiguration config, ServiceProvider services)
        {
            return services.GetRequiredService<CommandLineParser>();
        }

    }

    public static class CommandLineParserExtensions
    {
        public static IServiceCollection AddCommandLineParser(this IServiceCollection collection)
        {
            return collection.AddSingleton<CommandLineParser>();
        }
    }

}
