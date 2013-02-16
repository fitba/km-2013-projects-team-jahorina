using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wikiped.DBBL.BLL;
using Wikiped.DBBL.DAL;
using System.Web.Mvc;

namespace Wikiped.Models
{
    public class KomentariServis
    {
        public string tekst { get; set; }
        public string Korisnik { get; set; }
        public DateTime datum { get; set; }


    }
    public class ClanciServis
    {



        public int ClanakId { get; set; }
        public string slika { get; set; }
        public double popularnost { get; set; }
        public int ocjena { get; set; }
        public int vrsta { get; set; }
        public string naslov { get; set; }
        public string opis { get; set; }
        public string tekst { get; set; }
        public string klasa { get; set; }
        public int komentari { get; set; }
        public List<KomentariServis> komentariLista { get; set; }


    }
    public class ClanciServisObrada
    {
        public string klasa { get; set; }
        public List<ClanciServis> ClanciOrig { get; set; }

        public List<ClanciServisObrada> getAllClanci()
        {
            using (Spajanje s = new Spajanje())
            {


                List<ClanciServis> tst = (from c in s.Context.Clanci
                                          join sa in s.Context.Sadrzaji
                                          on c.ClanakID equals sa.ClanakID
                                          join ts in s.Context.TagVrste
                                          on c.TagVrstaID equals ts.TagVrstaID
                                          select new ClanciServis
                                          {
                                              ClanakId = c.ClanakID,
                                              slika = c.Slika,
                                              popularnost = (double)c.Popularnost,
                                              ocjena = (int)c.Ocjenjeno,
                                              naslov = sa.Naslov,
                                              opis = sa.Opis,
                                              tekst = sa.Tekst,
                                              klasa = ts.Opis,
                                              vrsta = (int)ts.Vrsta,
                                              komentari = (from k in s.Context.Komentari where k.ClanakID == c.ClanakID select k).Count()


                                          }).ToList();


                List<ClanciServisObrada> finals = new List<ClanciServisObrada>();
                ClanciServisObrada tempF;
                List<TagVrste> SveVrste = GetAllVrste();
                foreach (TagVrste it in SveVrste)
                {


                    try
                    {
                        tempF = null;
                        tempF = new ClanciServisObrada();
                        tempF.ClanciOrig = (from lis in tst where lis.vrsta == it.Vrsta select lis).ToList();
                        tempF.klasa = (from lis in tempF.ClanciOrig select lis.klasa).FirstOrDefault().ToString();
                        finals.Add(tempF);


                    }
                    catch (Exception)
                    {


                    }

                }
                List<Tags> tagovi;
                //tagovi=(from tc in s.Context.TagClanci join clan in s.Context.Clanci on tc.ClanakID equals clan.ClanakID
                //join t in s.Context.Tags on tc.TagID equals t.TagID select t).ToList()
                foreach (ClanciServisObrada repl in finals)
                {
                    foreach (ClanciServis cl in repl.ClanciOrig)
                    {
                        cl.tekst = cl.tekst.Substring(0, (cl.tekst.Length * 20) / 100);
                        cl.tekst = cl.tekst.Substring(0, cl.tekst.LastIndexOf(' '));
                        cl.tekst += "...";
                        tagovi = null;
                        tagovi = (from tc in s.Context.TagClanci
                                  join clan in s.Context.Clanci on tc.ClanakID equals clan.ClanakID
                                  join t in s.Context.Tags on tc.TagID equals t.TagID
                                  where tc.ClanakID == cl.ClanakId
                                  select t).ToList();
                        foreach (Tags tg in tagovi)
                        {
                            cl.tekst = MvcHtmlString.Create(cl.tekst.Replace(tg.Ime, "<div class='Clanci-Tag'>" + tg.Ime + "</div>")).ToString();

                        }
                    }
                }



                return finals;

            }
        }
        public List<TagVrste> GetAllVrste()
        {
            using (Spajanje s = new Spajanje())
            {
                return s.Context.TagVrste.ToList();

            }

        }
        public string getString(Guid g)
        {
            return g.ToString();

        }
        public static void InsertComment(int clanakID, Korisnici k, string tekst)
        {
            try
            {

                using (Spajanje s = new Spajanje())
                {
                    Komentari kom = new Komentari();
                    kom.ClanakID = clanakID;
                    kom.KorisnikID = k.KorisnikID;
                    kom.Tekst = tekst;
                    kom.Datum = DateTime.Now;
                    s.Context.Komentari.AddObject(kom);
                    s.Context.SaveChanges();

                }

            }
            catch (Exception)
            {


            }

        }
        public static ClanciServis getClanakById(int id)
        {

            using (Spajanje s = new Spajanje())
            {

                ClanciServis tst = (from c in s.Context.Clanci
                                    join sa in s.Context.Sadrzaji

                                    on c.ClanakID equals sa.ClanakID
                                    where c.ClanakID == id
                                    select new ClanciServis
                                    {
                                        ClanakId = c.ClanakID,
                                        slika = c.Slika,
                                        popularnost = (double)c.Popularnost,
                                        ocjena = (int)c.Ocjenjeno,
                                        naslov = sa.Naslov,
                                        opis = sa.Opis,
                                        tekst = sa.Tekst,

                                    }).FirstOrDefault();

                List<KomentariServis> koms= (from kom in s.Context.Komentari
                                             join kr in s.Context.Korisnici
                                             on kom.KorisnikID equals kr.KorisnikID
                                             where kom.ClanakID == id
                                             orderby kom.Datum
                                             select new KomentariServis { tekst = kom.Tekst, Korisnik = kr.UserName,datum=(DateTime)kom.Datum }).ToList();
                tst.komentariLista = koms;
                return tst;

            }
        }


    }
}