using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using UW.Shared.Persis.Collections;
using User = UW.Shared.Persis.Collections.User;

namespace UW.Shared.Persis.Helper
{
    public abstract class PersisHelper
    {
        public static readonly RequestOptions DEFREQ_OPTS = new RequestOptions { OfferThroughput = 400 }; //todo:實際運作400RU可能太小

        /// <summary>
        /// get client by arranged
        /// </summary>
        private static DocumentClient _client = null;
        public static DocumentClient GetClient()
        {
            _client = _client ?? new DocumentClient(new Uri(R.DB_URI), R.DB_KEY);
            return _client;
        }
        public static DocumentClient GetNewClient()
        {
            return new DocumentClient(new Uri(R.DB_URI), R.DB_KEY);
        }

        public static async Task BuildDB()
        {
            using (var client = new DocumentClient(new Uri(R.DB_URI), R.DB_KEY))
            {
                // try
                // {
                //     await client.ReadDatabaseAsync(URI_DB);
                //     return;
                // }
                // catch (DocumentClientException de)
                // {
                //     if (de.StatusCode != HttpStatusCode.NotFound)
                //         return;
                // }
                //create database
                await client.CreateDatabaseIfNotExistsAsync(new Database { Id = R.DB_NAME });

                //create collections
                // await client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                //                     new DocumentCollection { Id = COL_USER }, DefReqOpts);
                // await client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                //                     new DocumentCollection { Id = COL_SMSPCODE, DefaultTimeToLive = 60 * 60 }, DefReqOpts); //60min for a round
                // await client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                //                     new DocumentCollection { Id = COL_CONTACT }, DefReqOpts);
                // await client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                //                     new DocumentCollection { Id = COL_BALANCE }, DefReqOpts);
                // await client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                //                     new DocumentCollection { Id = COL_TXRECEIPT }, DefReqOpts);




                // await client.CreateDocumentCollectionIfNotExistsAsync(
                //     TxLocker._URI_DB, TxLocker._SPEC, DEFREQ_OPTS);

                await client.CreateDocumentCollectionIfNotExistsAsync(
                    User._URI_DB, User._SPEC, DEFREQ_OPTS);

                await client.CreateListOfStoredProcedureAsync(User._DB_NAME, User._COLLECTION_NAME, User._SPROCs);

            }
        }


    }
}