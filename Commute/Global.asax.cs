using Cloudinary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Validation.Providers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Commute
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Cloudinary initialization
            var settings = ConfigurationManager.AppSettings;
            var configuration = new AccountConfiguration(settings["Cloudinary.CloudName"],
                                                         settings["Cloudinary.ApiKey"],
                                                         settings["Cloudinary.ApiSecret"]);
            
            //Fix issue related to manually generated key for Route.RouteId
            //http://stackoverflow.com/questions/12305784/dataannotation-for-required-property
            GlobalConfiguration.Configuration.Services.RemoveAll(
typeof(System.Web.Http.Validation.ModelValidatorProvider),
v => v is InvalidModelValidatorProvider);

            AccountConfiguration.Initialize(configuration);
        }
    }
}