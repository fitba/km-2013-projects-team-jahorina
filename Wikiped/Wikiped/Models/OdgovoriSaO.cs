using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wikiped.Models
{
    public class OdgovoriSaO
    {
        private Wikiped.DBBL.DAL.Odgovori odgovori;
        private Wikiped.DBBL.DAL.Korisnici korisnik;
        private List<COdgovorNaOdgovor> odgovoriNaOdgovor;

        public List<COdgovorNaOdgovor> OdgovoriNaOdgovor
        {
            get { return odgovoriNaOdgovor; }
            set { odgovoriNaOdgovor = value; }
        }
        public Wikiped.DBBL.DAL.Korisnici Korisnik
        {
            get { return korisnik; }
            set { korisnik = value; }
        }
        public Wikiped.DBBL.DAL.Odgovori OdgovoriGlavni
        {
            get { return odgovori; }
            set { odgovori = value; }
        }
        public OdgovoriSaO()
        {
            odgovoriNaOdgovor = new List<COdgovorNaOdgovor>();
        }
    }
    public class COdgovorNaOdgovor
    {
        private Wikiped.DBBL.DAL.OdgovorNaOdgovor odgovoriNaOdgovor;
        private Wikiped.DBBL.DAL.Korisnici korisnik;
    
        public Wikiped.DBBL.DAL.Korisnici Korisnik
        {
            get { return korisnik; }
            set { korisnik = value; }
        }
        public Wikiped.DBBL.DAL.OdgovorNaOdgovor OdgovoriNaPodOgovor
        {
            get { return odgovoriNaOdgovor; }
            set { odgovoriNaOdgovor = value; }
        }

    }
}