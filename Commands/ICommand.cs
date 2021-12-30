using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Commands
{
    public interface ICommand
    {
        public void Configure(Command parent);
    }
}
