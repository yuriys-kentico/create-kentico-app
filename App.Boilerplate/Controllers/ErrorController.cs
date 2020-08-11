using System;
using System.Net;
using System.Web.Mvc;

using App.Boilerplate.Mvc;

namespace App.Boilerplate.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult Index(Exception exception)
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return new RewriteToPageTreeResult("500");
        }

        [HttpGet]
        public ActionResult Http404()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;

            return new RewriteToPageTreeResult("404");
        }
    }
}