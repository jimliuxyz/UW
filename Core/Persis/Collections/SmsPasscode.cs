
using System;
using System.Collections.ObjectModel;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace UW.Core.Persis.Collections
{
    //todo : 預計ttl維持24H 當genCount>=3 且 attemptCount>=3時 則待清除後才能被reset,再度使用
    public partial class SmsPasscode : Resource
    {
        public string id { get; set; }
        public string pk { get; set; }
        public string phoneno { get; set; }
        public string passcode { get; set; }

        public int resendCount { get; set; }   // passcode連續產生的次數
        public int verifyCount { get; set; }   // 當下passcode被嘗試的次數
        public DateTime verifyAvailTime { get; set; }
    }


    public partial class SmsPasscode
    {
        public static readonly string _COLLECTION_NAME = "SmsPasscode";
        public static readonly string _PK = "/pk";


        public static readonly string _DB_NAME = R.DB_NAME;
        public static readonly Uri _URI_DB = UriFactory.CreateDatabaseUri(_DB_NAME);
        public static readonly Uri _URI_COL = UriFactory.CreateDocumentCollectionUri(_DB_NAME, _COLLECTION_NAME);

        public static readonly DocumentCollection _SPEC = new DocumentCollection
        {
            Id = _COLLECTION_NAME,
            DefaultTimeToLive = 60 * 60,    // 60min
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
                        Path = "/phoneno/*",
                        Indexes = new Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) { Precision = -1 }
                        }
                    },
                },
                ExcludedPaths = new Collection<ExcludedPath>{
                    new ExcludedPath {
                        Path = "/"
                    }
                }
            },
            UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys = new Collection<UniqueKey>
                {
                    new UniqueKey { Paths = new Collection<string> { "/phoneno" }},
                }
            },
            PartitionKey = new PartitionKeyDefinition
            {
                Paths = new Collection<string> { _PK }
            }
        };
    }

}