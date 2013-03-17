using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wikiped.DBBL.DAL;

namespace Wikiped.Models
{
    public class TagCount
    {
        public string Name { get; set; }
        public int CountT { get; set; }
    }
    public class Pitanja : IDisposable
    {

        #region Property

        private Wikiped.DBBL.DAL.Pitanja pitanjaPost;
        private Wikiped.DBBL.DAL.Korisnici korisnik;
        private List<OdgovoriSaO> odgovori;
        private List<Wikiped.DBBL.DAL.Tags> tagovi;

        public Wikiped.DBBL.DAL.Korisnici Korisnik
        {
            get { return korisnik; }
            set { korisnik = value; }
        }
        public List<OdgovoriSaO> Odgovori
        {
            get { return odgovori; }
            set { odgovori = value; }
        }
        public Wikiped.DBBL.DAL.Pitanja PitanjaPost
        {
            get { return pitanjaPost; }
            set { pitanjaPost = value; }
        }
        public List<Wikiped.DBBL.DAL.Tags> Tagovi
        {
            get { return tagovi; }
            set { tagovi = value; }
        }

        #endregion
        WikipedEntities context;
        public Pitanja()
        {
            context = new WikipedEntities();
        }
        public List<DBBL.DAL.Pitanja> GetAllPitanja()
        {
            return context.Pitanja.ToList();
        }
        public DBBL.DAL.Pitanja GetPitanjeByID(int id)
        {
            return context.Pitanja.Where(x => x.PitanjeID == id).FirstOrDefault();
        }

        public void SetAllDetaByPitanjeID(int id)
        {
            pitanjaPost = context.Pitanja.Where(x => x.PitanjeID == id).FirstOrDefault();
            korisnik = context.Korisnici.Where(x => x.KorisnikID == pitanjaPost.KorisnikID).FirstOrDefault();
            List<DBBL.DAL.Odgovori> lstOdgovori = context.Odgovori.Where(x => x.PitanjeID == id).ToList();
            odgovori = new List<OdgovoriSaO>();
            DBBL.DAL.Pitanja tempP= GetPitanjeByID(id);
            tempP.BrojPregleda++;
            context.SaveChanges();
            foreach (var item in lstOdgovori)
            {
                OdgovoriSaO odgovoriAll = new OdgovoriSaO();
                odgovoriAll.OdgovoriGlavni = item;
                odgovoriAll.Korisnik = context.Korisnici.Where(x => x.KorisnikID == item.KorisnikID).FirstOrDefault();

                COdgovorNaOdgovor tempOdgovorO;
                List<DBBL.DAL.OdgovorNaOdgovor> lstOdgovoriOdg = context.OdgovorNaOdgovor.Where(x => x.OdgovorID == item.OdgovorID).ToList();
                foreach (var odg in lstOdgovoriOdg)
                {
                    tempOdgovorO = new COdgovorNaOdgovor();
                    tempOdgovorO.OdgovoriNaPodOgovor = odg;
                    tempOdgovorO.Korisnik = context.Korisnici.Where(x => x.KorisnikID == odg.KorisnikID).FirstOrDefault();
                    odgovoriAll.OdgovoriNaOdgovor.Add(tempOdgovorO);
                }
                odgovori.Add(odgovoriAll);
            }
        }

        public int OdgovorVoteUp(int odgovorId, int korisnikId)
        {
            GlasoviZaOdgovore temp = context.GlasoviZaOdgovore.Where(x => x.OdgovorID == odgovorId && x.KorisnikID == korisnikId).FirstOrDefault();
            if (temp == null)
            {
                temp = new GlasoviZaOdgovore();
                temp.KorisnikID = korisnikId;
                temp.OdgovorID = odgovorId;
                temp.Glas = 1;
                context.GlasoviZaOdgovore.AddObject(temp);
                context.SaveChanges();
            }
            else
            {
                if (temp.Glas < 1)
                {
                    temp.Glas++;
                    context.SaveChanges();
                }
            }
            DBBL.DAL.Odgovori odg = context.Odgovori.Where(x => x.OdgovorID == odgovorId).FirstOrDefault();
            odg.BrojGlasova = context.GlasoviZaOdgovore.Where(x => x.KorisnikID == korisnikId && x.OdgovorID == odgovorId).Sum(x => x.Glas).Value;
            context.SaveChanges();
            return odg.BrojGlasova.Value;
        }
        public int OdgovorVoteDown(int odgovorId, int korisnikId)
        {
            GlasoviZaOdgovore temp = context.GlasoviZaOdgovore.Where(x => x.OdgovorID == odgovorId && x.KorisnikID == korisnikId).FirstOrDefault();
            if (temp == null)
            {
                temp = new GlasoviZaOdgovore();
                temp.KorisnikID = korisnikId;
                temp.OdgovorID = odgovorId;
                temp.Glas = -1;
                context.GlasoviZaOdgovore.AddObject(temp);
                context.SaveChanges();
            }
            else
            {
                if (temp.Glas >= 0)
                {
                    temp.Glas--;
                    context.SaveChanges();
                }
            }
            DBBL.DAL.Odgovori odg = context.Odgovori.Where(x => x.OdgovorID == odgovorId).FirstOrDefault();
            odg.BrojGlasova = context.GlasoviZaOdgovore.Where(x => x.KorisnikID == korisnikId && x.OdgovorID == odgovorId).Sum(x => x.Glas).Value;
            context.SaveChanges();
            return odg.BrojGlasova.Value;
        }

        public int PitanjeVoteUp(int pitanjeID, int korisnikId)
        {
            GlasoviZaPitanja temp = context.GlasoviZaPitanja.Where(x => x.PitanjeID == pitanjeID && x.KorisnikID == korisnikId).FirstOrDefault();
            if (temp == null)
            {
                temp = new GlasoviZaPitanja();
                temp.KorisnikID = korisnikId;
                temp.PitanjeID = pitanjeID;
                temp.Glas = 1;
                context.GlasoviZaPitanja.AddObject(temp);
                context.SaveChanges();
            }
            else
            {
                if (temp.Glas < 1)
                {
                    temp.Glas++;
                    context.SaveChanges();
                }
            }
            DBBL.DAL.Pitanja odg = context.Pitanja.Where(x => x.PitanjeID == pitanjeID).FirstOrDefault();
            odg.BrojGlasova = context.GlasoviZaPitanja.Where(x => x.KorisnikID == korisnikId && x.PitanjeID == pitanjeID).Sum(x => x.Glas).Value;
            context.SaveChanges();
            return odg.BrojGlasova.Value;
        }

        public int PitanjeVoteDown(int pitanjeID, int korisnikId)
        {
            GlasoviZaPitanja temp = context.GlasoviZaPitanja.Where(x => x.PitanjeID == pitanjeID && x.KorisnikID == korisnikId).FirstOrDefault();
            if (temp == null)
            {
                temp = new GlasoviZaPitanja();
                temp.KorisnikID = korisnikId;
                temp.PitanjeID = pitanjeID;
                temp.Glas = -1;
                context.GlasoviZaPitanja.AddObject(temp);
                context.SaveChanges();
            }
            else
            {
                if (temp.Glas >= 0)
                {
                    temp.Glas--;
                    context.SaveChanges();
                }
            }
            DBBL.DAL.Pitanja odg = context.Pitanja.Where(x => x.PitanjeID == pitanjeID).FirstOrDefault();
            odg.BrojGlasova = context.GlasoviZaPitanja.Where(x => x.KorisnikID == korisnikId && x.PitanjeID == pitanjeID).Sum(x => x.Glas).Value;
            context.SaveChanges();
            return odg.BrojGlasova.Value;
        }

        public void AddCommentOnAnswer(OdgovorNaOdgovor temp)
        {
            context.OdgovorNaOdgovor.AddObject(temp);
            context.SaveChanges();
        }
        public void AddAnswer(Odgovori odg)
        {
            context.Odgovori.AddObject(odg);
            context.SaveChanges();
        }
        public List<Tags> GetAllTags()
        {
            return context.Tags.ToList();
        }
        public Tags GetTagByID(int id)
        {
            return context.Tags.Where(x => x.TagID == id).FirstOrDefault();
        }
        public int AddPitanja(DBBL.DAL.Pitanja pt)
        {
            context.Pitanja.AddObject(pt);
            context.SaveChanges();
            DBBL.DAL.Pitanja temp = context.Pitanja.ToList().LastOrDefault();
            if (temp != null)
            {
                return temp.PitanjeID;
            }
            return 0;
        }
        public List<TagVrste> GetAllKategorije()
        {
            return context.TagVrste.ToList();
        }
        public void AddTagsForPitanja(TagoviPitanja tg)
        {
            context.TagoviPitanja.AddObject(tg);
            context.SaveChanges();
        }
        public int GetTagIdByName(string name)
        {
            Tags tag = context.Tags.ToList().Where(x => x.Ime == name).FirstOrDefault();
            if (tag != null)
            {
                return tag.TagID;
            }
            else
            {
                Tags t = new Tags();
                t.Ime = name;
                context.Tags.AddObject(t);
                context.SaveChanges();
                Tags tagZ = context.Tags.LastOrDefault();
                if (tagZ != null)
                {
                    return tagZ.TagID;
                }
                return 0;
            }
        }
        public IEnumerable<string> GetAllTagsForPitanjeID(int id)
        {
            var upit = (from t in context.Tags
                        join pt in context.TagoviPitanja
                        on t.TagID equals pt.TagID
                        where pt.PitanjeID == id
                        select t.Ime).Distinct().ToList();
            return upit;
        }
        public int GetCountTagByTagoviPitanja(int id)
        {
            return context.TagoviPitanja.Where(x => x.TagID == id).Count();
        }
        public int GetCountTagByTagoviClanci(int id)
        {
            return context.TagClanci.Where(x => x.TagID == id).Count();
        }
        public List<TagCount> GetAllTagsCount()
        {
            List<TagCount> tag = new List<TagCount>();
            List<Tags> tagovi = context.Tags.ToList();
            foreach (var item in tagovi)
            {
                TagCount t= new TagCount();
                int count = 0;
                count += GetCountTagByTagoviPitanja(item.TagID);
                count += GetCountTagByTagoviClanci(item.TagID);
                t.Name = item.Ime;
                t.CountT = count;
                tag.Add(t);
            }
            return tag.OrderByDescending(x => x.CountT).ToList();
        }
        public List<DBBL.DAL.Pitanja> GetAllPitanjaByTagName(string name)
        {
            Tags tag = context.Tags.Where(x => x.Ime == name).FirstOrDefault();
            if (tag != null)
            {
                var upit = (from p in context.Pitanja
                            join tg in context.TagoviPitanja on p.PitanjeID equals tg.PitanjeID
                            where tg.TagID == tag.TagID
                            select p).ToList();
                return upit;
            }
            return null;
        }
        public List<Sadrzaji> GetAllSadrzajiByTagName(string name)
        {
            Tags tag = context.Tags.Where(x => x.Ime == name).FirstOrDefault();
            if (tag != null)
            {
                var upit = (from c in context.Clanci
                            join s in context.Sadrzaji on c.ClanakID equals s.ClanakID
                            join t in context.TagClanci on c.ClanakID equals t.ClanakID
                            where t.TagID == tag.TagID
                            select s).ToList();
                return upit;
            }
            return null;
        }
        public Korisnici GetKorisnikByID(int id)
        {
            return context.Korisnici.Where(x => x.KorisnikID == id).FirstOrDefault();
        }
        public int GetBrojOdgovoraZaPitanje(int pitanjeID)
        {
            return context.Odgovori.Where(x => x.PitanjeID == pitanjeID).Count();
        }
        public List<DBBL.DAL.Pitanja> GetAllPitanjaByKorisnikId(int id)
        {
            return context.Pitanja.Where(X => X.KorisnikID == id).ToList();
        }
        public List<DBBL.DAL.Pitanja> GetAllPitanjaOdgovoriByKorisnikId(int id)
        {
            var upit = (from p in context.Pitanja
                        join o in context.Odgovori on p.PitanjeID equals o.PitanjeID
                        where o.KorisnikID == id
                        select p).ToList();
            return upit;
        }
        public List<DBBL.DAL.Sadrzaji> GetAllClanciByKorisnikId(int id)
        {
            var upit = (from c in context.Clanci
                        join s in context.Sadrzaji on c.ClanakID equals s.ClanakID
                        where c.KorisnikID == id
                        select s).ToList();
            return upit;
        }
        public void Dispose()
        {
            context.Dispose();
        }
    }
}