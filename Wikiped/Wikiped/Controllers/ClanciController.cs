using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.Models;
using Wikiped.DBBL.DAL;
using Wikiped.DBBL.BLL;
using System.Web.Script.Serialization;
using System.IO;
using System.Drawing;
namespace Wikiped.Controllers
{
    public class ClanciController : Controller
    {
        //
        // GET: /Clanci/

        public ActionResult Index()
        {
            ClanciServisObrada c = new ClanciServisObrada();
            ViewBag.Search = false;
            return View(c.getAllClanci());
        }
        [HttpPost]
        public ActionResult Index(string btnSearch, string search)
        {
            List<LuceneObject> clanci;
            clanci = LucenePt.Search(search).ToList();
            if (clanci != null)
            {
                clanci = clanci.Where(x => x.IsQuestion == false).ToList();
                if (clanci.Count == 0)
                {
                    ViewBag.HasResult = false;
                }
                else
                {
                    ViewBag.HasResult = true;
                }
            }
            else
            {
                ViewBag.HasResult = false;
            }

            ViewBag.Clanci = clanci;
            ViewBag.Search = true;
            return View();
        }

        public ActionResult Edit(int id)
        {
            ClanciServisObrada c = new ClanciServisObrada();
            return View(ClanciServisObrada.getClanakById(id));
        }
        public JsonResult MogucnostGlasanja(int clanakId)
        {
            if (Session["Korisnik"] == null)
            {
                return Json(new
                {
                    Success = false,
                    Message = "* Morate biti logovani",
                    color = ""
                });
            }
            else
            {
                Korisnici kor = Session["Korisnik"] as Korisnici;
                using (Spajanje sp = new Spajanje())
                {
                    OcjenaKorisnici glasanje = (from co in sp.Context.OcjenaKorisnici
                                                where co.ClanakID == clanakId && co.KorisnikID == kor.KorisnikID
                                                select co).FirstOrDefault();

                    if (glasanje != null)
                    {
                        return Json(new
                        {
                            Success = false,
                            Message = "* Glasali ste za ovaj clanak",
                            color = ""

                        });
                    }



                }

            }
            return Json(new
            {
                Success = true,
                Message = "",
                color = ""
            });
        }
        public ActionResult ClanciPregled(int clID)
        {
            ClanciServis clanak = ClanciServisObrada.getClanakById(clID);
            List<ClanakProsjek> preporukaTemp = preporukaItembase(clID);
            List<int> preporukaIds = (from p in preporukaTemp select p.ClanakId).ToList();


            ViewBag.Preporuka = ClanciServisObrada.getClanciByIds(preporukaIds);
            List<string> Words = getWordForWiki(clID);

            ExternalIntegration t = new ExternalIntegration();

            List<Wiki> clanciWiki = new List<Wiki>();
            foreach (string its in Words)
            {
                clanciWiki.AddRange(t.SearchWikipedia(its));
            }
            clanciWiki = clanciWiki.Take(5).ToList();

            ViewBag.PreporukaWiki = clanciWiki;
            Korisnici kor = Session["Korisnik"] as Korisnici;
            if (kor != null)
            {
            List<ClanciContains> PrepKolab= preporukaColaborative(kor.KorisnikID);
            ViewBag.PreporukaKorisnici = getKorisniciByIds(PrepKolab);
            }
            else{
                ViewBag.PreporukaKorisnici = null;
            }

            

            


            return View(clanak);
        }
        public ActionResult zloupotreba(int zlID)
        {
            try
            {
                string poruka = "";
                bool uspjeh = false;
                if (Session["Korisnik"] != null)
                {
                    using (Spajanje s = new Spajanje())
                    {
                        Zloupotrebe zl = (from z in s.Context.Zloupotrebe where z.KomentarID == zlID select z).FirstOrDefault();

                        if (zl == null)
                        {

                            zl = new Zloupotrebe();
                            zl.KomentarID = zlID;
                            Korisnici kor = Session["Korisnik"] as Korisnici;
                            zl.KorisnikID = kor.KorisnikID;
                            s.Context.Zloupotrebe.AddObject(zl);
                            s.Context.SaveChanges();

                            uspjeh = true;
                            poruka = "Uspješno prijavljen komentar";
                            //spremi u bazu i vrati stanje
                        }
                        else
                        {
                            poruka = "Prijavili ste ovaj komentar";
                            //error vec ste glasali
                        }
                    }
                }
                else
                {
                    //error morate se logovati
                    poruka = "Morate se prijaviti";
                }

                return Json(new
                {
                    success = uspjeh,
                    message = poruka


                });
            }
            catch (Exception)
            {

                return null;
            }


        }
        public JsonResult SetOcjena(int clanakId, double rating)
        {
            try
            {
                dynamic mogucnost = MogucnostGlasanja(clanakId).Data;

                if (mogucnost.Success == false)
                {
                    return Json(mogucnost);
                }
                Korisnici kor = Session["Korisnik"] as Korisnici;
                using (Spajanje s = new Spajanje())
                {
                    OcjenaKorisnici oc = new OcjenaKorisnici();
                    oc.KorisnikID = kor.KorisnikID;
                    oc.ClanakID = clanakId;
                    oc.Ocjena = (rating / 2);
                    s.Context.OcjenaKorisnici.AddObject(oc);


                    Clanci clanakTemp = (from c in s.Context.Clanci where c.ClanakID == clanakId select c).FirstOrDefault();
                    if (clanakTemp.Ocjenjeno > 0)
                    {
                        int brojOcjena = (int)clanakTemp.Ocjenjeno;
                        double ProsOcjena = (double)clanakTemp.Popularnost;
                        double novaProsjecna = ((ProsOcjena * (double)brojOcjena) + (double)oc.Ocjena) / (brojOcjena + 1);

                        clanakTemp.Popularnost = novaProsjecna;
                        clanakTemp.Ocjenjeno++;

                    }
                    else
                    {
                        clanakTemp.Popularnost = (double)oc.Ocjena;
                        clanakTemp.Ocjenjeno++;
                    }
                    s.Context.SaveChanges();

                    return Json(new
                    {
                        Success = true,
                        Message = "Uspjesno ste glasali",
                        Result = new { Rating = (clanakTemp.Popularnost * 2), Raters = ("(od " + clanakTemp.Ocjenjeno + " korisnika)") },
                        color = ""

                    });
                }
                //PostModel post = Engine.Posts.SetRating(id, rating);

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NoviKomentar(string text, int clanakID)
        {
            try
            {

                using (Spajanje s = new Spajanje())
                {
                    Korisnici k = Session["Korisnik"] as Korisnici;
                    if (k == null)
                    {
                        return Json(new
                        {
                            Success = false,
                            poruka = "* Morate biti logovani"
                        });
                    }
                    else
                    {
                        if (text.Length > 11)
                        {

                            ClanciServisObrada.InsertComment(clanakID, k, text);
                            return Json(new
                            {
                                tekst = text,
                                datum = DateTime.Now.ToString(),
                                korisnik = k.UserName,
                                poruka = "Uspješno objavljen komentar",
                                Success = true
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                Success = false,
                                poruka = "* Morate unijeti minimalno 5 slova"
                            });

                        }

                    }

                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    Success = false,
                    poruka = "* Desila se pogreška"
                });

            }

        }

        public ActionResult Create()
        {
            Korisnici kor = Session["Korisnik"] as Korisnici;
            if (kor != null)
            {
                List<TagVrste> tgs;
                using (Spajanje s = new Spajanje())
                {


                    tgs = (from tg in s.Context.TagVrste orderby tg.Opis ascending select tg).ToList();
                    ViewBag.clanci = (from cl in s.Context.Clanci
                                      join sa in s.Context.Sadrzaji on
                                          cl.ClanakID equals sa.ClanakID
                                      select
                                          sa).ToList();

                }
                using (Wikiped.Models.Pitanja pt = new Wikiped.Models.Pitanja())
                {
                    ViewBag.Tagovi = pt.GetAllTagsCount().Take(15);
                }
                return View(tgs);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(string naslov, int kategorija, string novaKat, int slicnaKat, string tekst, HttpPostedFileBase slika, string tagovi, bool update, int clids)
        {
            if (update != true)
            {
                #region unos novog clanka
                using (Spajanje s = new Spajanje())
                {
                    Clanci novi = new Clanci();
                    novi.Popularnost = 0;
                    novi.Ocjenjeno = 0;

                    novi.Guid = Guid.NewGuid();
                    var fileName = novi.Guid.ToString().Substring(0, 9);
                    string putanja = unosSlike(slika, fileName, 0);
                    if (putanja != string.Empty)
                    {

                        novi.Slika = putanja;

                    }

                    novi.TagVrstaID = unosClanakvrste(kategorija, novaKat, slicnaKat);
                    Korisnici kor = Session["Korisnik"] as Korisnici;
                    novi.KorisnikID = kor.KorisnikID;

                    s.Context.Clanci.AddObject(novi);
                    s.Context.SaveChanges();

                    Clanci ClanakS = s.Context.Clanci.ToList().Last();

                    Sadrzaji sadrzajS = new Sadrzaji();
                    sadrzajS.ClanakID = ClanakS.ClanakID;
                    sadrzajS.Naslov = naslov;
                    sadrzajS.Tekst = tekst;
                    sadrzajS.Datum = DateTime.Now;
                    s.Context.Sadrzaji.AddObject(sadrzajS);
                    s.Context.SaveChanges();



                    TagoviUnos(tagovi, ClanakS.ClanakID);

                    return Redirect("ClanciPregled?clID=" + ClanakS.ClanakID.ToString());


                }

                #endregion

            }
            else
            {
                using (Spajanje s = new Spajanje())
                {
                    Clanci clanak = (from c in s.Context.Clanci where c.ClanakID == clids select c).FirstOrDefault();
                    Sadrzaji sadrzaj = (from c in s.Context.Sadrzaji where c.ClanakID == clids select c).FirstOrDefault();
                    if (clanak != null)
                    {
                        sadrzaj.Naslov = naslov;
                        sadrzaj.Tekst = tekst;
                        TagoviUnos(tagovi, clids);
                        var fileName = clanak.Guid.ToString().Substring(0, 9);
                        string putanja = unosSlike(slika, fileName, clids);

                        if (putanja != string.Empty)
                        {
                            if (putanja != null)
                            {
                                clanak.Slika = putanja;
                            }

                        }
                        if (slicnaKat > 0)
                        {
                            kategorija = 0;
                        }
                        clanak.TagVrstaID = unosClanakvrste(kategorija, novaKat, slicnaKat);
                        s.Context.SaveChanges();
                        return Redirect("ClanciPregled?clID=" + clids.ToString());

                    }
                }
            }
            LucenePt.ClearLuceneIndex();
            LucenePt.AddUpdateLuceneIndex(LucenePt.GetAllObjectForLuceneIndex());
            using (Wikiped.Models.Pitanja pt = new Wikiped.Models.Pitanja())
            {
                ViewBag.Tagovi = pt.GetAllTagsCount().Take(15);
            }
            return View();
        }
        public string unosSlike(HttpPostedFileBase slika, string naziv, int clid)
        {
            if (slika != null && slika.ContentLength > 0)
            {
                if (clid != 0)
                {
                    using (Spajanje s = new Spajanje())
                    {
                        Clanci clanak = (from c in s.Context.Clanci where c.ClanakID == clid select c).FirstOrDefault();
                        if (clanak.Slika != "")
                        {
                            System.IO.File.Delete(Server.MapPath(clanak.Slika));

                        }
                    }

                }
            }
            string putanja = "";
            if (slika != null && slika.ContentLength > 0)
            {

                var fileName = naziv;
                string ext = Path.GetExtension(slika.FileName);
                var path = Path.Combine(Server.MapPath("/Images/clanci/"), fileName + ext);
                slika.SaveAs(path);

                Image ss = System.Drawing.Image.FromFile(path);

                using (ss)
                {
                    ss = SlikeServices.ResizeSlikePravilno(ss, 120, 120);
                    ss.Save(Server.MapPath("/Images/clanci/" + fileName + "2" + ext));

                }

                System.IO.File.Delete(path);
                putanja = "/Images/clanci/" + fileName + "2" + ext;

            }
            if (putanja != string.Empty)
            {
                return putanja;
            }
            else
            {
                return "";

            }

        }
        public int unosClanakvrste(int kategorija, string novaKat, int slicnaKat)
        {

            using (Spajanje s = new Spajanje())
            {
                TagVrste tg2 = s.Context.TagVrste.ToList().Last();
                if (kategorija == 0)
                {
                    if (novaKat != string.Empty)
                    {
                        TagVrste tg = new TagVrste();
                        if (slicnaKat == 0)
                        {

                            tg.Vrsta = (tg2.Vrsta + 1);
                            tg.Opis = novaKat;
                            tg.Klasa = "PlavaS";

                        }
                        else
                        {
                            TagVrste tgtmp2 = (from tv in s.Context.TagVrste where tv.TagVrstaID == slicnaKat select tv).FirstOrDefault();

                            tg.Vrsta = (tg2.Vrsta + 1);
                            tg.Opis = novaKat;
                            tg.Klasa = tgtmp2.Klasa;
                        }

                        s.Context.TagVrste.AddObject(tg);
                        s.Context.SaveChanges();
                        tg = null;
                        tg = s.Context.TagVrste.ToList().Last();
                        return tg.TagVrstaID;



                    }
                    else
                    {
                        TagVrste tg = (from tv in s.Context.TagVrste where tv.Vrsta == 1 select tv).FirstOrDefault();
                        return tg.TagVrstaID;
                    }

                }
                else
                {
                    return kategorija;

                }
            }
        }
        public void TagoviUnos(string tagovi, int clid)
        {
            using (Spajanje s = new Spajanje())
            {
                List<TagClanci> tgs = (from tg in s.Context.TagClanci where tg.ClanakID == clid select tg).ToList();
                foreach (TagClanci tg in tgs)
                {
                    s.Context.TagClanci.DeleteObject(tg);
                    s.Context.SaveChanges();
                }

                List<string> tegs = tagovi.Split(',').ToList();
                if (tegs.Count > 0 && tegs[0] != string.Empty)
                {
                    List<Tags> Tagtemp = new List<Tags>();
                    Tags tgtmp;
                    foreach (string it in tegs)
                    {
                        tgtmp = new Tags();
                        tgtmp.Ime = it;
                        tgtmp.Opis = it;
                        Tagtemp.Add(tgtmp);

                    }

                    Tags tmpTag;
                    TagClanci tmpTclan;

                    foreach (Tags tgs2 in Tagtemp)
                    {
                        tmpTag = null;
                        tmpTag = (from tt in s.Context.Tags where tt.Ime == tgs2.Ime select tt).FirstOrDefault();

                        if (tmpTag == null)
                        {
                            s.Context.Tags.AddObject(tgs2);
                            s.Context.SaveChanges();
                            tmpTag = s.Context.Tags.ToList().Last();
                        }

                        tmpTclan = new TagClanci();
                        tmpTclan.ClanakID = clid;
                        tmpTclan.TagID = tmpTag.TagID;

                        s.Context.TagClanci.AddObject(tmpTclan);
                        s.Context.SaveChanges();

                    }

                }

            }
        }


        public ActionResult updateData(int zlID)
        {
            preporukaColaborative(1);
            try
            {

                return Json(ClanciTemp.getClanciById(zlID));
            }
            catch (Exception)
            {

                return null;
            }


        }
        public List<string> getWordForWiki(int clId)
        {
            using (Spajanje s = new Spajanje())
            {
                List<string> words = new List<string>();
                words.AddRange((from cl in s.Context.TagClanci
                                join t in s.Context.Tags
                                    on cl.TagID equals t.TagID
                                where cl.ClanakID == clId
                                select t.Ime).ToList());

                words.AddRange((from sd in s.Context.Sadrzaji where sd.ClanakID == clId select sd.Naslov).ToList());
                words = words.Distinct().ToList();
                return words;
            }
        }


        #region Preporuka item base
        public List<ClanakProsjek> preporukaItembase(int clId)
        {
            using (Spajanje s = new Spajanje())
            {

                List<int> tgs = (from cl in s.Context.TagClanci where cl.ClanakID == clId select (int)cl.TagID).ToList();
                List<ClanakTag> TagoviContain = BrojTagovaUCanku(tgs, clId);

                Clanci cls = (from c in s.Context.Clanci where c.ClanakID == clId select c).FirstOrDefault();
                TagVrste vrste = (from v in s.Context.TagVrste where v.TagVrstaID == cls.TagVrstaID select v).FirstOrDefault();

                List<ClanakProsjek> pros = ProsjecnaOcjenaPoKategoriji((double)cls.Popularnost, cls.ClanakID, vrste.Klasa);

                List<ClanakProsjek> Preporuka = SumiranjeProsjeka(pros, TagoviContain, (double)cls.Popularnost);

                List<ClanakProsjek> PreporukaFinal = BrojOcjena(Preporuka, (int)cls.Ocjenjeno);
                return PreporukaFinal;
            }

        }
        public List<ClanakProsjek> ProsjecnaOcjenaPoKategoriji(double prosjek, int clID, string vrsta)
        {

            using (Spajanje s = new Spajanje())
            {
                List<TagVrste> vrste = (from tv in s.Context.TagVrste where tv.Klasa == vrsta select tv).ToList();
                List<ClanakProsjek> ProsjekOcjenaFinal = new List<ClanakProsjek>();
                foreach (TagVrste tgv in vrste)
                {



                    List<ClanakProsjek> ProsjekOcjenaPovis = (from cc in s.Context.Clanci
                                                              where cc.Popularnost > prosjek
                                                              && cc.TagVrstaID == tgv.TagVrstaID
                                                              orderby cc.Popularnost ascending
                                                              select
                                                                  new ClanakProsjek { ClanakId = cc.ClanakID, Prosjek = (double)cc.Popularnost }).Take(5).ToList();

                    List<ClanakProsjek> ProsjekOcjenaIspod = (from cc in s.Context.Clanci
                                                              where cc.Popularnost <= prosjek
                                                              && cc.TagVrstaID == tgv.TagVrstaID
                                                              orderby cc.Popularnost descending
                                                              select
                                                                  new ClanakProsjek { ClanakId = cc.ClanakID, Prosjek = (double)cc.Popularnost }).Take(5).ToList();
                    ProsjekOcjenaFinal.AddRange(ProsjekOcjenaPovis);
                    ProsjekOcjenaFinal.AddRange(ProsjekOcjenaIspod);
                }


                float procentniDio = 23;
                float brojOcjena = 5;
                float OsnovnaJedinica = procentniDio / brojOcjena;


                for (int i = 0; i < ProsjekOcjenaFinal.Count; i++)
                {
                    if (ProsjekOcjenaFinal[i].Prosjek > prosjek)
                    {
                        ProsjekOcjenaFinal[i].Prosjek = ProsjekOcjenaFinal[i].Prosjek - prosjek;

                    }
                    else
                    {
                        ProsjekOcjenaFinal[i].Prosjek = prosjek - ProsjekOcjenaFinal[i].Prosjek;
                    }
                    ProsjekOcjenaFinal[i].Prosjek = (procentniDio - (ProsjekOcjenaFinal[i].Prosjek * OsnovnaJedinica)) + 22.0;
                }
                ProsjekOcjenaFinal = (from p in ProsjekOcjenaFinal orderby p.Prosjek descending where p.ClanakId != clID select p).ToList();

                return ProsjekOcjenaFinal;
            }
        }
        public List<ClanakProsjek> BrojOcjena(List<ClanakProsjek> Prosjeci, int brojOcjenaOrig)
        {
            try
            {


                using (Spajanje s = new Spajanje())
                {
                    Clanci temp;
                    double postotak = 22;
                    double razmak;
                    int ids;
                    for (int i = 0; i < Prosjeci.Count; i++)
                    {
                        //temp = null;
                        ids = Prosjeci[i].ClanakId;
                        temp = (from c in s.Context.Clanci where c.ClanakID == ids select c).FirstOrDefault();

                        if (temp.Ocjenjeno == brojOcjenaOrig)
                        {
                            Prosjeci[i].Prosjek = Prosjeci[i].Prosjek + postotak;
                        }
                        else
                        {
                            if (temp.Ocjenjeno != 0 && brojOcjenaOrig != 0)
                            {

                                if (temp.Ocjenjeno > brojOcjenaOrig)
                                {

                                    razmak = (double)(temp.Ocjenjeno - brojOcjenaOrig);
                                    Prosjeci[i].Prosjek = Prosjeci[i].Prosjek + (postotak - ((postotak / (double)temp.Ocjenjeno) * razmak));

                                }
                                else
                                {
                                    razmak = (double)(brojOcjenaOrig - temp.Ocjenjeno);
                                    Prosjeci[i].Prosjek = Prosjeci[i].Prosjek + (postotak - ((postotak / brojOcjenaOrig) * razmak));

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
        public List<ClanakTag> BrojTagovaUCanku(List<int> tgs, int clID)
        {
            List<ClanakTag> clTag;
            ClanakTag cltemp;
            List<ClanakTag> clTagFin = new List<ClanakTag>();

            using (Spajanje s = new Spajanje())
            {

                clTag = s.Context.TagClanci.Join(s.Context.Tags, tg => tg.TagID, t => t.TagID, (
                    tg, t) => new { TagClanci = tg, Tags = t }).Where(
                    q => tgs.Contains(q.Tags.TagID)).Select(x => new ClanakTag
                    {
                        ClanakId = (int)x.TagClanci.ClanakID,
                        ClanakCount = (int)x.TagClanci.ClanakID,
                        Prosjek = (double)0
                    }).ToList();
                foreach (ClanakTag it in clTag)
                {
                    cltemp = null;
                    cltemp = new ClanakTag();
                    cltemp.ClanakId = it.ClanakId;
                    cltemp.ClanakCount = clTag.Where(x => x.ClanakId == cltemp.ClanakId).Select(x => x.ClanakId).ToList().Count;
                    clTagFin.Add(cltemp);
                }
                clTag = clTagFin.Distinct(new ClanciPoredjenje()).ToList();
                clTag = (from cc in clTag orderby cc.ClanakCount descending where cc.ClanakId != clID select cc).ToList();
                float procentniDio = 33;
                float brojTagova = (float)tgs.Count;
                float OsnovnaJedinica = procentniDio / brojTagova;
                clTag = (from cc in clTag
                         orderby cc.Prosjek descending
                         select new ClanakTag
                         {
                             ClanakId = cc.ClanakId,
                             ClanakCount = cc.ClanakCount,
                             Prosjek = (cc.ClanakCount * OsnovnaJedinica)
                         }).ToList();


            }
            return clTag;

        }
        public double UporediProsjek(double origProsjek, int clanakId)
        {
            float procentniDio = 34;
            float brojOcjena = 5;
            float OsnovnaJedinica = procentniDio / brojOcjena;
            double Ostatak, prosjek;
            using (Spajanje s = new Spajanje())
            {
                Clanci cla = (from c in s.Context.Clanci where c.ClanakID == clanakId select c).FirstOrDefault();
                if (cla.Popularnost <= origProsjek)
                {
                    Ostatak = origProsjek - (double)cla.Popularnost;
                }
                else
                {
                    Ostatak = (double)cla.Popularnost - origProsjek;

                }
                prosjek = Ostatak * OsnovnaJedinica;

            }

            return prosjek;
        }
        public List<ClanakProsjek> SumiranjeProsjeka(List<ClanakProsjek> pros, List<ClanakTag> TagoviContain, double origprosjek)
        {
            ClanakProsjek cltemp;
            ClanakProsjek cltemp2;
            List<ClanakProsjek> final = new List<ClanakProsjek>();
            using (Spajanje s = new Spajanje())
            {
                for (int i = 0; i < TagoviContain.Count; i++)
                {
                    cltemp = null;
                    cltemp = (from p in pros where p.ClanakId == TagoviContain[i].ClanakId select p).FirstOrDefault();
                    if (cltemp != null)
                    {
                        cltemp2 = new ClanakProsjek();
                        cltemp2.ClanakId = cltemp.ClanakId;
                        cltemp2.Prosjek = cltemp.Prosjek + TagoviContain[i].Prosjek;
                        pros.Remove(cltemp);
                        pros.Add(cltemp2);

                    }
                    else
                    {
                        cltemp2 = new ClanakProsjek();
                        cltemp2.ClanakId = TagoviContain[i].ClanakId;
                        cltemp2.Prosjek = cltemp2.Prosjek + UporediProsjek(origprosjek, TagoviContain[i].ClanakId);
                        pros.Add(cltemp2);
                    }
                }
                pros = (from p in pros orderby p.Prosjek descending select p).Take(5).ToList();
                return pros;
            }

        }
        #endregion


        #region Preporuka colaborative
        public List<ClanciContains> preporukaColaborative(int userId)
        {
            List<ClanciContains> PreporukaFinal;

            List<ClanciGlasovi> MyClanci = getAllMyClanciByVote(userId);

            List<int> UsersIds = new List<int>();
            UsersIds.Add(userId);

            if (getAllMyBrojClanaka(userId) > 0)
            {

                List<ClanciGlasovi> OtherClanci = getAllOtherClanciByVote(MyClanci, userId);
                double brojPitanja = (double)MyClanci.Count;

                List<ClanciContains> otherContains = getAllClanciByContains(OtherClanci, brojPitanja);

                PreporukaFinal = otherContains.Take(5).ToList();

                if (PreporukaFinal.Count < 5)
                {
                    int top = 5 - PreporukaFinal.Count;
                    UsersIds.AddRange((from p in PreporukaFinal select p.KorisnikID).ToList());

                    PreporukaFinal.AddRange(getTop5ClanakaDef(top, UsersIds));

                }

            }
            else
            {
                PreporukaFinal = getTop5ClanakaDef(5, UsersIds);

            }
            PreporukaFinal = (from p in PreporukaFinal orderby p.Contains descending select p).ToList();
            return PreporukaFinal;


        }
        public List<ClanciGlasovi> getAllMyClanciByVote(int userId)
        {
            using (Spajanje s = new Spajanje())
            {

                List<ClanciGlasovi> glasPitanje = (from gp in s.Context.OcjenaKorisnici
                                                    where gp.KorisnikID == userId
                                                   select new ClanciGlasovi
                                                    {
                                                        KorisnikID = (int)gp.KorisnikID,
                                                        ClanakId = (int)gp.ClanakID,
                                                        Ocjena = (int)gp.Ocjena
                                                    }).ToList();

                return glasPitanje;

            }

        }
        public double getAllMyBrojClanaka(int userId)
        {
            using (Spajanje s = new Spajanje())
            {

                double brojpitanja = (from gp in s.Context.OcjenaKorisnici
                                      where gp.KorisnikID == userId
                                      select gp.ClanakID).ToList().Count;

                return brojpitanja;

            }

        }
        public List<ClanciGlasovi> getAllOtherClanciByVote(List<ClanciGlasovi> myClanci, int userId)
        {
            using (Spajanje s = new Spajanje())
            {
                List<ClanciGlasovi> ocjenaClanakUsers = new List<ClanciGlasovi>();

                foreach (ClanciGlasovi pt in myClanci)
                {
                    ocjenaClanakUsers.AddRange((from gp in s.Context.OcjenaKorisnici
                                               where (gp.ClanakID == pt.ClanakId && gp.Ocjena == pt.Ocjena && gp.KorisnikID != userId)
                                               select new ClanciGlasovi
                                               {
                                                   KorisnikID = (int)gp.KorisnikID,
                                                   ClanakId = (int)gp.ClanakID,
                                                   Ocjena = (int)gp.Ocjena
                                               }).ToList());
                }
                return ocjenaClanakUsers;
            }

        }
        public List<ClanciContains> getAllClanciByContains(List<ClanciGlasovi> otherClanci, double brojClanaka)
        {
            List<int> idUsers = otherClanci.Select(x => (int)x.KorisnikID).Distinct().ToList();

            ClanciContains tempP;
            List<ClanciContains> KorisniciContains = new List<ClanciContains>();
            List<ClanciContains> KorisniciFinal = new List<ClanciContains>();
            for (int i = 0; i < idUsers.Count; i++)
            {
                tempP = null;
                tempP = new ClanciContains();
                tempP.Contains = (from pt in otherClanci where pt.KorisnikID == idUsers[i] select pt.KorisnikID).ToList().Count();
                tempP.KorisnikID = idUsers[i];
                KorisniciContains.Add(tempP);
            }
            KorisniciContains = (from kc in KorisniciContains orderby kc.Contains descending select kc).ToList();
            double postotak;
            for (int i = 0; i < KorisniciContains.Count; i++)
            {
                if (getAllMyBrojClanaka(KorisniciContains[i].KorisnikID) > brojClanaka)
                {
                    postotak = 100 / getAllMyBrojClanaka(KorisniciContains[i].KorisnikID);
                    KorisniciContains[i].Contains = KorisniciContains[i].Contains * postotak;

                }
                else
                {
                    postotak = 100 / brojClanaka;
                    KorisniciContains[i].Contains = KorisniciContains[i].Contains * postotak;

                }
            }
            KorisniciFinal = (from kr in KorisniciContains orderby kr.Contains descending select kr).ToList();
            return KorisniciFinal;
        }
        public List<ClanciContains> getTop5ClanakaDef(int top, List<int> UsesIds)
        {
            using (Spajanje s = new Spajanje())
            {
                try
                {

                List<int> userIdsN = (from p in s.Context.Clanci orderby p.Ocjenjeno, p.Popularnost descending select (int)p.KorisnikID).Distinct().Take(15).ToList();

                List<int> userIdsF = userIdsN.Where(x => !UsesIds.Contains(x)).ToList();
               
                List<ClanciContains> preporukaFinal = s.Context.Korisnici.Where(x => userIdsF.Contains(x.KorisnikID)).Select(x => new ClanciContains
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
        #endregion

        public void brisanjeClanka(int zlID)
        {

            using (Spajanje s = new Spajanje())
            {
                Clanci cls = (from c in s.Context.Clanci where c.ClanakID == zlID select c).FirstOrDefault();
                if (cls.Slika != "")
                {
                    System.IO.File.Delete(Server.MapPath(cls.Slika));

                }
            }
            ClanciServisObrada.brisanjeById(zlID);


        }
        public List<Korisnici> getKorisniciByIds(List<ClanciContains> userIds)
        {
            Korisnici koT;
                List<Korisnici> kor = new List<Korisnici>();
            using (Spajanje s = new Spajanje())
            {
                
                foreach (var k in userIds)
                {
                    koT = null;
                    koT = (from ks in s.Context.Korisnici where ks.KorisnikID == k.KorisnikID select ks).FirstOrDefault();
                    kor.Add(koT);
                }
               
            }
                return kor;
        }



    }
    public class ClanciGlasovi
    {
        public int KorisnikID { get; set; }
        public int ClanakId { get; set; }
        public int Ocjena { get; set; }
    }
    public class ClanciContains
    {
        public int KorisnikID { get; set; }
        public double Contains { get; set; }
    }
}
