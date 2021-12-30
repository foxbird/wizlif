using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Scanners
{
    public interface IScanner
    {

        public Task Scan(string interfaceName, int port);

    }
}
