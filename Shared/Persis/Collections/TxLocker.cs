using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Shared.Persis.Collections
{
    public partial class TxLocker
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string memId { get; set; }
        public string pk { get; set; }
        public long time = DateTime.UtcNow.ToFileTimeUtc();
    }

    public partial class TxLocker
    {
        public static readonly string _COLLECTION_NAME = "TxLocker";
        public static readonly string _PK = "/pk";


        public static readonly string _DB_NAME = R.DB_NAME;
        public static readonly Uri _URI_DB = UriFactory.CreateDatabaseUri(_DB_NAME);
        public static readonly Uri _URI_COL = UriFactory.CreateDocumentCollectionUri(_DB_NAME, _COLLECTION_NAME);

        public static readonly DocumentCollection _SPEC = new DocumentCollection
        {
            Id = _COLLECTION_NAME,
            DefaultTimeToLive = 60,
            IndexingPolicy = new IndexingPolicy(
                new RangeIndex(DataType.String) { Precision = -1 }
            )
            {
                IndexingMode = IndexingMode.Consistent,
                IncludedPaths = new Collection<IncludedPath>{
                    new IncludedPath {
                        Path = "/time/*",
                        Indexes = new Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) { Precision = -1 }
                        }
                    }
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
                    new UniqueKey { Paths = new Collection<string> { "/memId" }},
                }
            },
            PartitionKey = new PartitionKeyDefinition
            {
                Paths = new Collection<string> { _PK }
            }
        };




    }
}
