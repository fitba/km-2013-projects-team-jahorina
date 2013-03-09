using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Version = Lucene.Net.Util.Version;
using System.IO;
using Wikiped.DBBL.DAL;

namespace Wikiped.Models
{
    /// <summary>
    /// Otvorite Package Manager Console in Visual Studio:
    /// View > Other Windows > Package Manager Console u unesite : install-package Lucene.Net
    /// Ovo je potrebno insalirati u vs u kako bi mogli koristi navedene namespace koji su ukljuceni
    /// jer inace lucinu necete moic koristi
    /// </summary>
    public static class LucenePt
    {
        #region property
        /// <summary>
        /// lucene treba da izgradi search index, to ce se staviti u fajl koji je generisan od lucine na lokalni direktori
        /// ovdje je fizicka putanja do direktorija koja ce se nalaziti na root levelu, sa imenom foldera lucene_index
        /// </summary>
        public static string _luceneDir =
            Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "lucene_index");
        private static FSDirectory _directoryTemp;
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(_luceneDir, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }
        #endregion

        // search methods
        public static IEnumerable<LuceneObject> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<LuceneObject>();

            // set up lucene searcher
            var searcher = new IndexSearcher(_directory, false);
            var reader = IndexReader.Open(_directory, false);
            var docs = new List<Document>();
            var term = reader.TermDocs();
            // v 2.9.4: use 'term.Doc()'
            // v 3.0.3: use 'term.Doc'
            while (term.Next()) docs.Add(searcher.Doc(term.Doc));
            reader.Dispose();
            searcher.Dispose();
            return _mapLuceneToDataList(docs);
        }

        #region Search fukcije
        // Ako proslijedimo rijec Pret dodaje se * i vraca sve iteme koji imaju u sebi pret
        public static IEnumerable<LuceneObject> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input)) return new List<LuceneObject>();

            var terms = input.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }
        // Vraca iteme koji imaju rijec kao proslijedenu Pret trazi se match iste rijeci
        public static IEnumerable<LuceneObject> SearchDefault(string input, string fieldName = "")
        {
            return string.IsNullOrEmpty(input) ? new List<LuceneObject>() : _search(input, fieldName);
        }
        #endregion

        /// <summary>
        /// glavna pretraga fukcija
        /// parsira search string query u Lucene query object,vazno napomenu ti var hits_limit = 1000; jer varijabla
        /// koja kaze da lucina ne vraca vise od 1000 rezultata, razlog sto je ogranicena na 1000 jer preko lucina postaje spora
        /// u koliko u fukciji nismo proslijedili specifino polje za pretragu pretraga ce se vristi na svim poljima
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="searchField"></param>
        /// <returns></returns>
        private static IEnumerable<LuceneObject> _search(string searchQuery, string searchField = "")
        {
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<LuceneObject>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory, false))
            {
                var hits_limit = 1000;
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);

                // search by single field
                if (!string.IsNullOrEmpty(searchField))
                {
                    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, hits_limit).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
                // search by multiple fields (ordered by RELEVANCE)
                else
                {
                    var parser = new MultiFieldQueryParser
                        (Version.LUCENE_30, new[] { "Id", "Name", "Description", "IsQuestion" }, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, null, hits_limit, Sort.INDEXORDER).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
            }
        }
        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        #region map Lucene search index to data
        private static IEnumerable<LuceneObject> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }
        private static IEnumerable<LuceneObject> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }
        /// <summary>
        /// mapira lucine serch index u nas tip podatka
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static LuceneObject _mapLuceneDocumentToData(Document doc)
        {
            return new LuceneObject
            {
                Id = Convert.ToInt32(doc.Get("Id")),
                Name = doc.Get("Name"),
                Description = doc.Get("Description"),
                IsQuestion =Boolean.Parse(doc.Get("IsQuestion"))
            };
        }
        #endregion

        #region add/update/clear search index data

        /// <summary>
        /// Dodajemo u lucine search index listu podataka proslijedeni,koristenje using otvaramo lucinin dokument gdje ce se vrsiti
        /// upis
        /// </summary>
        /// <param name="sampleDatas"></param>
        public static void AddUpdateLuceneIndex(IEnumerable<LuceneObject> sampleDatas)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entries if any)
                foreach (var sampleData in sampleDatas)
                    _addToLuceneIndex(sampleData, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
        public static void AddUpdateLuceneIndex(LuceneObject sampleData)
        {
            AddUpdateLuceneIndex(new List<LuceneObject> { sampleData });
        }

        /// <summary>
        /// Ukoliko neki podatak izbrisemo iz baze podataka, potrebno ga je izbrisati iz lucine sarch indexa, jer da ne bi pretraga
        /// varaca podatke koje ne postoje u bazi podataka,item se brise pomocu id-a
        /// </summary>
        /// <param name="record_id"></param>
        public static void ClearLuceneIndexRecord(int record_id)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("Id", record_id.ToString()));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        /// <summary>
        /// Takodje u koliko se promjeni sema baze podataka, zelimo da sve podatke izbrisemo iz lucine sarch index-a
        /// lucine uvijek stavlja na fajlove "lock" jer kada pocinjemo sa pretragom ili updetom podaci ne mogu biti mjenjani
        /// takodje svi fajlovi su "unlocked" jer fajlovi mogu manuelno izbrisani, i ako se pokusa pretraga uraditi ili update
        /// dobit ce se dosta errora, da bi izbjegi navedene probleme koristimo .Close() and .Dispose() 
        /// </summary>
        /// <returns></returns>
        public static bool ClearLuceneIndex()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion

        /// <summary>
        /// u koliko nasih indexa imamo dosta i fajl postaje sve veci da bi ubrzali nasu pretragu potrebno je uraditi optimize
        /// </summary>
        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }

        /// <summary>
        /// Lucene kreira search index na osnovu podataka u nasem slucaju nase klase PitanjaLucene,
        /// ova metoda kreira jedan search index na osnovu naseg podatka,ona uzme jedan podatak i mapira ga u Lucene class Dokument
        /// i doda seach index uz pomoc index writer-a u fukciji smo dodali Field.Index.ANALYZED samo na Name i Desrcripton properety
        /// iz razloga sto zelimo u pretragi koristi samo ta dva polja jer je to text,ID su jedinstvene vrijednosti pa koje ne zelimo analizirati
        /// pa koristimo Field.Index.NOT_ANALYZED
        /// </summary>
        /// <param name="sampleData"></param>
        /// <param name="writer"></param>
        private static void _addToLuceneIndex(LuceneObject sampleData, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("Id", sampleData.Id.ToString()));
            writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields
            doc.Add(new Field("Id", sampleData.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Name", sampleData.Name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Description", sampleData.Description, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("IsQuestion", sampleData.IsQuestion.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            // add entry to index
            writer.AddDocument(doc);
        }

        #region Pitanja i Clanci
        public static List<LuceneObject> GetAllClanke()
        {
            List<LuceneObject> lstClanci = new List<LuceneObject>();
            WikipedEntities context = new WikipedEntities();
            List<Sadrzaji> lstSadrzaji= context.Sadrzaji.ToList();
            foreach (var item in lstSadrzaji)
            {
                LuceneObject temp = new LuceneObject();
                temp.Id = item.SadrzajID;
                temp.Name = item.Naslov;
                temp.Description = item.Tekst;
                temp.IsQuestion = false;
                lstClanci.Add(temp);
            }
            return lstClanci;
        }
        public static List<LuceneObject> GetAllPitanja()
        {
            List<DBBL.DAL.Pitanja> pitanja;
            using (Pitanja pt = new Pitanja())
            {
                pitanja = pt.GetAllPitanja();
            }
            List<LuceneObject> lstPitanjeLucne = new List<LuceneObject>();
            foreach (var item in pitanja)
            {
                LuceneObject pt = new LuceneObject();
                pt.Id = item.PitanjeID;
                pt.Name = item.Naziv;
                pt.Description = item.Opis;
                pt.IsQuestion = true;
                lstPitanjeLucne.Add(pt);
            }
            return lstPitanjeLucne;
        }
        public static List<LuceneObject> GetAllObjectForLuceneIndex()
        {
            List<LuceneObject> temp=new List<LuceneObject>();
            temp.AddRange(GetAllClanke());
            temp.AddRange(GetAllPitanja());
            return temp;
        }
        #endregion

    }
}