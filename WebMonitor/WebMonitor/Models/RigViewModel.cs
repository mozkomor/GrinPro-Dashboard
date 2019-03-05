using GrinProMiner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMonitor.Models
{
    public class RigViewModel
    {
        /// <summary>
        /// IP of rig on network
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// optional name/descriptionof rig
        /// </summary>
        public string Description { get; set; }

        public string ConnectionAddress { get; set; }
        public string ConnectionStatus { get; set; }
        public string LastJob { get; set; } //seconds
        public string LastShare { get; set; } //seconds

        public ShareStats Shares { get; set; }
        public TimeSpan Uptime { get; set; }
        /// <summary>
        /// With Dashboard
        /// </summary>
        public DateTime LastCommunication { get; set; }
        public List<SimpleWorkerInfo> Workers { get; set; }

        public string GetOneLineStatus() => string.Join(",", Workers.GroupBy(w => w.Status).OrderByDescending(g => g.Count()).Select(g => $"({g.Count()}) {g.First().Status}"));
        public bool GetAllONLINE => Workers.All(w => w.Status.ToUpper() == "ONLINE");
    }
}
