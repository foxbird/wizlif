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
    public class WizScanner : ICommand
    {

        private readonly ILogger<WizScanner> _logger;
        protected IScanner _scanner;

        public static readonly int WIZ_DEFAULT_UDP_PORT = 38899;

        public WizScanner(ILogger<WizScanner> logger, IScanner scanner)
        {
            _logger = logger;
            _scanner = scanner;
        }

        public void Configure(Command parent)
        {
            var scan = new Command("scan", "Scan for WiZ bulbs")
            {
                new Option<string>(new []{"--int", "-i" }, "Interface name for scanning"),
                new Option<int>(new []{ "--port", "-p" }, description: "UDP port for wiz communication", getDefaultValue: () => WIZ_DEFAULT_UDP_PORT)
            };
            scan.Handler = CommandHandler.Create<string, int>(Run);

            parent.Add(scan);
        }

        public async Task Run(string i, int port)
        {
            await _scanner.Scan(i, port);
        }
    }
}
