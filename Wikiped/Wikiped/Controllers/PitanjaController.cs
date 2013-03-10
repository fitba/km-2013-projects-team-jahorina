using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.Models;
using Wikiped.DBBL.BLL;


namespace Wikiped.Controllers
{
    public class PitanjaController : Controller
    {
        //
        // GET: /Pitanja/

        public ActionResult Index()
        {
            //LucenePt.ClearLuceneIndex();
            List<DBBL.DAL.Pitanja> pitanja;
            using (Pitanja pt = new Pitanja())
            {
                pitanja = pt.GetAllPitanja();
                ViewBag.AllPitanja = pitanja;
            }
            ViewBag.Search = false;
            ViewBag.GetTags = new Func<int, IEnumerable<string>>(GetAllTagsForPitanje);
            return View();
        }

        [HttpPost]
        public ActionResult Index(string btnSearch, string search)
        {
            LucenePt.ClearLuceneIndex();
            LucenePt.AddUpdateLuceneIndex(LucenePt.GetAllObjectForLuceneIndex());

            List<LuceneObject> pitanja;
            pitanja = LucenePt.Search(search).ToList();
            if (pitanja != null)
            {
                pitanja = pitanja.Where(x => x.IsQuestion == true).ToList();
            }
            ViewBag.AllPitanja = pitanja;
            ViewBag.Search = true;
            ViewBag.GetTags = new Func<int, IEnumerable<string>>(GetAllTagsForPitanje);
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
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(string tags, string title, string opis)
        {
            string[] tagsA = null;
            if (tags != String.Empty)
            {
                tagsA = tags.Split(',');
            };
            DBBL.DAL.Pitanja pitanje = new DBBL.DAL.Pitanja();
            pitanje.Naziv = title;
            pitanje.Opis = opis;
            pitanje.Datum = DateTime.Now;
            pitanje.BrojGlasova = 0;
            pitanje.BrojPregleda = 0;

            using (Pitanja pt = new Pitanja())
            {
                int id = pt.AddPitanja(pitanje);

                foreach (var item in tagsA)
                {
                    int idTaga = pt.GetTagIdByName(item);
                    Wikiped.DBBL.DAL.TagoviPitanja tp = new DBBL.DAL.TagoviPitanja();
                    tp.TagID = idTaga;
                    tp.PitanjeID = id;
                    pt.AddTagsForPitanja(tp);
                }
            }
            return RedirectToAction("Create");
        }

        public ActionResult Details(int id)
        {
            Pitanja pitanje = new Pitanja();
            IEnumerable<object> lstTags;
            using (Pitanja pt = new Pitanja())
            {
                pt.SetAllDetaByPitanjeID(id);
                lstTags = pt.GetAllTagsForPitanjeID(id);
                pitanje = pt;
            }
            ViewBag.Tags = lstTags;
            return View(pitanje);
        }
        
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Details(int id, string txtComment)
        {
            if (!String.IsNullOrEmpty(txtComment))
            {
                DBBL.DAL.Odgovori o = new DBBL.DAL.Odgovori();
                o.PitanjeID = id;
                o.Odgovor = txtComment;
                o.Datum = DateTime.Now;
                o.KorisnikID = 1;
                o.BrojGlasova = 0;

                Pitanja pitanje = new Pitanja();
                using (Pitanja pt = new Pitanja())
                {
                    pt.AddAnswer(o);
                    pt.SetAllDetaByPitanjeID(id);
                    pitanje = pt;
                }
            }
            return RedirectToAction("Details", new { ID = id });
        }
        #region Ajax Vote and Comment

        public ActionResult VoteUp(int id, bool mainQuestion)
        {
            int korisnikId = 1;
            int voteNumber = default(int);
            using (Pitanja pt = new Pitanja())
            {
                if (mainQuestion == true)
                {
                    voteNumber = pt.PitanjeVoteUp(id, korisnikId);
                }
                else
                {
                    voteNumber = pt.OdgovorVoteUp(id, korisnikId);
                }
            }

            return Json(new { voteNumber = voteNumber });
        }
        public ActionResult VoteDown(int id, bool mainQuestion)
        {
            int korisnikId = 1;
            int voteNumber = default(int);
            using (Pitanja pt = new Pitanja())
            {
                if (mainQuestion == true)
                {
                    voteNumber = pt.PitanjeVoteDown(id, korisnikId);
                }
                else
                {
                    voteNumber = pt.OdgovorVoteDown(id, korisnikId);
                }
            }
            return Json(new { voteNumber = voteNumber });
        }
        public ActionResult AddCommentForAnswer(int id, string textComm)
        {
            if (!String.IsNullOrEmpty(textComm))
            {
                DBBL.DAL.OdgovorNaOdgovor tempOdg = new DBBL.DAL.OdgovorNaOdgovor();
                tempOdg.OdgovorID = id;
                tempOdg.Odgovor = textComm;
                tempOdg.KorisnikID = 1;
                tempOdg.Datum = DateTime.Now;
                using (Pitanja pt = new Pitanja())
                {
                    pt.AddCommentOnAnswer(tempOdg);
                }
                return Json(new { voteNumber = 1 });
            }
            return View();
        }

        #endregion
        //povis clase mora biti [Serializable]
        public ActionResult GetTags(string id)
        {
            List<DBBL.DAL.Tags> lstTags;
            using (Pitanja pt = new Pitanja())
            {
                lstTags = pt.GetAllTags();
            }

            List<string> lstTagovi = new List<string>();

            List<MyTags> lstMTasg = new List<MyTags>();
            foreach (var item in lstTags)
            {
                MyTags temp = new MyTags();
                temp.Id = item.TagID;
                temp.Name = item.Ime;
                lstMTasg.Add(temp);
            }
            //return Json(lstTagovi);
            return Json(lstMTasg);
        }
    }

    [Serializable]
    public class MyTags
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class PitanjaGlasovi
    {
        public int KorisnikID { get; set; }
        public int PitanjeId { get; set; }
        public int Glas { get; set; }

    }
    public class PitanjaContains
    {
        public int KorisnikID { get; set; }
        public double Contains { get; set; }

    }
    public class PitanjaPreporuka
    {
        public int PitanjeID { get; set; }
        public double Contains { get; set; }

    }

}
