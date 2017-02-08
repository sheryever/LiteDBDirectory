# LiteDBDirectory

A Lucene.net Directory to store the Lucene.net index files in LiteDB database to secure the indexed data with LiteDb password connection, The base implementation is taken from [LuceneNetSqlDirectory](https://github.com/MahyTim/LuceneNetSqlDirectory) and converted for LiteDb.

The solution is only for desktop and small Asp.net applications for the lage applications hosted on webfarm it is recommanded to use [LuceneNetSqlDirectory](https://github.com/MahyTim/LuceneNetSqlDirectory).

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
	var indexWriter = new IndexWriter(directory, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
		!IndexReader.IndexExists(directory),
		new IndexWriter.MaxFieldLength(IndexWriter.DEFAULT_MAX_FIELD_LENGTH));

	indexWriter.SetRAMBufferSizeMB(500);

	var bookPages = _libraryService.GetAllBookPages();  // You service layer to load data

	foreach(var page in bookPages)
	{
		var bookPageDoc = new Document();
		doc.Add(new Field("id", page.Id, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
		doc.Add(new Field("book-title", page.Title, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));
		doc.Add(new Field("book-page", page.Text, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));
			
		indexWriter.AddDocument(bookPageDoc);
	}
	indexWriter.Flush(true, true, true);
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
	var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "book-page", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
	var query = parser.Parse("text to search");
	hits = searcher.Search(query, 100);

	Console.WriteLine("Found {0} results for {1}", hits.TotalHits, phrase);
	foreach (var hitsScoreDoc in hits.ScoreDocs)
	{
	  var doc = searcher.IndexReader[hitsScoreDoc.Doc];
	  Console.WriteLine("Book id: {0}", doc.GetValues("id")[0]);
	}
}
```
