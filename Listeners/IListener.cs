using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Listeners
{
    public delegate void DataReceivedDelegate(UdpClient client, IPEndPoint endpoint, byte[] data);

    public interface IListener
    {
        public DataReceivedDelegate DataReceived { get; set; }
        public int Port { get; set; }
        public UdpClient Client { get; set; }
        
        public bool IsRunning();
        public void Start();
        public void Stop();
    }
}
