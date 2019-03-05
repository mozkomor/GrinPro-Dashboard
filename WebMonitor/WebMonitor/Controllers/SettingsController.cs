using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Models;

namespace WebMonitor.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Connections");
        }

        public IActionResult Connections()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Connections(ConnectionOptions connectionOptions)
        {
            if (ModelState.IsValid)
            {
                //todo save connections
                return View();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}