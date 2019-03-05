using GrinProMiner.Models;
using Microsoft.AspNetCore.Hosting.Internal;
using Mozkomor.GrinGoldMiner;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebMonitor.Models;

namespace WebMonitor
{
    public class APIComm
    {
        private static HttpClient _client;
        public static HttpClient client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.Timeout = TimeSpan.FromSeconds(10);
                }

                return _client;
            }
        }

        public static Task RefreshRigsTask;
        
        /// <summary>
        /// Key is IP
        /// </summary>
        public static ConcurrentDictionary<string, RigStatus> Rigs { get; set; }

        public static void Init()
        {
            ReadRigsFromFile();
            //DebugSim100Rigs();
            RefreshRigsTask = Task.Factory.StartNew(() => { RefreshRigs(); }, TaskCreationOptions.LongRunning);
        }

        private static void DebugSim100Rigs()
        {
            var rig1 = Rigs.First();

            for (int i = 0;  i < 100; i++)
            {
                var ip_endpart = 127 + i;
                var ip = $"10.0.0.{ip_endpart}";

                Rigs[ip] = rig1.Value;
            }
        }

        /// <summary>
        /// Take all rigs from Rigs and save their IPs to file (overwrite what is there)
        /// </summary>
        public static void SaveRigsToFile()
        {
            var abs = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //var abs = Path.GetFullPath("~/").Replace("~\\", "");
            var dir = Path.Combine(abs,"Data");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, "knownrigs.txt");

            var keys = Rigs.Keys.ToList();
            lock (keys)
            {
                File.WriteAllText(path, string.Join(Environment.NewLine, Rigs.Keys));
            }
        }

        public static void ReadRigsFromFile()
        {
            try
            {
                var abs = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                //var abs = Path.GetFullPath("~/").Replace("~\\", "");
                var dir = Path.Combine(abs, "Data");
                if (Directory.Exists(dir))
                {
                    var file = Path.Combine(dir, "knownrigs.txt");
                    if (File.Exists(file))
                    {
                        var lines = File.ReadAllLines(file);
                        AddMultipleRigs(lines, out List<string> a, out int b);
                    }
                }
            }
            catch { }
        }

        public static void AddMultipleRigs(string[] addresses, out List<string> notAdded, out int added)
        {
            notAdded = new List<string>();
            added = 0;
            foreach (var rig in addresses)
            {
                if (APIComm.Rigs.ContainsKey(rig))
                {
                    //already in Rigs, just deal with new
                    //var last = APIComm.Rigs[rig];
                    //var rigResponse = GetRigStatus(last.Port, last.RigName, last.IP).Result;
                }
                else
                {
                    try
                    {
                        var rigstatus = APIComm.GetRigStatus("5777", "", rig, saveToFile: false).Result;

                        //we hafe fresh stats
                        if (rigstatus != null)
                        {
                            added++;
                            //method GetRigStatus added to Rigs collection
                        }
                        else
                        {
                            notAdded.Add(rig);
                        }
                    }
                    catch { notAdded.Add(rig); }
                }
            }

            APIComm.SaveRigsToFile();
        }

        public static async Task<RigStatus> GetRigStatus(string port, string name, string ip, bool saveToFile = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://{ip}:{port}/api/status");

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    SimpleStatus s = await response.Content.ReadAsAsync<SimpleStatus>();
                    RigStatus r = new RigStatus() { LastComm = DateTime.Now, Port = port, RigName = name, Status = s, IP = ip };
                    APIComm.Rigs[ip.ToString()] = r;
                    if (saveToFile)
                        APIComm.SaveRigsToFile();
                    return r;
                }
                else
                {
                    return null;
                }

            }
            catch
            { return null; }

        }

        public static async Task<Config> GetRigConfig(string address)
        {
            if (!address.Contains(":"))
                address += ":5777";

            var request = new HttpRequestMessage(HttpMethod.Get, $"http://{address}/api/config");

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Config r = await response.Content.ReadAsAsync<Config>();
                    return r;
                }
                else
                {
                    return null;
                }

            }
            catch
            { return null; }

        }

        public static async Task<bool> PostRigConfig(string address, Config config)
        {
            if (!address.Contains(":"))
                address += ":5777"; 

            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{address}/api/config");
            request.Content = new StringContent(JsonConvert.SerializeObject(config),Encoding.UTF8,"application/json");

            try
            {
                var response = await client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            { return false; }

        }

        public static async Task<bool> PostRigPrimaryConnection(string address, Connection connection)
        {
            if (!address.Contains(":"))
                address += ":5777";

            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{address}/api/connections/active");
            request.Content = new StringContent(JsonConvert.SerializeObject(connection), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            { return false; }

        }

        public static void RefreshRigs()
        {
            Task.Delay(TimeSpan.FromMinutes(1)).Wait();

            while (true)
            {
                var keys = Rigs.Keys.ToList();

                foreach (var key in keys)
                {
                    var last = APIComm.Rigs[key];
                    var rigstatus = APIComm.GetRigStatus(last.Port, last.RigName, last.IP).Result;
                    APIComm.Rigs[key] = rigstatus;
                }

                Task.Delay(TimeSpan.FromMinutes(1)).Wait();
            }
        }
    }

    public class RigStatus
    {
        /// <summary>
        /// Last status from miner api
        /// </summary>
        public SimpleStatus Status { get; set; }

        /// <summary>
        /// Last communication with dashboard
        /// </summary>
        public DateTime LastComm { get; set; }

        /// <summary>
        /// From Config of Miner, optional name/description
        /// </summary>
        public string RigName { get; set; }

        public string Port { get; set; }

        public string IP { get; set; }
    }
}
