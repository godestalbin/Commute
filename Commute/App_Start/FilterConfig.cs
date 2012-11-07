using System.Web;
using System.Web.Mvc;

namespace Commute
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new System.Web.Mvc.AuthorizeAttribute()); //Set [Authorize] attribute for all controllers
        }
    }
}