using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Models
{
    public class WizBulb
    {
        public IPAddress IpAddress { get; set; }
        public PhysicalAddress MacAddress { get; set; }
        public string Name { get; set; }
    }
}
