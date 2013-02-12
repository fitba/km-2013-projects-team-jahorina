using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wikiped.Models
{
    public static class ExtensionMethod
    {
        public static int ToInt(this string s)
        {
            int value=default(int);
            int.TryParse(s, out value);
            return value;
        }
       
    }
}