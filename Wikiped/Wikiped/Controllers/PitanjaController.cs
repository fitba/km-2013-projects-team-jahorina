using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wikiped.Models;
namespace Wikiped.Controllers
{
    public class PitanjaController : Controller
    {
        //
        // GET: /Pitanja/

        public ActionResult Index()
        {
            List<DBBL.DAL.Pitanja> pitanja;
            using (Pitanja pt = new Pitanja())
            {
                pitanja = pt.GetAllPitanja();
            }
            return View(pitanja);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string tags)
        {
            string[] tagsA = null;
            if (tags != String.Empty)
            {
                tagsA = tags.Split(',');
            };

            return Content(tags);
        }

        public ActionResult Details(int id)
        {
            Pitanja pitanje = new Pitanja();
            using (Pitanja pt = new Pitanja())
            {
                pt.SetAllDetaByPitanjeID(id);
                pitanje = pt;
            }

            return View(pitanje);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Details(int id, string txtComment)
        {
            if (!String.IsNullOrEmpty(txtComment))
            {
                DBBL.DAL.Odgovori o = new DBBL.DAL.Odgovori();
                o.PitanjeID = id;
                o.Odgovor = txtComment;
                o.Datum = DateTime.Now;
                o.KorisnikID = 1;
                o.BrojGlasova = 0;

                Pitanja pitanje = new Pitanja();
                using (Pitanja pt = new Pitanja())
                {
                    pt.AddAnswer(o);
                    pt.SetAllDetaByPitanjeID(id);
                    pitanje = pt;
                }

                return View(pitanje);
            }
            Pitanja pitanje2 = new Pitanja();
            using (Pitanja pt = new Pitanja())
            {
                pt.SetAllDetaByPitanjeID(id);
                pitanje2 = pt;
            }

            return View(pitanje2);
        }
        #region Ajax Vote and Comment

        public ActionResult VoteUp(int id, bool mainQuestion)
        {
            int korisnikId = 1;
            int voteNumber = default(int);
            using (Pitanja pt = new Pitanja())
            {
                if (mainQuestion == true)
                {
                    voteNumber = pt.PitanjeVoteUp(id, korisnikId);
                }
                else
                {
                    voteNumber = pt.OdgovorVoteUp(id, korisnikId);
                }
            }

            return Json(new { voteNumber = voteNumber });
        }
        public ActionResult VoteDown(int id, bool mainQuestion)
        {
            int korisnikId = 1;
            int voteNumber = default(int);
            using (Pitanja pt = new Pitanja())
            {
                if (mainQuestion == true)
                {
                    voteNumber = pt.PitanjeVoteDown(id, korisnikId);
                }
                else
                {
                    voteNumber = pt.OdgovorVoteDown(id, korisnikId);
                }
            }
            return Json(new { voteNumber = voteNumber });
        }
        public ActionResult AddCommentForAnswer(int id, string textComm)
        {
            if (!String.IsNullOrEmpty(textComm))
            {
                DBBL.DAL.OdgovorNaOdgovor tempOdg = new DBBL.DAL.OdgovorNaOdgovor();
                tempOdg.OdgovorID = id;
                tempOdg.Odgovor = textComm;
                tempOdg.KorisnikID = 1;
                tempOdg.Datum = DateTime.Now;
                using (Pitanja pt = new Pitanja())
                {
                    pt.AddCommentOnAnswer(tempOdg);
                }
                return Json(new { voteNumber = 1 });
            }
            return View();
        }

        #endregion
        //povis clase mora biti [Serializable]
        public ActionResult GetTags(string id)
        {
            List<DBBL.DAL.Tags> lstTags;
            using (Pitanja pt = new Pitanja())
            {
                lstTags = pt.GetAllTags();
            }
            List<string> lstTagovi = new List<string>();

            List<MyTags> lstMTasg = new List<MyTags>();
            foreach (var item in lstTags)
            {
                MyTags temp = new MyTags();
                temp.Id = item.TagID;
                temp.Name = item.Ime;
                lstMTasg.Add(temp);
            }

            //return Json(lstTagovi);
            return Json(lstMTasg);
            // JavaScriptSerializer serializer = new JavaScriptSerializer();
            //var temp = lstTags;
            //return Json(lstTags, JsonRequestBehavior.AllowGet);
            //return Json(new { admir = "test" });
            // return Json(MvcHtmlString.Create(serializer.Serialize(lstTags)));
            // return Json(new { "Name" = "Ghost Bar", "Address" = "2440 Victory Park Lane", "OpenDate"="Open"});

        }
        //public static MvcHtmlString ToJson(this HtmlHelper html, object obj)
        //{
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    return MvcHtmlString.Create(serializer.Serialize(obj));
        //}

        //public static MvcHtmlString ToJson(this HtmlHelper html, object obj, int recursionDepth)
        //{
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    serializer.RecursionLimit = recursionDepth;
        //    return MvcHtmlString.Create(serializer.Serialize(obj));
        //}
        // <script>
        // var s = @(Html.ToJson(Model.Content));
        //</script>
    }

    [Serializable]
    public class MyTags
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
