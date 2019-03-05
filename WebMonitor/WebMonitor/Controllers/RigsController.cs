using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GrinProMiner.Models;
using Microsoft.AspNetCore.Mvc;
using Mozkomor.GrinGoldMiner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebMonitor.Models;

namespace WebMonitor.Controllers
{
    public class RigsController : Controller
    {
        //private readonly IHttpClientFactory _clientFactory;
        //public RigsController(IHttpClientFactory clientFactory)
        //{
        //    _clientFactory = clientFactory;
        //}

        public IActionResult Index()
        {
            var rigs = new List<RigViewModel>();
            foreach (var rig in APIComm.Rigs)
            {
                RigStatus rigInfo = rig.Value;
                string rigIP = rig.Key;

                rigs.Add(
                    new RigViewModel()
                    {
                        Address = rigIP,
                        Description = rigInfo.RigName,
                        ConnectionAddress = rigInfo.Status.ConnectionAddress,
                        ConnectionStatus = rigInfo.Status.ConnectionStatus,
                        LastJob = rigInfo.Status.LastJob,
                        LastShare = rigInfo.Status.LastShare,
                        Shares = rigInfo.Status.Shares,
                        Workers = rigInfo.Status.Workers,
                        //Uptime = TimeSpan.FromHours(1),
                        LastCommunication = rigInfo.LastComm,
                        }
                    );
            }

            ViewBag.Rigs = rigs;

            ViewBag.MessageNotAdded = TempData["NotAdded"];
            ViewBag.Added = (int?)TempData["Added"];
            return View();
        }

        [Route("Rigs/{rig}")]
        public IActionResult Status(string rig)
        {
            //known rig
            if (APIComm.Rigs.ContainsKey(rig))
            {
                //fetch fresh stats
                var last = APIComm.Rigs[rig];
                var rigstatus = APIComm.GetRigStatus(last.Port,last.RigName,last.IP).Result;

                //we hafe fresh stats
                if (rigstatus != null)
                {
                    return View(APIComm.Rigs[rig]);
                }
                else
                {
                    ViewBag.WasNotAbleToConnect = true;
                    return View(last);
                }
            }
            else
            {
                var rigstatus = APIComm.GetRigStatus("5777", "", rig).Result;

                //we hafe fresh stats
                if (rigstatus != null)
                {
                    return View(APIComm.Rigs[rig]);
                }
                else
                {
                    return StatusCode(404, "Rig not found");
                }
            }
                
        }

        [Route("Rigs/{rig}/Connection")]
        public IActionResult SetPrimaryConnectioins(string rig)
        {
            ViewBag.Address = rig;
            var rigConfig = APIComm.GetRigConfig(rig).Result;
            var primaryConn = rigConfig.PrimaryConnection;
            
            return View("SetConnection", primaryConn);
        }

        [HttpPost]
        [Route("Rigs/{rig}/Connection")]
        public IActionResult SetPrimaryConnectioins(string rig, Connection connection)
        {
            var succ = APIComm.PostRigPrimaryConnection(rig, connection).Result;
            if (succ)
                return Json(new { success = true });
            else
                return Json(new { success = false });
        }

        [Route("Rigs/{rig}/Config")]
        public IActionResult Config(string rig)
        {
            var rigConfig = APIComm.GetRigConfig(rig).Result;
            ViewBag.Address = rig;
            return View("Config", rigConfig);
        }

        [HttpPost]
        [Route("Rigs/{rig}/Config")]
        public IActionResult Config(Config config, string rig)
        {
            var succ = APIComm.PostRigConfig(rig,config).Result;
            if (succ)
                return Json(new { success = true });
            else
                return Json(new { success = false });
        }

        [HttpGet]
        [Route("Rigs/add")]
        public IActionResult AddMany()
        {
            return View("Add");
        }

        [HttpPost]
        [Route("Rigs/add")]
        public IActionResult AddMany(string rigs)
        {
            var splitted = rigs.Split(new string[] { Environment.NewLine, "\r\n", "\r", "\n", " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            List<string> notAdded;
            int added;
            APIComm.AddMultipleRigs(splitted, out notAdded, out added);
            TempData["Added"] = added; //TempData survive redirect (uses session under the hood)
            TempData["NotAdded"] = string.Join(", ", notAdded);
            return RedirectToAction("Index");
        }


        [Route("Rigs/{rig}/remove")]
        public IActionResult Remove(string rig)
        {
            var rigStatus = APIComm.Rigs[rig];

            APIComm.Rigs.Remove(rig, out RigStatus stat);
            APIComm.SaveRigsToFile();

            return RedirectToAction("Index");
        }
    }
}