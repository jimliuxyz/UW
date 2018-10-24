using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using UW.Core.Persis.Collections;
using User = UW.Core.Persis.Collections.User;

namespace UW.Core.Persis.Helper
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




                await client.CreateDocumentCollectionIfNotExistsAsync(
                    BalanceV2._URI_DB, BalanceV2._SPEC, DEFREQ_OPTS);
                await client.CreateListOfStoredProcedureAsync(BalanceV2._DB_NAME, BalanceV2._COLLECTION_NAME, BalanceV2._SPROCs);

                await client.CreateDocumentCollectionIfNotExistsAsync(
                    User._URI_DB, User._SPEC, DEFREQ_OPTS);
                await client.CreateListOfStoredProcedureAsync(User._DB_NAME, User._COLLECTION_NAME, User._SPROCs);

            }
        }

        /// <summary>
        /// parse exMsg to dynamic object
        /// </summary>
        /// <param name="exMsg">stored procedure exception message</param>
        /// <returns>{Message: "error.", CustomError: {code:-1, message: "error..."}}</returns>
        public static dynamic ToSPException(string exMsg)
        {
            try
            {
                //Message: {"Errors":["Encountered exception while executing function. Exception = {\"mydata\":\"123\"}"]}
                var json = "";
                var arr = Regex.Split(exMsg, "(\r\n)");

                for (int i = 0; i < arr.Length; i++)
                {
                    var line = arr[i];
                    if (i == 0) //first line for 'Message'
                    {
                        var arr2 = line.Split(":", 2);
                        if (arr2 != null && arr2.Length == 2)
                        {
                            if (arr2[0] == "Message")
                            {
                                var obj = arr2[1].ToObject();

                                var arr3 = ((string)obj.Errors[0]).Split(" Exception = ", 2);
                                if (arr3 != null && arr3.Length == 2)
                                {
                                    json += $"\"Message\" : \"{arr3[0]}\"" + ",";
                                    json += $"\"CustomError\" : {arr3[1]}" + ",";
                                }
                            }
                        }
                    }
                    else
                    {
                        arr = Regex.Split(line, ",");
                        foreach (var str in arr)
                        {
                            var arr2 = str.Split(":", 2);
                            if (arr2 != null && arr2.Length == 2)
                            {
                                json += $"\"{arr2[0].Replace(" ", "")}\" : \"{arr2[1]}\"" + ",";
                            }
                        }
                    }
                }
                var res = ("{" + json + "}").ToObject();
                return res;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception("Can't Convert Stored Proceduce Exception!\n" + exMsg);
            }
        }


    }
}