    using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.DBBL.BLL;
using Wikiped.DBBL.DAL;
using Wikiped.Models;

namespace Wikiped.Controllers
{
    public class IndexController : Controller
    {
        //
        // GET: /Index/
        //[CustomAuthorize(Roles = Wikiped.Models.CustomAuthorizeAttribute.SiteRoles.Admin | Wikiped.Models.CustomAuthorizeAttribute.SiteRoles.Helpdesk)]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string regUser, string regEmail, string regPass)
        {
            using (Spajanje s = new Spajanje())
            {
                Korisnici kor=null;
                   kor = (from ko in s.Context.Korisnici where ko.UserName==regUser select ko).FirstOrDefault();
                   if (kor == null)
                   {
                       kor = (from ko in s.Context.Korisnici where ko.Email == regEmail select ko).FirstOrDefault();
                       if (kor == null)
                       {
                           kor = new Korisnici();
                           kor.UserName = regUser;
                           kor.Email = regEmail;
                           kor.Password = regPass;
                           s.Context.Korisnici.AddObject(kor);
                           s.Context.SaveChanges();

                       }
                       else
                       {
                           ViewBag.regGreska = "* E-mail postoji!";
                       }
                   }
                   else
                   {
                       ViewBag.regGreska = "* Korisnik postoji!";
                   }

                

            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string logUser, string logPass)
        {
            using (Spajanje s = new Spajanje())
            {
                Korisnici k = (from ko in s.Context.Korisnici where (ko.UserName == logUser) && (ko.Password == logPass) select ko).SingleOrDefault();
                if (k != null)
                {
                    Session.Add("Korisnik", k);
                    
                    ViewBag.uspjesno = "";
                   return RedirectToAction("Index", "Pocetna");
                }
                else
                {
                    ViewBag.uspjesno = "Greska pri prijavi!";
                }

            }
            return View("Index");
        }
        public ActionResult Fit()
        {
            return Redirect("http://www.fit.ba");
        }

    }
}
