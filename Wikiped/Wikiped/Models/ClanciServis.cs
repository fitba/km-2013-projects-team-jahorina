using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wikiped.DBBL.BLL;
using Wikiped.DBBL.DAL;
using System.Web.Mvc;

namespace Wikiped.Models
{
    public class ClanciPoredjenje : IEqualityComparer<ClanakTag>
    {
        #region IEqualityComparer<Post> Members

        public bool Equals(ClanakTag x, ClanakTag y)
        {
            return x.ClanakId.Equals(y.ClanakId);
        }

        public int GetHashCode(ClanakTag obj)
        {
            return obj.ClanakId.GetHashCode();
        }

        #endregion
    }
    public class ClanakProsjek
    {
        public int ClanakId { get; set; }
        public double Prosjek { get; set; }
    }
    public class ClanakTag
    {
        public int ClanakId { get; set; }
        public int ClanakCount { get; set; }
        public double Prosjek { get; set; }
    }
    public class KomentariServis
    {
        public int komentarID { get; set; }
        public string tekst { get; set; }
        public string Korisnik { get; set; }
        public DateTime datum { get; set; }
        public int zloupotreba { get; set; }


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
        public List<Tags> tagovi { get; set; }
        public DateTime? datum { get; set; }
        public string korisnik { get; set; }
    }
    public class ClanciServisObrada
    {
        public string klasa { get; set; }
        public List<ClanciServis> ClanciOrig { get; set; }

        public static List<ClanciServis> getClanciByIds(List<int> clanciIds)
        {
            List<ClanciServis> clanciPoids = new List<ClanciServis>();

            ClanciServis temp;
            foreach (int i in clanciIds)
            {
                temp = null;
                temp = getClanakById(i);


                temp.tekst = temp.tekst.Substring(0, (temp.tekst.Length * 20) / 100);
                try
                {
                    temp.tekst = temp.tekst.Substring(0, temp.tekst.LastIndexOf(' '));
                }
                catch (Exception)
                {


                }

                temp.tekst += "...";
                clanciPoids.Add(temp);
            }
            return clanciPoids;

        }
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
                                              klasa = ts.Klasa,
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
                        try
                        {
                            cl.tekst = cl.tekst.Substring(0, cl.tekst.LastIndexOf(' '));
                        }
                        catch (Exception)
                        {


                        }

                        cl.tekst += "...";
                        tagovi = null;
                        tagovi = (from tc in s.Context.TagClanci
                                  join clan in s.Context.Clanci on tc.ClanakID equals clan.ClanakID
                                  join t in s.Context.Tags on tc.TagID equals t.TagID
                                  where tc.ClanakID == cl.ClanakId
                                  select t).ToList();
                        foreach (Tags tg in tagovi)
                        {
                            cl.tekst = MvcHtmlString.Create(cl.tekst.Replace(tg.Ime, "<a href='/Tag/Details/?name=" +tg.Ime + "' class='Clanci-Tag' title='' rel='tag'>" + tg.Ime + "</a>")).ToString();

                        }
                    }
                }



                return finals.Take(30).ToList();

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
                                        datum = sa.Datum



                                    }).FirstOrDefault();
                tst.tagovi = (from tg in s.Context.TagClanci
                              join ts in s.Context.Tags
                                               on tg.TagID equals ts.TagID
                              where tg.ClanakID == tst.ClanakId
                              select ts).ToList();
                tst.korisnik = (from cl in s.Context.Clanci
                                join ko in s.Context.Korisnici on
                                    cl.KorisnikID equals ko.KorisnikID
                                where cl.ClanakID == cl.ClanakID
                                select ko.UserName).FirstOrDefault();
                List<KomentariServis> koms = (from kom in s.Context.Komentari
                                              join kr in s.Context.Korisnici
                                              on kom.KorisnikID equals kr.KorisnikID
                                              where kom.ClanakID == id
                                              orderby kom.Datum
                                              select new KomentariServis { komentarID = kom.KomentarID, tekst = kom.Tekst, Korisnik = kr.UserName, datum = (DateTime)kom.Datum, zloupotreba = (from zl in s.Context.Zloupotrebe where zl.KomentarID == kom.KomentarID select zl).Count() }).ToList();
                tst.komentariLista = koms;


                foreach (Tags tg in tst.tagovi)
                {
                    tst.tekst = MvcHtmlString.Create(tst.tekst.Replace(tg.Ime, "<a href='/Tag/Details/?name=" + tg.Ime + "' class='Clanci-Tag' title='' rel='tag'>" + tg.Ime + "</a>")).ToString();

                }
                return tst;

            }
        }

        public static void brisanjeById(int id)
        {
            using (Spajanje s = new Spajanje())
            {
                Clanci clanak = (from cc in s.Context.Clanci where cc.ClanakID == id select cc).FirstOrDefault();
                List<OcjenaKorisnici> ocjene = (from o in s.Context.OcjenaKorisnici where o.ClanakID == id select o).ToList();
                List<Komentari> komentari = (from k in s.Context.Komentari where k.ClanakID == id select k).ToList();
                List<Sadrzaji> sadrzaji = (from o in s.Context.Sadrzaji where o.ClanakID == id select o).ToList();
                List<TagClanci> tagovi = (from o in s.Context.TagClanci where o.ClanakID == id select o).ToList();
                List<Zloupotrebe> zloupotrebe = new List<Zloupotrebe>();
                List<Zloupotrebe> zloupotrebeFin = new List<Zloupotrebe>();
                foreach (Komentari ko in komentari)
                {

                    zloupotrebe = (from kom in s.Context.Zloupotrebe where kom.KomentarID == ko.KomentarID select kom).ToList();
                    zloupotrebeFin.AddRange(zloupotrebe);
                }
                foreach (OcjenaKorisnici oc in ocjene)
                {
                    s.Context.OcjenaKorisnici.DeleteObject(oc);
                }
                foreach (Zloupotrebe zl in zloupotrebeFin)
                {
                    s.Context.Zloupotrebe.DeleteObject(zl);
                }
                foreach (Komentari ko in komentari)
                {
                    s.Context.Komentari.DeleteObject(ko);
                }
                foreach (TagClanci tg in tagovi)
                {
                    s.Context.TagClanci.DeleteObject(tg);
                }
                foreach (Sadrzaji sads in sadrzaji)
                {
                    s.Context.Sadrzaji.DeleteObject(sads);
                }
                s.Context.Clanci.DeleteObject(clanak);

                s.Context.SaveChanges();


            }
        }
    }
    public class ClanciTemp
    {
        public string Naslov { get; set; }
        public int Kategorija { get; set; }
        public string tekst { get; set; }
        public List<string> tagovi { get; set; }

        public static ClanciTemp getClanciById(int zlID)
        {
            using (Spajanje s = new Spajanje())
            {
                ClanciTemp clanci = (from cl in s.Context.Clanci
                                     join sa in s.Context.Sadrzaji
                                         on cl.ClanakID equals sa.ClanakID
                                     join vr in s.Context.TagVrste
                                     on cl.TagVrstaID equals vr.TagVrstaID
                                     where cl.ClanakID == zlID
                                     select new ClanciTemp
                                     {
                                         Naslov = sa.Naslov,
                                         Kategorija = (int)vr.TagVrstaID,
                                         tekst = sa.Tekst

                                     }).FirstOrDefault();

                clanci.tagovi = (from t in s.Context.TagClanci
                                 join tg in s.Context.Tags
                                     on t.TagID equals tg.TagID
                                 where t.ClanakID == zlID
                                 select tg.Ime
                                 ).ToList();
                return clanci;

            }

        }

    }
}