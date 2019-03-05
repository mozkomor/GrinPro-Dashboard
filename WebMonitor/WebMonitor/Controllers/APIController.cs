using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GrinProMiner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        //private readonly IHttpClientFactory _clientFactory;

        //public APIController(IHttpClientFactory clientFactory)
        //{
        //    _clientFactory = clientFactory;
        //}

        [HttpGet]
        [Route("hello/{port?}")]
        public async Task<IActionResult> Hello(string port = "", string name = "")
        {
            var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4();
            port = string.IsNullOrEmpty(port) ? "5777" : port;
            var r = await APIComm.GetRigStatus(port, name, ip.ToString());
            return (r != null) ? (IActionResult)Ok() : StatusCode(500, "Expecting SimpleStatus");
        }

        //private async Task<IActionResult> GetRigStatus(string port, string name, string ip)
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, $"http://{ip}:{port}/api/status");
        //    var client = _clientFactory.CreateClient();

        //    try
        //    {
        //        var response = await client.SendAsync(request);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            SimpleStatus s = await response.Content.ReadAsAsync<SimpleStatus>();
        //            RigStatus r = new RigStatus() { LastComm = DateTime.Now, Port = port, RigName = name, Status = s, IP = ip };
        //            APIComm.Rigs[ip.ToString()] = r;
        //            APIComm.SaveRigsToFile();
        //            return Ok();
        //        }
        //        else
        //        {
        //            return StatusCode(500, "Expecting SimpleStatus");
        //        }
        //    }
        //    catch
        //    {
        //        return StatusCode(500, "Miner not reachable.");
        //    }
        //}
    }
}