using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Shared.Persis.Collections
{
    public partial class UserNoPK
    {
        [JsonProperty(PropertyName = "id")]
        public string userId { get; set; }

        public string pk { get; set; }

        public long createdTime { get; set; }
        public string alias { get; set; }   //Unique Key in whole collection

        public bool allowDiscover = true;
        public string name { get; set; }

        public string phoneno { get; set; }

        public string avatar { get; set; }
        public string tokenRnd { get; set; }    //deprecated
        public string jwtHash { get; set; }    //用來驗證會員最後建立的JWT

        public List<CurrencySettings> currencies { get; set; } = new List<CurrencySettings>();

        public NtfInfo ntfInfo { get; set; } = new NtfInfo();
    }

    public partial class UserNoPK
    {
        public static readonly string _COLLECTION_NAME = "User_NoPK";

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
                        Path = "/id/*",
                        Indexes = new Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) { Precision = -1 }
                        }
                    },
                    new IncludedPath {
                        Path = "/createdTime/*",
                        Indexes = new Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) { Precision = -1 }
                        }
                    },
                    new IncludedPath {
                        Path = "/alias/*",
                        Indexes = new Collection<Index>()
                        {
                            new HashIndex(DataType.String) { Precision = -1 }
                        }
                    },
                    new IncludedPath {
                        Path = "/allowDiscover/*",
                        Indexes = new Collection<Index>()
                        {
                            new HashIndex(DataType.String) { Precision = -1 }
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
                    new UniqueKey { Paths = new Collection<string> { "/alias" }},
                    new UniqueKey { Paths = new Collection<string> { "/phoneno" }},
                }
            },
            PartitionKey = new PartitionKeyDefinition
            {
            }
        };
    }

}
