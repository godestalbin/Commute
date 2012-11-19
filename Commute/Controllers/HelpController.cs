using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Commute.Controllers
{
    public class HelpController : Controller
    {
        //Help from Amazon S3 displayed in iframe
        [AllowAnonymous]
        public ActionResult Help()
        {
            return View();
        }

    }
}
