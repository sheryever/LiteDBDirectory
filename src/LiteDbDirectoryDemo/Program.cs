using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace SQLiteDirectoryDemo
{
    class Program
    {
        private static string connectionString =
           $"Filename={Path.Combine(Environment.CurrentDirectory, "index.ldib")}";

        static void Main(string[] args)
        {

            using (var db = new LiteDatabase(connectionString))
            {
                LiteDbDirectory liteDbDirectory = new LiteDbDirectory(db);
            }

            Do();
        }

        static void LockCanBeReleased()
        {
            using (var db = new LiteDatabase(connectionString))
            {

                StandardAnalyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);

                var directory = new LiteDbDirectory(db);
                var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);

                IndexWriter indexWriter = null;
                while (indexWriter == null)
                {
                    try
                    {
                        indexWriter = new IndexWriter(directory, config);
                    }
                    catch (LockObtainFailedException)
                    {
                        Console.WriteLine("Lock is taken, waiting for timeout...{0}", DateTime.Now);
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        static void Do()
        {
            using (var db = new LiteDatabase(connectionString))
            {
                
                var directory = new LiteDbDirectory(db);

                IndexTempData(directory);

                IndexSearcher searcher;
                var reader = DirectoryReader.Open(directory);

                using (new AutoStopWatch("Creating searcher"))
                {
                    searcher = new IndexSearcher(reader);
                }
                using (new AutoStopWatch("Count"))
                    Console.WriteLine("Number of docs: {0}", searcher.IndexReader.NumDocs);

                while (true)
                {
                    SearchForPhrase(searcher, "microsoft");
                    Thread.Sleep(1000);
                    //Console.WriteLine("Press a key to search again");
                    //Console.ReadKey();
                }
            }
        }

        private static void IndexTempData(LiteDbDirectory directory)
        {
            for (int outer = 0; outer < 5; outer++)
            {
                IndexWriter indexWriter = null;
                while (indexWriter == null)
                {
                    try
                    {
                        var analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
                        var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
                        indexWriter = new IndexWriter(directory, config);
                    }
                    catch (LockObtainFailedException)
                    {
                        Console.WriteLine("Lock is taken, waiting for timeout...");
                        Thread.Sleep(1000);
                    }
                }

                Console.WriteLine("IndexWriter lock obtained, this process has exclusive write access to index");

                //indexWriter.SetInfoStream(new StreamWriter(Console.OpenStandardOutput()));
                //indexWriter.UseCompoundFile = false;

                for (int iDoc = 0; iDoc < 1000; iDoc++)
                {
                    //if (iDoc % 10 == 0)
                    // Console.WriteLine(iDoc);
                    Document doc = new Document();
                    doc.Add(new StringField("id", DateTime.Now.ToFileTimeUtc().ToString(), Field.Store.YES));
                    doc.Add(new TextField("Title", "dog " + GeneratePhrase(50), Field.Store.YES));
                    doc.Add(new TextField("Body", "dog " + GeneratePhrase(50), Field.Store.NO));
                    indexWriter.AddDocument(doc);
                    //GC.Collect();
                }

                Console.WriteLine("Total docs is {0}", indexWriter.NumDocs);

                Console.WriteLine("Flushing and disposing writer...");
                indexWriter.Flush(true, true);
                //indexWriter.Dispose();
                indexWriter.Commit();
                indexWriter.Dispose();
                GC.Collect();
            }
        }


        static void SearchForPhrase(IndexSearcher searcher, string phrase)
        {
            using (new AutoStopWatch($"Search for {phrase}"))
            {
                var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "Body", new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));
                Lucene.Net.Search.Query query = parser.Parse(phrase);
                searcher.Search(query, 10);
                var hits = searcher.Search(new TermQuery(new Term("Title", "find me")), 100);

                hits = searcher.Search(query, 100);
                Console.WriteLine("Found {0} results for {1}", hits.TotalHits, phrase);

                foreach (var hitsScoreDoc in hits.ScoreDocs)
                {
                    var doc = searcher.IndexReader.Document(hitsScoreDoc.Doc);
                    Console.WriteLine("Book title: {0}", doc.GetValues("Title")[0]);
                }
            }
        }

        static readonly Random Random = new Random((int)DateTime.Now.Ticks);
        static readonly string[] SampleTerms =
            {
                "dog","cat","car","horse","door","tree","chair","microsoft","apple","adobe","google","golf","linux","windows","firefox","mouse","hornet","monkey","giraffe","computer","monitor",
                "steve","fred","lili","albert","tom","shane","gerald","chris",
                "love","hate","scared","fast","slow","new","old"
            };

        private static string GeneratePhrase(int MaxTerms)
        {
            StringBuilder phrase = new StringBuilder();
            int nWords = 2 + Random.Next(MaxTerms);
            for (int i = 0; i < nWords; i++)
            {
                phrase.AppendFormat(" {0} {1}", SampleTerms[Random.Next(SampleTerms.Length)], Random.Next(32768).ToString());
            }
            return phrase.ToString();
        }

    }
}
