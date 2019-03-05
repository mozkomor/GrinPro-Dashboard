using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMonitor.Models
{
    public class ConnectionOptions
    {
        StratumConnectionOption Connection1 { get; set; }
        StratumConnectionOption Connection2 { get; set; }
        StratumConnectionOption Connection3 { get; set; }
    }

    public class StratumConnectionOption
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Ssl { get; set; }
        public int Priority { get; set; }

    }
}
