# LiteDBDirectory

A Lucene.net Directory to store the Lucene.net index files in a LiteDB database to secure the indexed data with a LiteDb password connection.  The base implementation is taken from [LuceneNetSqlDirectory](https://github.com/MahyTim/LuceneNetSqlDirectory) and converted for LiteDb.

The solution is only for desktop and small ASP.NET applications.  For large applications hosted on webfarms, it is recommended to use [LuceneNetSqlDirectory](https://github.com/MahyTim/LuceneNetSqlDirectory).

## Nuget Package
```
Install-Package LuceneLiteDbDirectory
```

## Using LiteDbDirectory
Initializing LiteDbDirectory
```C#
string connectionString = $"Filename={Path.Combine(Environment.CurrentDirectory, "MyIndex.Db")};password=somepassword;";

using (var db = new LiteDatabase(connectionString))
{
    LiteDbDirectory liteDbDirectory = new LiteDbDirectory(db);
    try
    {
        liteDbDirectory.CheckRequiredCollection();
    }
    catch (ConfigurationErrorsException e)
    {
        LiteDbDirectory.CreateRequiredCollections(db, dropExisting: true);
    }
}
```
Using LiteDbDirectory for indexing data  
```c#
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store.LiteDbDirectory;
/* Indexing code */
using (var db = new LiteDatabase(connectionString))
{
	var analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
	var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
 	indexWriter = new IndexWriter(directory, config);

	var bookPages = _libraryService.GetAllBookPages();  // You service layer to load data

	foreach(var page in bookPages)
	{
		var bookPageDoc = new Document();

                doc.Add(new StringField("id", page.Id, Field.Store.YES));
                doc.Add(new TextField("book-title", page.Title, Field.Store.YES));
                doc.Add(new TextField("book-page", page.Text, Field.Store.NO));
			
		indexWriter.AddDocument(bookPageDoc);
	}
	indexWriter.Flush(true, true);
	indexWriter.Commit();
	indexWriter.Dispose();
}
```
Using LiteDbDirectory for Searching data 
```c#
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store.LiteDbDirectory;
/* searching code */
using (var db = new LiteDatabase(connectionString))
{
	LiteDbDirectory liteDbDirectory = new LiteDbDirectory(db);
    
	IndexSearcher searcher = new IndexSearcher(liteDbDirectory);
	var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "book-page", new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));
	var query = parser.Parse("text to search");
	hits = searcher.Search(query, 100);

	Console.WriteLine("Found {0} results for {1}", hits.TotalHits, phrase);
	foreach (var hitsScoreDoc in hits.ScoreDocs)
	{
	   var doc = searcher.IndexReader.Document(hitsScoreDoc.Doc);
	  Console.WriteLine("Book id: {0}", doc.GetValues("id")[0]);
	}
}
```
