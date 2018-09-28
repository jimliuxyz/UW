using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using UW.Models.Collections;

namespace UW.Persis
{
    public abstract class Base
    {
        public static readonly Uri URI_DB = UriFactory.CreateDatabaseUri(R.DB_NAME);

        //user
        public static readonly string COL_USER = typeof(UW.Models.Collections.User).Name;
        public static readonly Uri URI_USER = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_USER);

        //sms passcode
        public static readonly string COL_SMSPCODE = typeof(SmsPasscode).Name;
        public static readonly Uri URI_SMSPCODE = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_SMSPCODE);

        //contact
        public static readonly string COL_CONTACT = typeof(Contacts).Name;
        public static readonly Uri URI_CONTACT = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_CONTACT);

        //balance
        public static readonly string COL_BALANCE = typeof(Balance).Name;
        public static readonly Uri URI_BALANCE = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_BALANCE);

        //receipts
        public static readonly string COL_TXRECEIPT = typeof(TxReceipt).Name;
        public static readonly Uri URI_TXRECEIPT = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_TXRECEIPT);

        //tx locker
        public static readonly string COL_TXLOCKER = typeof(TxLocker).Name;
        public static readonly Uri URI_TXLOCKER = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_TXLOCKER);


        public static readonly RequestOptions DefReqOpts = new RequestOptions { OfferThroughput = 400 }; //todo:實際運作400RU可能太小

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

                DocumentCollection collectionDefinition = new DocumentCollection { Id = COL_TXLOCKER, DefaultTimeToLive = 60 };
                collectionDefinition.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });
                collectionDefinition.PartitionKey.Paths.Add("/pk");
                collectionDefinition.UniqueKeyPolicy = new UniqueKeyPolicy
                {
                    UniqueKeys =
                    new Collection<UniqueKey>
                    {
                        new UniqueKey { Paths = new Collection<string> { "/memId" }},
                    }
                };
                await client.CreateDocumentCollectionIfNotExistsAsync(URI_DB, collectionDefinition, DefReqOpts);

            }
        }
    }
}