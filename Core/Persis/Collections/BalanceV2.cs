using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Core.Persis.Collections
{
    public partial class BalanceV2
    {
        [JsonProperty(PropertyName = "id")]
        public string userId { get; set; }
        public string pk { get; set; }
        // public string currency { get; set; }
        // public decimal balance { get; set; }
        public Dictionary<string, decimal> balance { get; set; }
        public Dictionary<string, FlowBuf> outBuf { get; set; }
        public Dictionary<string, FlowBuf> inBuf { get; set; }
        // public List<FlowBuf> outBuf = new List<FlowBuf>();
        // public List<FlowBuf> inBuf = new List<FlowBuf>();
    }

    public class FlowBuf
    {
        public string currency { get; set; }
        public long time { get; set; }
        public string txId { get; set; }
        public string receiptId { get; set; }
        public decimal amount { get; set; }
        public bool confirm { get; set; } = false;
    }


    public partial class BalanceV2
    {
        public static readonly string _COLLECTION_NAME = "Balance";
        public static readonly string _PK = "/pk";


        public static readonly string _DB_NAME = R.DB_NAME;
        public static readonly Uri _URI_DB = UriFactory.CreateDatabaseUri(_DB_NAME);
        public static readonly Uri _URI_COL = UriFactory.CreateDocumentCollectionUri(_DB_NAME, _COLLECTION_NAME);

        public static readonly DocumentCollection _SPEC = new DocumentCollection
        {
            Id = _COLLECTION_NAME,
            IndexingPolicy = new IndexingPolicy(
                new RangeIndex(DataType.String) { Precision = -1 }
            )
            {
                IndexingMode = IndexingMode.Consistent,
                IncludedPaths = new Collection<IncludedPath>{
                    new IncludedPath {
                        Path = "/pk/*",
                        Indexes = new Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) { Precision = -1 }
                        }
                    },
                    new IncludedPath {
                        Path = "/userId/currency/*",
                        Indexes = new Collection<Index>()
                        {
                            new HashIndex(DataType.String) { Precision = -1 }
                        }
                    },
                },
                ExcludedPaths = new Collection<ExcludedPath>{
                    new ExcludedPath {
                        Path = "/*"
                    }
                }
            },
            UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys = new Collection<UniqueKey>
                {
                    new UniqueKey { Paths = new Collection<string> { "/userId/currency" }},
                }
            },
            PartitionKey = new PartitionKeyDefinition
            {
                Paths = new Collection<string> { _PK }
            }
        };

    }


    public partial class BalanceV2 // for StoredProcedure
    {
        public static readonly StoredProcedure _SP_Transaction = new StoredProcedure
        {
            Id = "Transaction",
            Body = File.ReadAllText(@"./Core/Sproc/Balance/Transaction.js")
        };
        public static readonly Uri _URI_Transaction = UriFactory.CreateStoredProcedureUri(_DB_NAME, _COLLECTION_NAME, _SP_Transaction.Id);

        public static readonly List<StoredProcedure> _SPROCs = new List<StoredProcedure>{
            _SP_Transaction
        };
    }
}
