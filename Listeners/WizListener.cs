using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Listeners
{
    public class WizListener : IWizListener
    {
        public int Port { get; set; }
        public DataReceivedDelegate DataReceived { get; set; }

        private readonly ILogger<WizListener> _logger;
        public UdpClient Client { get; set; }
        private Thread thread { get; set; }
        private CancellationTokenSource source { get; set; }
        private CancellationToken token { get; set; }
        private AutoResetEvent readySignal { get; set; } = new AutoResetEvent(false);

        public WizListener(ILogger<WizListener> logger)
        {
            _logger = logger;
            source = new CancellationTokenSource();
            token = source.Token;
        }

        protected bool isRunning = false;
        public bool IsRunning()
        {
            return isRunning;
        }

        public void Start()
        {
            if (Client == null)
                throw new ArgumentNullException("Client", "Client must point to a valid UdpClient");

            if (isRunning)
                return;

            _logger.LogInformation("Started new Wiz Listener thread");
            thread = new Thread(Listen);
            thread.Start();
            readySignal.WaitOne();
            isRunning = true;
        }

        private void Listen()
        {

            // Signal the main thread to keep going now that we're open
            readySignal.Set();

            while (!token.IsCancellationRequested)
            {
                _logger.LogDebug("Waiting on data from UDP");
                var task = Client.ReceiveAsync();
                try
                {
                    Task.WaitAny(new Task[] { task }, token);
                    var remote = task.Result.RemoteEndPoint;
                    var data = task.Result.Buffer;
                    _logger.LogDebug($"Received data from UDP [{remote.Address}/{remote.Port}] -- {data.Length} bytes");

                    if (DataReceived != null)
                        DataReceived(Client, remote, data);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Listen task cancelled");
                }
            }
        }

        public void Stop()
        {
            _logger.LogDebug("Cancelling the token to terminate the the thread");
            source.Cancel();

            _logger.LogDebug("Joining thread to make sure it exits");
            thread.Join();

            _logger.LogInformation("Wiz listener stopped");
            readySignal.Reset();
            isRunning = false;
        }
    }
}
