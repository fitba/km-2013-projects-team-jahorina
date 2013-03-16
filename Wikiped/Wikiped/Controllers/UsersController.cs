using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.Models;

namespace Wikiped.Controllers
{
    public class UsersController : Controller
    {
        //
        // GET: /Users/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Users/Details/5

        public ActionResult Details(int id)
        {
            List<DBBL.DAL.Pitanja> lstPitanja = new List<DBBL.DAL.Pitanja>();
            List<DBBL.DAL.Sadrzaji> lstSadrzaji = new List<DBBL.DAL.Sadrzaji>();
            using (Pitanja pt = new Pitanja())
            {
                lstPitanja.AddRange(pt.GetAllPitanjaByKorisnikId(id));
                lstPitanja.AddRange(pt.GetAllPitanjaOdgovoriByKorisnikId(id));
                lstSadrzaji.AddRange(pt.GetAllClanciByKorisnikId(id));
                ViewBag.Tagovi = pt.GetAllTagsCount().Take(15);
            }
            ViewBag.Sadrzaji = lstSadrzaji;
            ViewBag.GetTags = new Func<int, IEnumerable<string>>(GetAllTagsForPitanje);
            ViewBag.GetKorisnikName = new Func<int, string>(GetKorisnikName);
            ViewBag.GetBrojOdgovora = new Func<int, int>(GetBrojOdgovora);
            ViewBag.AllPitanja = lstPitanja.Distinct().ToList();
            return View();
        }
        public IEnumerable<string> GetAllTagsForPitanje(int id)
        {
            IEnumerable<string> lstTags;
            using (Pitanja pt = new Pitanja())
            {
                lstTags = pt.GetAllTagsForPitanjeID(id);
            }
            return lstTags;
        }
        public int GetBrojOdgovora(int id)
        {
            int count = default(int);
            using (Pitanja pt = new Pitanja())
            {
                count = pt.GetBrojOdgovoraZaPitanje(id);
            }
            return count;
        }
        public string GetKorisnikName(int id)
        {
            DBBL.DAL.Korisnici korisnik;
            using (Pitanja pt = new Pitanja())
            {
                korisnik = pt.GetKorisnikByID(id);
            }
            if (korisnik != null)
            {
                return korisnik.UserName;
            }
            return string.Empty;
        }

        //
        // GET: /Users/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Users/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Users/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Users/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Users/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Users/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
