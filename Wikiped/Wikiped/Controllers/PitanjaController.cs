﻿using System;
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
                ViewBag.Tagovi = pt.GetAllTagsCount().Take(15);
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

            List<LuceneObject> pitanja=new List<LuceneObject>();
            ViewBag.StackOQuest = new List<Question>();
            if (!String.IsNullOrEmpty(search))
            {
                pitanja = LucenePt.Search(search).ToList();
                ExternalIntegration t = new ExternalIntegration();
                ViewBag.StackOQuest=t.SearchStackOverflow(search);
            }
            if (pitanja != null)
            {
                pitanja = pitanja.Where(x => x.IsQuestion == true).ToList();
            }
            using (Pitanja pt = new Pitanja())
            {
                ViewBag.Tagovi = pt.GetAllTagsCount().Take(15);
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
        #region Preporuka colaborative
        public List<PitanjaPreporuka> preporukaColaborative(int userId)
        {
            List<PitanjaPreporuka> PreporukaFinal;

            List<PitanjaGlasovi> MyPitanja = getAllMyPitanjaByVote(userId);
            List<int> UsesIds = (from m in MyPitanja select m.PitanjeId).ToList();

            if (getAllMyBrojPitanja(userId) > 0)
            {
               

                List<PitanjaGlasovi> OtherPitanja = getAllOtherPitanjaByVote(MyPitanja, userId);
                double brojPitanja = (double)MyPitanja.Count;
                List<PitanjaContains> otherContains = getAllPitanjaByContains(OtherPitanja, brojPitanja);
                PreporukaFinal = getTop5Pitanja(MyPitanja, otherContains);
                if (PreporukaFinal.Count < 5)
                {
                    int top=5 - PreporukaFinal.Count;
                    UsesIds.AddRange((from p in PreporukaFinal select p.PitanjeID).ToList());
                    PreporukaFinal.AddRange(getTop5PitanjaDef(top, UsesIds));
                    
                }
              
            }
            else
            {
                PreporukaFinal = getTop5PitanjaDef(5, UsesIds);
              
            }
            PreporukaFinal = (from p in PreporukaFinal orderby p.Contains descending select p).ToList();
            return PreporukaFinal;


        }
        public List<PitanjaGlasovi> getAllMyPitanjaByVote(int userId)
        {
            using (Spajanje s = new Spajanje())
            {

                List<PitanjaGlasovi> glasPitanje = (from gp in s.Context.GlasoviZaPitanja
                                                    where gp.KorisnikID == userId
                                                    select new PitanjaGlasovi
                                                    {
                                                        KorisnikID = (int)gp.KorisnikID,
                                                        PitanjeId = (int)gp.PitanjeID,
                                                        Glas = (int)gp.Glas
                                                    }).ToList();

                return glasPitanje;
               
            }

        }
        public double getAllMyBrojPitanja(int userId)
        {
            using (Spajanje s = new Spajanje())
            {

                double brojpitanja = (from gp in s.Context.GlasoviZaPitanja
                                      where gp.KorisnikID == userId
                                                    select gp.PitanjeID).ToList().Count;

                return brojpitanja;

            }

        }
        public List<PitanjaGlasovi> getAllOtherPitanjaByVote(List<PitanjaGlasovi> myPitanja, int userId)
        {
            using (Spajanje s = new Spajanje())
            {
                List<PitanjaGlasovi> glasPitanjeUsers = new List<PitanjaGlasovi>();

                foreach (PitanjaGlasovi pt in myPitanja)
                {
                    glasPitanjeUsers = (from gp in s.Context.GlasoviZaPitanja
                                        where (gp.PitanjeID == pt.PitanjeId && gp.Glas == pt.Glas && gp.KorisnikID != pt.KorisnikID)
                                        select new PitanjaGlasovi
                                        {
                                            KorisnikID = (int)gp.KorisnikID,
                                            PitanjeId = (int)gp.PitanjeID,
                                            Glas = (int)gp.Glas
                                        }).ToList();
                }
                return glasPitanjeUsers;
            }

        }
        public List<PitanjaContains> getAllPitanjaByContains(List<PitanjaGlasovi> otherPitanja,double brojPitanja)
        {
            List<int> idUsers = otherPitanja.Select(x => (int)x.KorisnikID).Distinct().ToList();

            PitanjaContains tempP;
            List<PitanjaContains> KorisniciContains = new List<PitanjaContains>();
            List<PitanjaContains> KorisniciFinal = new List<PitanjaContains>();
            for (int i = 0; i < idUsers.Count; i++)
            {
                tempP = null;
                tempP = new PitanjaContains();
                tempP.Contains = (from pt in otherPitanja where pt.KorisnikID == idUsers[i] select pt.KorisnikID).ToList().Count();
                tempP.KorisnikID = idUsers[i];
                KorisniciContains.Add(tempP);
            }
            KorisniciContains = (from kc in KorisniciContains orderby kc.Contains descending select kc).ToList();
            double postotak;
            for (int i = 0; i < KorisniciContains.Count; i++)
            {
                if (getAllMyBrojPitanja(KorisniciContains[i].KorisnikID) > brojPitanja)
                {
                    postotak = 100 / getAllMyBrojPitanja(KorisniciContains[i].KorisnikID);
                    KorisniciContains[i].Contains = KorisniciContains[i].Contains * postotak;

                }
                else
                {
                    postotak = 100 / brojPitanja;
                    KorisniciContains[i].Contains = KorisniciContains[i].Contains * postotak;

                }
            }
            KorisniciFinal = (from kr in KorisniciContains orderby kr.Contains descending select kr).ToList();
            return KorisniciFinal;
        }
        public List<PitanjaPreporuka> getTop5Pitanja(List<PitanjaGlasovi> myPitanja, List<PitanjaContains> otherUser)
        {
            using(Spajanje s= new Spajanje()){
            List<int> mojaPitanjaID=(from m in myPitanja select m.PitanjeId).ToList();

            List<PitanjaPreporuka> pitanjaPreporuka;

            List<PitanjaPreporuka> pitanjaFinalPreporuka=new List<PitanjaPreporuka>();
            
            List<PitanjaGlasovi> KorisnikPitanja;
            foreach (PitanjaContains p in otherUser)
            {
                pitanjaPreporuka = null;
                KorisnikPitanja=null;
                KorisnikPitanja=getAllMyPitanjaByVote(p.KorisnikID);

                pitanjaPreporuka = KorisnikPitanja.Where(
                    x => !mojaPitanjaID.Contains((int)x.PitanjeId)).Select(x => new PitanjaPreporuka
                {
                    Contains = p.Contains,
                    PitanjeID = x.PitanjeId
                }).Take(5).ToList();
                pitanjaFinalPreporuka.AddRange(pitanjaPreporuka);
                if (pitanjaFinalPreporuka.Count > 4)
                {
                    break;
                }
            }
            return pitanjaFinalPreporuka.Take(5).ToList();

            }

            //clTag = s.Context.TagClanci.Join(s.Context.Tags, tg => tg.TagID, t => t.TagID, (
            //        tg, t) => new { TagClanci = tg, Tags = t }).Where(
            //        q => tgs.Contains(q.Tags.TagID)).Select(x => new ClanakTag
            //        {
            //            ClanakId = (int)x.TagClanci.ClanakID,
            //            ClanakCount = (int)x.TagClanci.ClanakID,
            //            Prosjek = (double)0
            //        }).ToList();

        }
        public List<PitanjaPreporuka> getTop5PitanjaDef(int top, List<int> UsesIds)
        {
            using (Spajanje s = new Spajanje())
            {
                List<PitanjaPreporuka> preporukaFinal=s.Context.Pitanja.Where(x=>!UsesIds.Contains(x.PitanjeID)).OrderByDescending(
                    x=>x.BrojGlasova).ThenByDescending(x=>x.BrojPregleda).Select(x=> new PitanjaPreporuka {
                        PitanjeID = x.PitanjeID,
                        Contains = 0
                    }).Take(top).ToList();
                return preporukaFinal;
            }

        }
        #endregion
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
