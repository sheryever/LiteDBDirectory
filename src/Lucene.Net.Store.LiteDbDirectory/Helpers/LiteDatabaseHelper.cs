using LiteDB;

namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    static class LiteDatabaseHelper
    {
        public static void DropTableIfExists(this LiteDatabase db, string collectionName)
        {
            if (db.CollectionExists(collectionName))
            {
                db.DropCollection(collectionName);
            }
        }
    }
}