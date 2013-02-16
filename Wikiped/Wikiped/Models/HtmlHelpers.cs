using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace Wikiped.Models
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString Ratings(this HtmlHelper helper, double rating, int idClanka, int Iduser)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<span class='rating' rating='{0}' post='{1}' title='ocjena: {2}'>", rating * 2, idClanka, rating);

            string formatStr = "<img src='/Images/{0}' alt='star'  width='7' height='12' class='star' value='{1}' />";
            int brojac = 0;
            for (Double i = .5; i <= 5.0; i = i + .5)
            {
                brojac = brojac + 1;

                if (i <= rating)
                {
                    sb.AppendFormat(formatStr, (i * 2) % 2 == 1 ? "leftON.gif" : "rightON.gif", brojac);
                }
                else
                {
                    sb.AppendFormat(formatStr, (i * 2) % 2 == 1 ? "leftOFF.gif" : "rightOFF.gif", brojac);
                }
            }
            sb.AppendFormat(" <span class='ocjene-text'>(od {0} korisnika)</span>", Iduser);
            sb.Append("</span>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}