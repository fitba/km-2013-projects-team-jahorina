using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wikiped.DBBL.DAL;
namespace Wikiped.DBBL.BLL
{
    public class Spajanje : IDisposable
    {
        WikipedEntities context;
        public Spajanje()
        {
            context = new WikipedEntities();
            context.Connection.Open();
        }

        public WikipedEntities Context { get { return this.context; } }

        public void Dispose()
        {
            context.Connection.Close();
            context.Dispose();
        }
    }
}