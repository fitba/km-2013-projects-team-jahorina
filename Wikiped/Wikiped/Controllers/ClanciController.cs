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
            return View(c.getAllClanci());
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
            using (Spajanje s = new Spajanje())
            {

               
                List<TagVrste> tgs = (from tg in s.Context.TagVrste where tg.Vrsta!=1 orderby tg.Opis ascending select tg).ToList();
               ViewBag.clanci=(from cl in s.Context.Clanci join sa in s.Context.Sadrzaji on 
                               cl.ClanakID equals sa.ClanakID select
                   sa).ToList();

                return View(tgs);
            }
            
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(string naslov, int kategorija,string novaKat, int slicnaKat, string tekst, HttpPostedFileBase slika, string tagovi, bool update)
        {
            if (update != true)
            {
                
                    using (Spajanje s = new Spajanje())
                    {
                        Clanci novi = new Clanci();
                        novi.Popularnost = 0;
                        novi.Ocjenjeno = 0;
                        
                        novi.Guid = Guid.NewGuid();
                        string putanja = "";
                        if (slika != null && slika.ContentLength > 0)
                        {

                            var fileName = novi.Guid.ToString().Substring(0, 9);
                            string ext = Path.GetExtension(slika.FileName);
                            var path = Path.Combine(Server.MapPath("~/Images/clanci/"), fileName + ext);
                            slika.SaveAs(path);

                            Image ss = System.Drawing.Image.FromFile(path);

                            using (ss)
                            {
                                ss = SlikeServices.ResizeSlikePravilno(ss, 120, 120);
                                ss.Save(Server.MapPath("~/Images/clanci/" + fileName + "2" + ext));

                            }

                            System.IO.File.Delete(path);
                            putanja = "~/Images/clanci/" + fileName + "2" + ext;

                        }
                        if (putanja != string.Empty)
                        {
                            novi.Slika = putanja;
                        }
                        else
                        {
                            novi.Slika = null;

                        }

                        if (kategorija == 0)
                        {
                            if (novaKat != string.Empty)
                            {
                                TagVrste tg = new TagVrste();
                                if (slicnaKat == 0)
                                {
                                    tg.Vrsta = 1;
                                    tg.Opis = novaKat;

                                }
                                else
                                {
                                    TagVrste tgtmp2 = (from tv in s.Context.TagVrste where tv.TagVrstaID == slicnaKat select tv).FirstOrDefault();
                                    tg.Vrsta = tgtmp2.Vrsta;
                                    tg.Opis = novaKat;


                                }
                                s.Context.TagVrste.AddObject(tg);
                                s.Context.SaveChanges();
                                tg = null;
                                tg = s.Context.TagVrste.ToList().Last();
                                novi.TagVrstaID = tg.TagVrstaID;




                            }
                            else
                            {
                                TagVrste tg = (from tv in s.Context.TagVrste where tv.Vrsta == 1 select tv).FirstOrDefault();
                                novi.TagVrstaID = tg.TagVrstaID;

                            }

                        }
                        else
                        {
                            novi.TagVrstaID = kategorija;

                        }
                        
                        s.Context.Clanci.AddObject(novi);
                        s.Context.SaveChanges();

                        Clanci ClanakS =s.Context.Clanci.ToList().Last();

                        Sadrzaji sadrzajS = new Sadrzaji();
                        sadrzajS.ClanakID = ClanakS.ClanakID;
                        sadrzajS.Naslov = naslov;
                        sadrzajS.Tekst = tekst;
                        sadrzajS.Datum = DateTime.Now;
                        s.Context.Sadrzaji.AddObject(sadrzajS);
                        s.Context.SaveChanges();

                        List<string> tegs = tagovi.Split(',').ToList();
                        if (tegs.Count > 0 && tegs[0]!=string.Empty)
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
                            foreach (Tags tgs in Tagtemp)
                            {
                                s.Context.Tags.AddObject(tgs);
                                s.Context.SaveChanges();

                                tmpTag = s.Context.Tags.ToList().Last();
                                tmpTclan = new TagClanci();
                                tmpTclan.ClanakID = ClanakS.ClanakID;
                                tmpTclan.TagID = tmpTag.TagID;
                                s.Context.TagClanci.AddObject(tmpTclan);
                                s.Context.SaveChanges();

                            }
                        }
                        
                        

                    }
                    

                
            }
            else
            {

            }
            return View();
        }
    
    
    
    
    
    }
}
