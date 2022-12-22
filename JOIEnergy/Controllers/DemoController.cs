using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace JOIEnergy.Controllers
{
    [Route("demo")]
    public class DemoController : Controller
    {
        public DemoController()
        {
            
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Content(DateTime.Now.ToString());
        }

    }
}