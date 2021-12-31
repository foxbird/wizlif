using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Models
{
    public class WizBulbRegistry
    {
        protected Dictionary<PhysicalAddress, WizBulb> Storage { get; set; } = new Dictionary<PhysicalAddress, WizBulb>();

        public List<WizBulb> Bulbs { get { return Storage.Values.ToList(); } }

        public bool Add(string ipAddress, string macAddress, string name = default)
        {
            var mac = PhysicalAddress.Parse(macAddress);
            var ip = IPAddress.Parse(ipAddress);

            return Add(ip, mac, name);
        }

        public bool Add(IPAddress ipAddress, string macAddress, string name = default)
        {
            var mac = PhysicalAddress.Parse(macAddress);

            return Add(ipAddress, mac, name);
        }

        public bool Add(IPAddress ipAddress, PhysicalAddress macAddress, string name = default)
        {
            if (String.IsNullOrEmpty(name))
                name = "wiz_" + macAddress.ToString();

            // Update IP Address on ones we have
            if (Storage.ContainsKey(macAddress))
            {
                var bulb = Storage[macAddress];
                bulb.IpAddress = ipAddress;
                bulb.Name = name;
                return true;
            }

            // Add the bulb directly
            Storage.Add(macAddress, new WizBulb() { IpAddress = ipAddress, MacAddress = macAddress, Name = name });
            return true;
        }
    }
}
