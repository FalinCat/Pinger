using Pinger.Model;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace Pinger.Services.Network
{
    public class NetScan
    {
        //private List<NetworkEntityStatus> statuses = new List<NetworkEntityStatus>();
        private ConcurrentDictionary<string, NetworkEntityStatus> statuses = new ConcurrentDictionary<string, NetworkEntityStatus>();

        public async Task<List<NetworkEntityStatus>> ScanRange(IPRange range)
        {
            statuses.Clear();
            var tasks = new List<Task>();
            var ipRange = range.GetAllIP();

            foreach (var item in ipRange)
            {
                var task = Scan(item.ToString());

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            return statuses.Select(c => c.Value).ToList().OrderBy(c => c.Address, new IPComparer()).ToList();
        }

        private async Task Scan(string ip)
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync(ip, 1000);
            var machineName = ResolveWithTimeout(ip, 1000);
            var mac = GetMacAddress(ip);

            var result = new NetworkEntityStatus()
            {
                Address = ip,
                Mac = mac,
                RoundtripTime = reply.RoundtripTime,
                Status = reply.Status.ToString(),
                Name = machineName
            };

            statuses.TryAdd(ip, result);
        }

        private string GetMachineName(string ipAddress)
        {
            var timeout = TimeSpan.FromSeconds(3.0);
            string machineName = string.Empty;
            try
            {
                var task = Dns.GetHostEntryAsync(ipAddress);
                if (!task.Wait(timeout))
                {
                    return "Имя не найдено";
                }
                machineName = task.Result.HostName;
            }
            catch (Exception ex)
            {
            }

            return machineName;
        }

        private string ResolveWithTimeout(string hostName, long timeout)
        {
            var timeOut = TimeSpan.FromMicroseconds(timeout);
            var task = Dns.GetHostEntryAsync(hostName);
            if (!task.Wait(timeOut))
            {
                return "Имя не найдено";
            }

            return task.Result.HostName;
        }

        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            Process pProcess = new Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                            + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                            + "-" + substrings[7] + "-"
                            + substrings[8].Substring(0, 2);
                return macAddress;
            }
            else
            {
                return "not found";
            }
        }
    }
}