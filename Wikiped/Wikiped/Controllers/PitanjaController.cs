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
        private int getKorisnikId;
        public int GetKorisnikId
        {
            get {
                DBBL.DAL.Korisnici kor = Session["Korisnik"] as DBBL.DAL.Korisnici;
                if (kor != null)
                {
                    return kor.KorisnikID;
                }
                return 0; 
            }
        }
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
            ViewBag.GetKorisnikName = new Func<int, string>(GetKorisnikName);
            ViewBag.GetBrojOdgovora = new Func<int, int>(GetBrojOdgovora);
            return View();
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
                korisnik=pt.GetKorisnikByID(id);
            }
            if (korisnik != null)
            {
                return korisnik.UserName;
            }
            return string.Empty;
        }
        [HttpPost]
        public ActionResult Index(string btnSearch, string search)
        {
            LucenePt.ClearLuceneIndex();
            LucenePt.AddUpdateLuceneIndex(LucenePt.GetAllObjectForLuceneIndex());

            List<LuceneObject> pitanja = new List<LuceneObject>();
            ViewBag.StackOQuest = new List<Question>();
            if (!String.IsNullOrEmpty(search))
            {
                pitanja = LucenePt.Search(search).ToList();
                ExternalIntegration t = new ExternalIntegration();
                ViewBag.StackOQuest = t.SearchStackOverflow(search);
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
            int idZanjdeg = default(int);
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
            pitanje.KorisnikID = GetKorisnikId;
            
            using (Pitanja pt = new Pitanja())
            {
                idZanjdeg = pt.AddPitanja(pitanje);

                foreach (var item in tagsA)
                {
                    int idTaga = pt.GetTagIdByName(item);
                    Wikiped.DBBL.DAL.TagoviPitanja tp = new DBBL.DAL.TagoviPitanja();
                    tp.TagID = idTaga;
                    tp.PitanjeID = idZanjdeg;
                    pt.AddTagsForPitanja(tp);
                }
            }
            LuceneObject lo = new LuceneObject();
            lo.IsQuestion = true;
            lo.Name = title;
            lo.Description = opis;
            lo.Id = idZanjdeg;
            LucenePt.AddUpdateLuceneIndex(lo);
            LucenePt.Optimize();
            return RedirectToAction("Create");
        }

        public ActionResult Details(int id)
        {
          preporukaItembase(1);
            Pitanja pitanje = new Pitanja();
            IEnumerable<object> lstTags;

            //put korisnik ID - get korisnici ids and simularity
            List<PitanjaContains> preporuka = preporukaColaborative(1);

            //Get korisnici by preporuka
            List<DBBL.DAL.Pitanja> pitanja = new List<DBBL.DAL.Pitanja>();
            using (Pitanja pt = new Pitanja())
            {
                pt.SetAllDetaByPitanjeID(id);
                lstTags = pt.GetAllTagsForPitanjeID(id);
                pitanje = pt;

                //foreach (var item in preporuka)
                //{
                //    pitanja.Add(pt.GetPitanjeByID(item.PitanjeID)); 
                //}
                ViewBag.PreporucenaPt = pitanja;
            }
            ViewBag.Tags = lstTags;
            return View(pitanje);
        }

        #region Preporuka item base
        public List<PitanjaProsjek> preporukaItembase(int PtId)
        {
            using (Spajanje s = new Spajanje())
            {

                List<int> tgs = (from cl in s.Context.TagoviPitanja where cl.PitanjeID == PtId select (int)cl.TagID).ToList();
                List<PitanjaTag> TagoviContain = BrojTagovaUPitanju(tgs, PtId);

                Wikiped.DBBL.DAL.Pitanja cls = (from c in s.Context.Pitanja where c.PitanjeID == PtId select c).FirstOrDefault();

                Wikiped.DBBL.DAL.TagVrste vrste = (from v in s.Context.TagVrste where v.TagVrstaID == cls.TagVrstaID select v).FirstOrDefault();

                double prosjek = (double)cls.BrojGlasova;
                int ptid = (int)cls.PitanjeID;
                List<PitanjaProsjek> pros = ProsjecnaOcjenaPoKategoriji(prosjek, ptid, vrste.Klasa);

                List<PitanjaProsjek> Preporuka = SumiranjeProsjeka(pros, TagoviContain, (double)cls.BrojPregleda);

                List<PitanjaProsjek> PreporukaFinal = BrojPregleda(Preporuka, (int)cls.BrojPregleda);
                return PreporukaFinal;
            }

        }
        public List<PitanjaProsjek> ProsjecnaOcjenaPoKategoriji(double prosjek, int ptID, string vrsta)
        {

            using (Spajanje s = new Spajanje())
            {
                List<Wikiped.DBBL.DAL.TagVrste> vrste = (from tv in s.Context.TagVrste where tv.Klasa == vrsta select tv).ToList();
                List<PitanjaProsjek> ProsjekOcjenaFinal = new List<PitanjaProsjek>();
                foreach (var tgv in vrste)
                {



                    List<PitanjaProsjek> ProsjekOcjenaPovis = (from cc in s.Context.Pitanja
                                                              where cc.BrojGlasova > prosjek
                                                              && cc.TagVrstaID == tgv.TagVrstaID
                                                              orderby cc.BrojGlasova ascending
                                                              select
                                                                  new PitanjaProsjek { PitanjeId = cc.PitanjeID, Prosjek = (double)cc.BrojGlasova }).Take(5).ToList();

                    List<PitanjaProsjek> ProsjekOcjenaIspod = (from cc in s.Context.Pitanja
                                                               where cc.BrojGlasova <= prosjek
                                                              && cc.TagVrstaID == tgv.TagVrstaID
                                                               orderby cc.BrojGlasova descending
                                                              select
                                                                  new PitanjaProsjek { PitanjeId = cc.PitanjeID, Prosjek = (double)cc.BrojGlasova }).Take(5).ToList();
                    ProsjekOcjenaFinal.AddRange(ProsjekOcjenaPovis);
                    ProsjekOcjenaFinal.AddRange(ProsjekOcjenaIspod);
                }

                Wikiped.DBBL.DAL.Pitanja temp;
                double postotak = 23;
                double razmak;
                int ids;

                for (int i = 0; i < ProsjekOcjenaFinal.Count; i++)
                {
                    //temp = null;
                    ids = ProsjekOcjenaFinal[i].PitanjeId;
                    temp = (from c in s.Context.Pitanja where c.PitanjeID == ids select c).FirstOrDefault();

                    if (temp.BrojGlasova == prosjek)
                    {
                        ProsjekOcjenaFinal[i].Prosjek =postotak;
                    }
                    else
                    {
                        if (temp.BrojGlasova != 0 && prosjek != 0)
                        {

                            if (temp.BrojGlasova > prosjek)
                            {

                                razmak = (double)(temp.BrojGlasova - prosjek);
                                ProsjekOcjenaFinal[i].Prosjek = (postotak - ((postotak / (double)temp.BrojGlasova) * razmak));

                            }
                            else
                            {
                                razmak = (double)(prosjek - temp.BrojGlasova);
                                ProsjekOcjenaFinal[i].Prosjek = (postotak - ((postotak / prosjek) * razmak));

                            }
                        }
                    }
                    ProsjekOcjenaFinal[i].Prosjek += 22.0;

                }




                ProsjekOcjenaFinal = (from p in ProsjekOcjenaFinal orderby p.Prosjek descending where p.PitanjeId != ptID select p).ToList();

                return ProsjekOcjenaFinal;
            }
        }
        public List<PitanjaProsjek> BrojPregleda(List<PitanjaProsjek> Prosjeci, int brojPregledaOrig)
        {
            try
            {


                using (Spajanje s = new Spajanje())
                {
                    Wikiped.DBBL.DAL.Pitanja temp;
                    double postotak = 22;
                    double razmak;
                    int ids;
                    for (int i = 0; i < Prosjeci.Count; i++)
                    {
                        //temp = null;
                        ids = Prosjeci[i].PitanjeId;
                        temp = (from c in s.Context.Pitanja where c.PitanjeID == ids select c).FirstOrDefault();

                        if (temp.BrojPregleda == brojPregledaOrig)
                        {
                            Prosjeci[i].Prosjek = Prosjeci[i].Prosjek + postotak;
                        }
                        else
                        {
                            if (temp.BrojPregleda != 0 && brojPregledaOrig != 0)
                            {

                                if (temp.BrojPregleda > brojPregledaOrig)
                                {

                                    razmak = (double)(temp.BrojPregleda - brojPregledaOrig);
                                    Prosjeci[i].Prosjek = Prosjeci[i].Prosjek + (postotak - ((postotak / (double)temp.BrojPregleda) * razmak));

                                }
                                else
                                {
                                    razmak = (double)(brojPregledaOrig - temp.BrojPregleda);
                                    Prosjeci[i].Prosjek = Prosjeci[i].Prosjek + (postotak - ((postotak / brojPregledaOrig) * razmak));

                                }
                            }
                        }

                    }

                }
            }
            catch (Exception)
            {

                return Prosjeci;
            }
            return Prosjeci;

        }
        public List<PitanjaTag> BrojTagovaUPitanju(List<int> tgs, int clID)
        {
            List<PitanjaTag> ptTag;
            PitanjaTag pttemp;
            List<PitanjaTag> ptTagFin = new List<PitanjaTag>();

            using (Spajanje s = new Spajanje())
            {

                ptTag = s.Context.TagoviPitanja.Join(s.Context.Tags, tg => tg.TagID, t => t.TagID, (
                    tg, t) => new { TagPitanja = tg, Tags = t }).Where(
                    q => tgs.Contains(q.Tags.TagID)).Select(x => new PitanjaTag
                    {
                        PitanjeId = (int)x.TagPitanja.PitanjeID,
                        PitanjeCount = (int)x.TagPitanja.PitanjeID,
                        Prosjek = (double)0
                    }).ToList();

                foreach (var it in ptTag)
                {
                    pttemp = null;
                    pttemp = new PitanjaTag();
                    pttemp.PitanjeId = it.PitanjeId;
                    pttemp.PitanjeCount = ptTag.Where(x => x.PitanjeId == pttemp.PitanjeId).Select(x => x.PitanjeId).ToList().Count;
                    ptTagFin.Add(pttemp);
                }

                ptTag = ptTagFin.Distinct(new PitanjaPoredjenje()).ToList();

                ptTag = (from cc in ptTag orderby cc.PitanjeCount descending where cc.PitanjeId != clID select cc).ToList();
                float procentniDio = 33;
                float brojTagova = (float)tgs.Count;
                float OsnovnaJedinica = procentniDio / brojTagova;
                ptTag = (from cc in ptTag
                         orderby cc.Prosjek descending
                         select new PitanjaTag
                         {
                             PitanjeId = cc.PitanjeId,
                             PitanjeCount = cc.PitanjeCount,
                             Prosjek = (cc.PitanjeCount * OsnovnaJedinica)
                         }).ToList();


            }
            return ptTag;

        }
        public double UporediProsjek(double origProsjek, int pitanjeId)
        {
            float procentniDio = 34;
            
            double Ostatak, prosjek;
            using (Spajanje s = new Spajanje())
            {
                Wikiped.DBBL.DAL.Pitanja cla = (from c in s.Context.Pitanja where c.PitanjeID == pitanjeId select c).FirstOrDefault();
                if (cla.BrojGlasova <= origProsjek)
                {
                    Ostatak = origProsjek - (double)cla.BrojGlasova;
                }
                else
                {
                    Ostatak = (double)cla.BrojGlasova - origProsjek;

                }
                
                double OsnovnaJedinica;
                if (origProsjek > cla.BrojGlasova)
                {
                    OsnovnaJedinica = procentniDio / origProsjek;
                    prosjek = Ostatak * OsnovnaJedinica;
                }
                else
                {
                    if (origProsjek == cla.BrojGlasova)
                    {
                            prosjek = procentniDio;
                    }
                    else
                    {
                        OsnovnaJedinica = procentniDio / (double)cla.BrojGlasova;
                        prosjek = Ostatak * OsnovnaJedinica;
                    }
                }
                

                

            }

            return prosjek;
        }
        public List<PitanjaProsjek> SumiranjeProsjeka(List<PitanjaProsjek> pros, List<PitanjaTag> TagoviContain, double origprosjek)
        {
            PitanjaProsjek pttemp;
            PitanjaProsjek pttemp2;
            List<PitanjaProsjek> final = new List<PitanjaProsjek>();
            using (Spajanje s = new Spajanje())
            {
                for (int i = 0; i < TagoviContain.Count; i++)
                {
                    pttemp = null;
                    pttemp = (from p in pros where p.PitanjeId == TagoviContain[i].PitanjeId select p).FirstOrDefault();
                    if (pttemp != null)
                    {
                        pttemp2 = new PitanjaProsjek();
                        pttemp2.PitanjeId = pttemp.PitanjeId;
                        pttemp2.Prosjek = pttemp.Prosjek + TagoviContain[i].Prosjek;
                        pros.Remove(pttemp);
                        pros.Add(pttemp2);

                    }
                    else
                    {
                        pttemp2 = new PitanjaProsjek();
                        pttemp2.PitanjeId = TagoviContain[i].PitanjeId;
                        pttemp2.Prosjek = pttemp2.Prosjek + UporediProsjek(origprosjek, TagoviContain[i].PitanjeId);
                        pros.Add(pttemp2);
                    }
                }
                pros = (from p in pros orderby p.Prosjek descending select p).Take(5).ToList();
                return pros;
            }

        }
        #endregion


        #region Preporuka colaborative
        public List<PitanjaContains> preporukaColaborative(int userId)
        {
            List<PitanjaContains> PreporukaFinal;

            List<PitanjaGlasovi> MyPitanja = getAllMyPitanjaByVote(userId);

            List<int> UsersIds = new List<int>();
            UsersIds.Add(userId);

            if (getAllMyBrojPitanja(userId) > 0)
            {

                List<PitanjaGlasovi> OtherPitanja = getAllOtherPitanjaByVote(MyPitanja, userId);
                double brojPitanja = (double)MyPitanja.Count;
                List<PitanjaContains> otherContains = getAllPitanjaByContains(OtherPitanja, brojPitanja);

                PreporukaFinal = otherContains.Take(5).ToList();

                if (PreporukaFinal.Count < 5)
                {
                    int top = 5 - PreporukaFinal.Count;
                    UsersIds.AddRange((from p in PreporukaFinal select p.KorisnikID).ToList());

                    PreporukaFinal.AddRange(getTop5PitanjaDef(top, UsersIds));

                }

            }
            else
            {
                PreporukaFinal = getTop5PitanjaDef(5, UsersIds);

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
                    glasPitanjeUsers.AddRange((from gp in s.Context.GlasoviZaPitanja
                                        where (gp.PitanjeID == pt.PitanjeId && gp.Glas == pt.Glas && gp.KorisnikID != userId)
                                        select new PitanjaGlasovi
                                        {
                                            KorisnikID = (int)gp.KorisnikID,
                                            PitanjeId = (int)gp.PitanjeID,
                                            Glas = (int)gp.Glas
                                        }).ToList());
                }
                return glasPitanjeUsers;
            }

        }
        public List<PitanjaContains> getAllPitanjaByContains(List<PitanjaGlasovi> otherPitanja, double brojPitanja)
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
        public List<PitanjaContains> getTop5PitanjaDef(int top, List<int> UsesIds)
        {
            using (Spajanje s = new Spajanje())
            {

                try
                {

                    List<int> userIdsN = (from p in s.Context.Pitanja orderby p.BrojGlasova, p.BrojPregleda descending select (int)p.KorisnikID).Distinct().Take(5).ToList();

                    List<int> userIdsF = userIdsN.Where(x => !UsesIds.Contains(x)).ToList();

                    List<PitanjaContains> preporukaFinal = s.Context.Korisnici.Where(x => userIdsF.Contains(x.KorisnikID)).Select(x => new PitanjaContains
                    {
                        KorisnikID = x.KorisnikID,
                        Contains = 0
                    }).Take(top).ToList();
                    return preporukaFinal;
                }
                catch (Exception)
                {

                    return null;
                }
            }

        } 
        //public List<PitanjaPreporuka> getTop5Pitanja(List<PitanjaGlasovi> myPitanja, List<PitanjaContains> otherUser)
        //{
        //    using (Spajanje s = new Spajanje())
        //    {
        //        List<int> mojaPitanjaID = (from m in myPitanja select m.PitanjeId).ToList();

        //        List<PitanjaPreporuka> pitanjaPreporuka;

        //        List<PitanjaPreporuka> pitanjaFinalPreporuka = new List<PitanjaPreporuka>();

        //        List<PitanjaGlasovi> KorisnikPitanja;
        //        foreach (PitanjaContains p in otherUser)
        //        {
        //            pitanjaPreporuka = null;
        //            KorisnikPitanja = null;
        //            KorisnikPitanja = getAllMyPitanjaByVote(p.KorisnikID);

        //            pitanjaPreporuka = KorisnikPitanja.Where(
        //                x => !mojaPitanjaID.Contains((int)x.PitanjeId)).Select(x => new PitanjaPreporuka
        //            {
        //                Contains = p.Contains,
        //                PitanjeID = x.PitanjeId
        //            }).Take(5).ToList();
        //            pitanjaFinalPreporuka.AddRange(pitanjaPreporuka);
        //            if (pitanjaFinalPreporuka.Count > 4)
        //            {
        //                break;
        //            }
        //        }
        //        return pitanjaFinalPreporuka.Take(5).ToList();

        //    }

        //    //clTag = s.Context.TagClanci.Join(s.Context.Tags, tg => tg.TagID, t => t.TagID, (
        //    //        tg, t) => new { TagClanci = tg, Tags = t }).Where(
        //    //        q => tgs.Contains(q.Tags.TagID)).Select(x => new ClanakTag
        //    //        {
        //    //            ClanakId = (int)x.TagClanci.ClanakID,
        //    //            ClanakCount = (int)x.TagClanci.ClanakID,
        //    //            Prosjek = (double)0
        //    //        }).ToList();

        //}
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
                o.KorisnikID = GetKorisnikId;
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
            int korisnikId = GetKorisnikId;
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
            int korisnikId = GetKorisnikId;
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
                tempOdg.KorisnikID = GetKorisnikId;
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
    //public class PitanjaPreporuka
    //{
    //    public int PitanjeID { get; set; }
    //    public double Contains { get; set; }
    //}
    public class PitanjaPoredjenje : IEqualityComparer<PitanjaTag>
    {
        #region IEqualityComparer<Post> Members

        public bool Equals(PitanjaTag x, PitanjaTag y)
        {
            return x.PitanjeId.Equals(y.PitanjeId);
        }

        public int GetHashCode(PitanjaTag obj)
        {
            return obj.PitanjeId.GetHashCode();
        }

        #endregion
    }
    public class PitanjaProsjek
    {
        public int PitanjeId { get; set; }
        public double Prosjek { get; set; }
    }
    public class PitanjaTag
    {
        public int PitanjeId { get; set; }
        public int PitanjeCount { get; set; }
        public double Prosjek { get; set; }
    }
}
