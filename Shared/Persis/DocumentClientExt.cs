using System;
using System.Linq;
using Microsoft.Azure.Documents.Client;

namespace UW.Shared.Persis
{
    public static class DocumentClientExt
    {
        private static readonly FeedOptions DefaultOptions = new FeedOptions { EnableCrossPartitionQuery = true };

        public static decimal GetDocsCount(this DocumentClient client, Uri collectionUri)
        {
            // SQL
            int count = client.CreateDocumentQuery<int>(
                collectionUri,
                // "SELECT VALUE COUNT(f) FROM Families f WHERE f.LastName = 'Andersen'",
                "SELECT VALUE COUNT(1) FROM c",
                DefaultOptions)
                .AsEnumerable().First();

            // LINQ
            count = client.CreateDocumentQuery(collectionUri, DefaultOptions)
                .Where(doc => true)
                .Count();

            return count;
        }
    }
}