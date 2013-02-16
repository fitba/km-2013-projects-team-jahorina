﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.Models;
using Wikiped.DBBL.DAL;
using Wikiped.DBBL.BLL;
using System.Web.Script.Serialization;

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
                        Result = new { Rating = (clanakTemp.Popularnost * 2), Raters =("(od " + clanakTemp.Ocjenjeno + " korisnika)")  },
                        color=""

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

    }
}
