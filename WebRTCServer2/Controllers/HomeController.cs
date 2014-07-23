using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRTCServer2.Filters;

namespace WebRTCServer2.Controllers
{
     [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            ViewBag.basePath = Request.Url.AbsoluteUri;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
