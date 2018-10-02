using System;
using System.Linq;
using Microsoft.Azure.Documents.Client;

/*
more sample queries
https://github.com/Azure/azure-cosmosdb-dotnet/blob/master/samples/code-samples/Queries/Program.cs
*/

namespace UW.Shared.Persis
{
    public static class DocumentClientExt
    {
        private static readonly FeedOptions DefaultOptions = new FeedOptions { EnableCrossPartitionQuery = true };


        /// <summary>
        /// Get document count of collection
        /// todo : someone says this may not work in huge collection
        /// </summary>
        /// <param name="client"></param>
        /// <param name="collectionUri"></param>
        /// <returns></returns>
        public static long GetDocsCount(this DocumentClient client, Uri collectionUri)
        {
            // SQL
            long count = client.CreateDocumentQuery<long>(
                collectionUri,
                "SELECT VALUE COUNT(1) FROM c",
                DefaultOptions)
                .AsEnumerable().First();

            // LINQ
            // count = client.CreateDocumentQuery(collectionUri, DefaultOptions)
            //     .Where(doc => true)
            //     .LongCount();

            return count;
        }
    }
}