using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.DBBL.BLL;
using Wikiped.DBBL.DAL;

namespace Wikiped.Controllers.Register
{
    public class PocetnaController : Controller
    {
        //
        // GET: /Pocetna/

        public ActionResult Index()
        {
            Korisnici kor = Session["Korisnik"] as Korisnici;
            ViewBag.logiran = kor.UserName.ToString();

            return RedirectToAction("Index","Index");
        }
        public ActionResult OProjektu()
        {
       
            return View();

        }
        public ActionResult odjava()
        {
           Session.RemoveAll();
           return RedirectToAction("Index", "Index");
        }
        [ChildActionOnly]
        //[CustomAuthorize(Roles = Wikiped.Models.CustomAuthorizeAttribute.SiteRoles.Admin | Wikiped.Models.CustomAuthorizeAttribute.SiteRoles.Helpdesk)]
        public ActionResult Prijavljen()
        {
            Korisnici kor = Session["Korisnik"] as Korisnici;
            if (kor != null)
            {
                return PartialView("Prijavljen", kor);
            }
            else
            {
                return PartialView("NePrijavljen");
            }

        }

    }
}
