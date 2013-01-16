using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wikiped.Models
{
    public class KorisniciServis
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public string Grad { get; set; }
        public string Adresa { get; set; }
        public int Drzava { get; set; }
        public int Vrsta { get; set; }
    }
}