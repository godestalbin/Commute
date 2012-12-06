using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.IO; //Upload file (write file on server)

namespace Commute.Controllers
{
    public class HelpController : Controller
    {
        //Help from Amazon S3 displayed in iframe
        [AllowAnonymous]
        public ActionResult Help(string helpFile)
        {
            string culture = Thread.CurrentThread.CurrentUICulture.Name; //en-US 
            string[] language = culture.Split( new Char[] {'-'});
            string helpFileNoExtension = Path.GetFileNameWithoutExtension(helpFile);
            string helpFileExtension = Path.GetExtension(helpFile);
            string helpLanguage = "";
            switch (helpFileNoExtension)
            {
                case "Commute documentation": //List file that have been translated
                case "Screen - Home - Welcome":    
                    switch (language[0])
                    {
                        case "fr":
                            helpLanguage = ".fr-FR";
                            break;
                    }
                    break;
            }

            ViewBag.HelpFile = System.Configuration.ConfigurationManager.AppSettings["Http.Documentation"] + helpFileNoExtension + helpLanguage + helpFileExtension; 
            return View();
        }

        [AllowAnonymous]
        public ActionResult Debug()
        {
            string uiCulture = Thread.CurrentThread.CurrentUICulture.Name; //en-US 
            string culture = Thread.CurrentThread.CurrentCulture.Name; //en-US 
            ViewBag.UiCulture = uiCulture;
            ViewBag.Culture = culture;
            return View();
        }

    }
}
