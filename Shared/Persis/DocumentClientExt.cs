using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using UW.Shared.Misc;

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
            var timer = new Stopwatch();
            timer.Start();
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
            Console.WriteLine("GetDocsCount : " + timer.Elapsed.TotalSeconds);

            return count;
        }

        /// <summary>
        /// Clear all documents of the collection
        /// </summary>
        /// <param name="client"></param>
        /// <param name="collectionUri"></param>
        /// <returns></returns>
        public static async Task ClearCollectionAsync(this DocumentClient client, Uri collectionUri)
        {
            var sqlquery = "SELECT * FROM c";
            var results = client.CreateDocumentQuery<DocumentWithPk>(collectionUri, sqlquery, new FeedOptions
            {
                EnableCrossPartitionQuery = true,
                MaxItemCount = 100
            }).AsDocumentQuery();

            var cnt = 0;
            double ruamount = 0;
            while (results.HasMoreResults)
            {
                // Console.WriteLine($"Has more({cnt})...");
                foreach (DocumentWithPk doc in await results.ExecuteNextAsync())
                {
                    var res = await client.DeleteDocumentAsync(doc.SelfLink, new RequestOptions
                    {
                        PartitionKey = new PartitionKey(doc.pk)
                    });
                    ruamount += res.RequestCharge;
                }
                cnt++;
            }
            Console.WriteLine(String.Format("ClearCollectionAsync RU: {0}", ruamount));
        }

        public static async Task<ResourceResponse<Document>> CreateDocumentIfNotExists(this DocumentClient client, string dbId, string collectionId, string docId, object doc, string pk)
        {
            var docUri = UriFactory.CreateDocumentUri(dbId, collectionId, docId);

            try
            {
                var res = await client.ReadDocumentAsync(docUri, new RequestOptions()
                {
                    PartitionKey = new PartitionKey(pk)
                });
                return res;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                }
                else
                {
                    // HttpStatusCode.TooManyRequests //must retry
                    Console.WriteLine("DocumentClientException : " + e.StatusCode);
                    throw e;
                }
            }

            try
            {
                var res = await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(dbId, collectionId), doc);

                return res;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}