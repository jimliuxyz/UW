using System;
using System.Collections.Generic;
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

        public static void GetPartitionInfo(this DocumentClient client, Uri collectionUri)
        {
            DocumentCollection collection = client.ReadDocumentCollectionAsync(collectionUri,
                new RequestOptions { PopulatePartitionKeyRangeStatistics = true }).Result;

            foreach (var partitionKeyRangeStatistics in collection.PartitionKeyRangeStatistics)
            {
                Console.WriteLine("PartitionKeyRangeId : " + partitionKeyRangeStatistics.PartitionKeyRangeId);
                Console.WriteLine(" DocumentCount : " + partitionKeyRangeStatistics.DocumentCount);
                Console.WriteLine(" SizeInKB : " + partitionKeyRangeStatistics.SizeInKB);

                foreach (var partitionKeyStatistics in partitionKeyRangeStatistics.PartitionKeyStatistics)
                {
                    Console.WriteLine($"  PartitionKey : {partitionKeyStatistics.PartitionKey}  {partitionKeyStatistics.SizeInKB}KB");
                }
            }
        }

        public static async Task<List<T>> ToListAsync<T>(this IDocumentQuery<T> queryable)
        {
            var watch = new Stopwatch();
            watch.Start();

            var list = new List<T>();
            var ru = 0.0;
            while (queryable.HasMoreResults)
            {   //Note that ExecuteNextAsync can return many records in each call
                var response = await queryable.ExecuteNextAsync<T>();
                list.AddRange(response);
                ru += response.RequestCharge;
            }
            Console.WriteLine(String.Format("queryable.ToListAsync / RU: {0} / Elapsed: {1}", ru, watch.Elapsed.TotalSeconds));

            return list;
        }
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> query)
        {
            return await query.AsDocumentQuery().ToListAsync();
        }

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

            // var timer = new Stopwatch();
            // timer.Start();
            // var query = client.CreateDocumentQuery<object>(collectionUri, new SqlQuerySpec("SELECT VALUE COUNT(1) FROM c"), DefaultOptions).AsDocumentQuery();
            // FeedResponse<long> queryResult = query.ExecuteNextAsync<long>().Result;
            // Console.WriteLine("GetDocsCount2 : " + timer.Elapsed.TotalSeconds);
            // Console.WriteLine("requestCharge : " + queryResult.RequestCharge);
            // Console.WriteLine("queryResult : " + queryResult.First());


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

        public static async Task CreateListOfStoredProcedureAsync(this DocumentClient client, string dbname, string colname, List<StoredProcedure> list)
        {
            var uriCol = UriFactory.CreateDocumentCollectionUri(dbname, colname);
            foreach (var sproc in list)
            {
                try
                {
                    StoredProcedure sp = await client.ReadStoredProcedureAsync(UriFactory.CreateStoredProcedureUri(dbname, colname, sproc.Id));

                    if (sp.Body == sproc.Body)
                        return;
                    else if (sp != null)
                    {
                        //todo : remove DeleteStoredProcedureAsync
                        await client.DeleteStoredProcedureAsync(UriFactory.CreateStoredProcedureUri(dbname, colname, sproc.Id));
                    }
                }
                catch (System.Exception) { }

                Console.WriteLine(" ... create store procedure `{0}`", sproc.Id);
                await client.CreateStoredProcedureAsync(uriCol, sproc);
            }
        }
    }
}