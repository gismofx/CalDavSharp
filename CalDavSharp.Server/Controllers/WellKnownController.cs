using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CalDavSharp.Server.Controllers
{
    [ApiController]
    public class WellKnownController : Controller
    {
        [AcceptVerbs("PROPFIND")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(".well-known/caldav", Order = 1)]
        public IActionResult WellKnownCaldav()
        {
            HttpContext.Response.Headers.Add("Location", "~/principals/");
            HttpContext.Response.StatusCode = 301;
            return new RedirectResult("~/principals",true,true);
            //return RedirectPermanentPreserveMethod("~/principals/");
        }
    }
}
