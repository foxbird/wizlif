using Foxpaws.Wizlif.Models;
using Foxpaws.Wizlif.Scanners;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Commands
{
    public class LifxServer : ICommand
    {

        private readonly ILogger<LifxServer> _logger;

        public LifxServer(ILogger<LifxServer> logger)
        {
            _logger = logger;
        }

        public void Configure(Command parent)
        {
            var serve = new Command("serve", "Serve up LIFX bulbs from a Wiz registry")
            {
                new Option<int>(new []{ "--port", "-p" }, description: "UDP port for wiz communication", getDefaultValue: () => 0)
            };
            serve.Handler = CommandHandler.Create<int>(Run);

            parent.Add(serve);
        }

        public async Task Run(int port)
        {
            var hex = "3100001402000000d073d500133700000000000000000201000000000000000066000000005555ffffffffac0d00000000";
            var bytes = Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();

            var message = LifxMessage.Read(bytes);
            await Task.CompletedTask;
        }
    }
}
