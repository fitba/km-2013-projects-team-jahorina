using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.Models;

namespace Wikiped.Controllers
{
    public class TagController : Controller
    {
        //
        // GET: /Tag/

        public ActionResult Index()
        {
            ExternalIntegration t = new ExternalIntegration();
            //t.SearchStackOverflow("asp.net");
            t.SearchWikipedia("Mike Tyson");
            return View();
        }
        public ActionResult Details(string name)
        {
            using (Wikiped.Models.Pitanja pt = new Wikiped.Models.Pitanja())
            {
                if (!String.IsNullOrEmpty(name))
                {
                    ViewBag.Pitanja = pt.GetAllPitanjaByTagName(name);
                    ViewBag.Sadrzaji = pt.GetAllSadrzajiByTagName(name);
                }
                ViewBag.Tagovi = pt.GetAllTagsCount().Take(15);
            }
            return View();
        }

    }
}
